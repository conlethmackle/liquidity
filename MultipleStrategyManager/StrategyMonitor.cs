using Common;
using Common.Messages;
using Common.Models.DTOs;
using DataStore;
using MessageBroker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Common.Models;

namespace MultipleStrategyManager
{
   public interface IStrategyMonitor
   {
      Task StartStrategy(StartStrategyData? data);
      Task StopStrategy(StopStrategyData? data);
   }
    
   public class StrategyMonitor : IStrategyMonitor
   {
      private readonly ILogger<StrategyMonitor> _logger;
      private readonly IPortfolioRepository _portfolioRepository;
      private readonly IMessageBroker _messageBroker;
      private string _rootPath { get; }
      private string _binaryPath { get; }
      private string _program { get; }

      private List<StrategyProcessConfig> _processConfig { get; set; } = new List<StrategyProcessConfig>();
      Dictionary<string, Process> _processTable { get; set; } = new Dictionary<string, Process>();
      private Dictionary<int, Tuple<string, string>> _processIdToNameTable { get; set; } = new Dictionary<int, Tuple<string, string>>();

      public StrategyMonitor(ILoggerFactory loggerFactory, IPortfolioRepository portfolioRepository, IConfiguration config, IMessageBroker messsageBroker )
      {
         _logger = loggerFactory.CreateLogger<StrategyMonitor>();
         _portfolioRepository = portfolioRepository;
         _rootPath = config["RootPath"];
         _binaryPath = config["PathToBinary"];
         _program = config["ProgramName"];
         _messageBroker = messsageBroker;
      }

      public async Task StartStrategy(StartStrategyData? data)
      {
         try
         {
            if (data != null)
            {
               var configData = await _portfolioRepository.GetOpeningLiquidationSubscriptionForStrategySPSubscriptionId(data.StrategyConfigId);
               var accountName = configData.SP.Name;
               var strategyName = configData.StrategySPSubscriptionConfig.Strategy.StrategyName;
               var configName = configData.StrategySPSubscriptionConfig.ConfigName;
               CreateNewProc(accountName, strategyName, configName);
               // Todo - figure how to detect which process has failed
            }
            else
            {
               _logger.LogWarning("No Liquidation Strategy Config for Id {Id}", data?.StrategyConfigId);
            }
         }
         catch(Exception e)
         {
            _logger.LogWarning(e, "Error reading Liquidation Strategy Config for Id = {Id} -  {Error}", data?.StrategyConfigId, e.Message);
         }
      }

      private void CreateNewProc(string accountName, string strategyName, string configName)
      {
         var process = new StrategyProcessConfig()
         {
            AccountName = accountName,
            StrategyName = strategyName,
            ConfigName = configName,
            FileLogging = "On",
            ConsoleLogging = "Off"
         };

         if (!_processTable.ContainsKey(configName))
         {

            var path = _rootPath + _program + _binaryPath + _program + ".exe";
            var arguments = FormatArgumentString(process);
            var startInfo = new ProcessStartInfo()
            {
               Arguments = arguments,
               RedirectStandardOutput = false,
               FileName = path,
            };
            var proc = Process.Start(startInfo);
            if (proc != null)
            {
               proc.EnableRaisingEvents = true;
               proc.Exited += new EventHandler((proc, e) => ProcessExited(proc, e));

               _processTable.Add(process.ConfigName, proc);
               _processIdToNameTable.Add(proc.Id, new Tuple<string, string>(process.ConfigName, accountName));
            }

            _logger.LogInformation($"Started Strategy {process.ConfigName} process with process{proc.Id}");
            SendSuccessfulStartAck(accountName, configName);
         }
         else
         {
            _logger.LogInformation("Process for {Config} already started", configName);
         }
      }

      private void SendSuccessfulStartAck(string accountName, string configName)
      {
         var data = new StrategyControlResponse()
         {
            ConfigName = configName,
            Message = $"{configName} strategy successfully started",
            State = StrategyProcessStatus.STARTED
         };

         var msgResponse = new MessageBusReponse()
         {
            AccountName = accountName,
            FromVenue = Constants.MULTI_STRATEGY_MANAGER,
            IsPrivate = true,
            ResponseType = ResponseTypeEnums.STRATEGY_CONTROL_RESPONSE,
            Data = JsonSerializer.Serialize(data),
            Success = true
         };

         var topic = accountName + "." + configName + "." + Constants.STRATEGY_CONTROL_RESPONSE;
         PublishHelper.Publish(topic, msgResponse, _messageBroker);  
      }

      private void ProcessExited(object? sender, EventArgs e)
      {
         var process = (Process)sender;
         // Get the process name
         if (_processIdToNameTable.ContainsKey(process.Id))
         {
            _logger.LogError("Strategy Process {Strategy} has exited", _processIdToNameTable[process.Id].Item1);
            var stopData = new StopStrategyData()
            {
               AccountName = _processIdToNameTable[process.Id].Item2,
               InstanceName = _processIdToNameTable[process.Id].Item1
            };
            SendProcessExitedAck(stopData);
         }
      }

      private string FormatArgumentString(StrategyProcessConfig process)
      {
         return $" Account={process.AccountName} Strategy={process.StrategyName} ConfigName={process.ConfigName} FileLogging={process.FileLogging} ConsoleLogging={process.ConsoleLogging}";
      }

      public async Task StopStrategy(StopStrategyData? data)
      {

         if (_processTable.ContainsKey(data.InstanceName))
         {
            var storedProcess = _processTable[data.InstanceName];
            storedProcess.Kill();
            _processTable.Remove(data.InstanceName);
            _processConfig.Remove(_processConfig.FirstOrDefault(i => i.ConfigName.Equals(data.InstanceName)));
            if (_processIdToNameTable.ContainsKey(storedProcess.Id))
            {
               var nameData = _processIdToNameTable[storedProcess.Id];
               data.AccountName = nameData.Item2;
               SendSuccessfulStopAck(data);
            }
         }
         
      }

      private void SendSuccessfulStopAck(StopStrategyData strategyData)
      {
         var rspData = new StrategyControlResponse()
         {
            ConfigName = strategyData.InstanceName,
            Message = $"{strategyData.InstanceName} strategy successfully stopped",
            State = StrategyProcessStatus.STOPPED
         };

         var msgResponse = new MessageBusReponse()
         {
            AccountName = strategyData.AccountName,
            FromVenue = Constants.MULTI_STRATEGY_MANAGER,
            IsPrivate = true,
            ResponseType = ResponseTypeEnums.STRATEGY_CONTROL_RESPONSE,
            Data = JsonSerializer.Serialize(rspData),
            Success = true
         };

         var topic = strategyData.AccountName + "." + strategyData.InstanceName + "." +
                     Constants.STRATEGY_CONTROL_RESPONSE;
         PublishHelper.Publish(topic, msgResponse, _messageBroker);
      }

      private void SendProcessExitedAck(StopStrategyData strategyData)
      {
         var rspData = new StrategyControlResponse()
         {
            ConfigName = strategyData.InstanceName,
            Message = $"{strategyData.InstanceName} strategy unexpectedly exited",
            State = StrategyProcessStatus.UNEXPECTED_STOP
         };

         var msgResponse = new MessageBusReponse()
         {
            AccountName = strategyData.AccountName,
            FromVenue = Constants.MULTI_STRATEGY_MANAGER,
            IsPrivate = true,
            ResponseType = ResponseTypeEnums.STRATEGY_CONTROL_RESPONSE,
            Data = JsonSerializer.Serialize(rspData),
            Success = true
         };

         var topic = strategyData.AccountName + "." + strategyData.InstanceName + "." +
                     Constants.STRATEGY_CONTROL_RESPONSE;
         PublishHelper.Publish(topic, msgResponse, _messageBroker);
      }
   }
}
