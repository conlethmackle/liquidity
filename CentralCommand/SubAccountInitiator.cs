using Common;
using Common.Messages;
using DataStore;
using MessageBroker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CentralCommand
{
   public interface ISubAccountInitiator
   {
      Task Init();
   }

   public class SubAccountInitiator : ISubAccountInitiator
   {
      private readonly ILogger<SubAccountInitiator> _logger;
      private readonly LiquidityDbContext _context;
      private readonly IMessageBroker _messageBroker;

      public SubAccountInitiator(ILoggerFactory loggerFactory, LiquidityDbContext liquidityDbContext, IMessageBroker messageBroker)
      {
         _logger = loggerFactory.CreateLogger<SubAccountInitiator>();
       
         _context = liquidityDbContext;
         if (_context == null)
         {
            throw new ArgumentNullException(nameof(liquidityDbContext));
         }
         _messageBroker = messageBroker;       
      }

      public async Task Init()
      {

         //   var sps = await _context.SPs.Include(e => e.Exchanges).ThenInclude(a => a.ApiKey).ToListAsync();
         var sps = await _context.SPs.ToListAsync();
         foreach (var sp in sps)
         {
            if (sp.IsEnabled)
            {
            /*   var exchangeDetails = sp.Exchanges.ToList();
               foreach(var exchange in exchangeDetails)
               {
                  if (exchange.IsEnabled)
                  {
                     var venue = await _context.Venues.Where(v  => v.VenueId == exchange.VenueId).FirstOrDefaultAsync();
                     if (venue != null)
                     {
                        var apikey = exchange.ApiKey;
                        var connectDetails = new PrivateConnectionLogon()
                        {
                           SPId = sp.SPId,
                           SPName = sp.Name,
                           ExchangeDetailsId = exchange.ExchangeDetailsId,
                           ApiKey = apikey.Key,                           
                           PassPhrase = apikey.PassPhrase,
                           Secret = apikey.Secret,
                        };

                        var data = JsonSerializer.Serialize(connectDetails);

                        var privateConnect = new MessageBusCommand()
                        {
                           AccountName = sp.Name,
                           CommandType = CommandTypesEnum.CONNECT_PRIVATE,
                           Exchange = venue.VenueName,
                           Data = data,
                           IsPrivate = true
                        };

                        var bytesRef = MessageBusCommand.ProtoSerialize(privateConnect);
                        _logger.LogInformation("Connecting {SP} to {Exchange} Exchange", sp.Name, venue.VenueName);
                        _messageBroker.PublishToSubject(venue.VenueName, bytesRef);
                     }
                  }
               } */
            }
         }        
      }
   }
}
