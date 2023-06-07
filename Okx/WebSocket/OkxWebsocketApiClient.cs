using Common.Messages;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Timers;
using Okx.Models;
using Okx.Config;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Okx.WebSocket
{
   public interface IOkxWebsocketApiClient
   {
      Task ConnectPrivate();
      Task ConnectPublic();
      Task DisconnectPrivate(string? description);
      Task DisconnectPublic(string? description);
      bool IsPrivateConnected();
      bool IsPublicConnected();
      void RunPublic(Func<string, Task> publicAction);
      void RunPrivate(Func<string, Task> privateAction);
      Task SendPrivate(string data);
      Task SendPublic(string data);
      void InitPrivate(PrivateConnectionLogon accountsConfig);
      void InitPublic();
   }

   public class OkxWebsocketApiClient : IOkxWebsocketApiClient
   {
      private string _privateConnection { get; set; }
      private string _publicConnection { get; set; }
      private ClientWebSocket _privateSocket { get; set; }
      private ClientWebSocket _publicSocket { get; set; }
      private ILogger<OkxWebsocketApiClient> _logger { get; set; }
      private PrivateConnectionLogon _privateConnectionLogon { get; set; }

      private System.Timers.Timer _timer;
      private const uint _retryInterval = 30000;
      private bool _restartInProgress = false;
      private bool _isPrivateConnected = false;
      private bool _isPublicConnected = false;
      private Thread _processingPrivateThread { get; set; }
      private Thread _processingPublicThread { get; set; }
      private bool _stopProcessingPrivateThread = false;
      private bool _stopProcessingPublicThread = false;
      private readonly OkxConnectionConfig _config; 

      public OkxWebsocketApiClient(ILoggerFactory loggerFactory, IOptions<OkxConnectionConfig> configData)
      {
         _logger = loggerFactory.CreateLogger<OkxWebsocketApiClient>();
         _privateSocket = new ClientWebSocket();
         _publicSocket = new ClientWebSocket();
         _timer = new System.Timers.Timer();
         _config = configData.Value;
      }

      public void InitPrivate(PrivateConnectionLogon accountsConfig)
      {
         _privateConnection = _config.PrivateWebSocketEndpoint;
         _privateConnectionLogon = accountsConfig;
      }

      public void InitPublic()
      {
         _publicConnection = _config.PublicWebSocketEndpoint;
      }


      public async Task ConnectPrivate()
      {
         try
         {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            _privateSocket.Options.RemoteCertificateValidationCallback = delegate { return true; };
            if (_privateSocket.State != WebSocketState.Open && _privateSocket.State != WebSocketState.Connecting)
            {
               await _privateSocket.ConnectAsync(new Uri(_privateConnection), CancellationToken.None);
               string logonMsg = ConstructLogonMessage();
               await SendPrivate(logonMsg);
            }

            _isPrivateConnected = true;

         }
         catch (Exception ex)
         {
            _isPrivateConnected = false;
            _logger.LogError("ERROR - {Message}", ex.Message);
            RestartPrivateSocket();
         }
      }

      private string ConstructLogonMessage()
      {
         DateTime currentTime = DateTime.UtcNow;
         long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
         var body = unixTime +  "GET" + "/users/self/verify";
         var signature = Convert.ToBase64String(HashHMAC(new UTF8Encoding().GetBytes(_privateConnectionLogon.Secret),
                  new UTF8Encoding().GetBytes(body)));

         var loginMsg = new OkxLogin()
         {
            ApiKey = _privateConnectionLogon.ApiKey,
            PassPhrase = _privateConnectionLogon.PassPhrase,
            Sign = signature,
            Timestamp = unixTime.ToString(),
         };

         OkxWebsocketRequestData<OkxLogin> loginOp = new OkxWebsocketRequestData<OkxLogin>()
         {
            Operation = "login",
            Args = new OkxLogin[1]
         };

         loginOp.Args[0] = loginMsg;

         return JsonSerializer.Serialize(loginOp);         
      }

      private static byte[] HashHMAC(byte[] key, byte[] message)
      {
         var hash = new HMACSHA256(key);
         return hash.ComputeHash(message);
      }

      public async Task ConnectPublic()
      {
         try
         {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            _publicSocket.Options.RemoteCertificateValidationCallback = delegate { return true; };
            if (_publicSocket.State != WebSocketState.Open && _publicSocket.State != WebSocketState.Connecting)
               await _publicSocket.ConnectAsync(new Uri(_publicConnection), CancellationToken.None);
            _isPublicConnected = true;

         }
         catch (Exception ex)
         {
            _isPublicConnected = false;
            _logger.LogError("ERROR - {Message}", ex.Message);
            RestartPublicSocket();
         }
      }

      public async Task DisconnectPrivate(string? description)
      {
         try
         {
            if (_isPrivateConnected)
            {
               // TODO - might have other instances of closure than just normal             
               if (_privateSocket.State == WebSocketState.Open || _privateSocket.State == WebSocketState.Connecting)
                  await _privateSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, description, CancellationToken.None);
               _isPrivateConnected = false;
            }
         }
         catch (Exception ex)
         {
            _logger.LogError("DisconnectPrivate - ERROR - {Message}", ex.Message);
            // TODO Maybe need to retry??????
            throw;

         }
      }

      public async Task DisconnectPublic(string? description)
      {
         try
         {
            if (_isPublicConnected)
            {
               // TODO - might have other instances of closure than just normal             
               if (_publicSocket.State == WebSocketState.Open || _publicSocket.State == WebSocketState.Connecting)
                  await _publicSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, description, CancellationToken.None);
               _isPublicConnected = false;
            }
         }
         catch (Exception ex)
         {
            _logger.LogError("DisconnectPrivate - ERROR - {Message}", ex.Message);
            // TODO Maybe need to retry??????
            throw;

         }
      }

      public bool IsPrivateConnected()
      {
         return _isPrivateConnected;
      }

      public bool IsPublicConnected()
      {
         return _isPublicConnected;
      }

      public void RunPublic(Func<string, Task> publicAction)
      {
         _stopProcessingPublicThread = false;
         var publicThread = new Thread(async () =>
            await ReceivePublicMessages(publicAction));
         publicThread.Start();
      }

      public void RunPrivate(Func<string, Task> privateAction)
      {
         _stopProcessingPrivateThread = false;
         var privateThread = new Thread(async () =>
         await ReceivePrivateMessages(privateAction));
         privateThread.Start();
      }

      private async Task ReceivePrivateMessages(Func<string, Task> action)
      {
         try
         {
            await Receive(action, _privateSocket, _stopProcessingPrivateThread);
         }
         catch (Exception ex)
         {
            // _logger.LogError($"ERROR - {ex.Message}");

            var state = _privateSocket.State;
            if (state == WebSocketState.Aborted || state == WebSocketState.Closed)
            {
               _logger.LogError(ex, "Private Websocket has been closed - going to try a reconnect {Error}", ex.Message);
               _isPrivateConnected = true;
               await action("CONNECTION DOWN");
               _stopProcessingPrivateThread = true;
            }
         }
      }

      private async Task ReceivePublicMessages(Func<string, Task> action)
      {
         try
         {
            await Receive(action, _publicSocket, _stopProcessingPublicThread);
         }
         catch (Exception ex)
         {
            // _logger.LogError($"ERROR - {ex.Message}");

            var state = _publicSocket.State;
            if (state == WebSocketState.Aborted || state == WebSocketState.Closed)
            {
               _logger.LogError(ex, "Public Websocket has been closed - going to try a reconnect");
               _isPublicConnected = true;
               await action("CONNECTION DOWN");
               _stopProcessingPublicThread = true;
            }
         }
      }

      private void RestartPrivateSocket()
      {
         //WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure;

         if (_privateSocket.State == WebSocketState.Aborted || _privateSocket.State == WebSocketState.Closed || _privateSocket.State == WebSocketState.None)
         {
            _privateSocket = new ClientWebSocket();
         }
         // _restartInProgress = true;
         //  _timer.Elapsed += OnRestartTimer;
         //  _timer.Interval = _retryInterval;
         //  _timer.Enabled = true;
      }

      private void RestartPublicSocket()
      {
         if (_privateSocket.State == WebSocketState.Aborted || _privateSocket.State == WebSocketState.Closed || _privateSocket.State == WebSocketState.None)
         {
            _privateSocket = new ClientWebSocket();
         }
      }

      private async void OnRestartTimer(object o, ElapsedEventArgs e)
      {
         _timer.Enabled = false;
         await ConnectPrivate();
      }

      public async Task SendPrivate(string data)
      {
         await SendCommon(data, _privateSocket, _isPrivateConnected);
      }

      public async Task SendPublic(string data)
      {
         await SendCommon(data, _publicSocket, _isPrivateConnected);
      }

      private async Task SendCommon(string data, ClientWebSocket socket, bool connectedFlag)
      {
         try
         {
            if (!_restartInProgress)
               await socket.SendAsync(Encoding.UTF8.GetBytes(data), WebSocketMessageType.Text, true, CancellationToken.None);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error in Send to Websocket {Exception}", e.Message);
            connectedFlag = false;
            throw;
            //         RestartSocket();
         }
      }

      private async Task Receive(Func<string, Task> action, ClientWebSocket websocket, bool processThread)
      {
         var buffer = new ArraySegment<byte>(new byte[2048]);
         while (!processThread)
         {
            WebSocketReceiveResult result;
            using (var ms = new MemoryStream())
            {
               do
               {
                  result = await websocket.ReceiveAsync(buffer, CancellationToken.None);
                  ms.Write(buffer.Array, buffer.Offset, result.Count);
               } while (!result.EndOfMessage);

               if (result.MessageType == WebSocketMessageType.Close)
                  break;

               ms.Seek(0, SeekOrigin.Begin);
               using (var reader = new StreamReader(ms, Encoding.UTF8))
               {
                  await action(await reader.ReadToEndAsync());
               }
            }
         }
      }
   }
}
