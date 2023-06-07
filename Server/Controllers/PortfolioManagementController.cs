using AutoMapper;
using Common.Models.DTOs;
using Common.Models.Entities;
using Common.Extensions;
using DataStore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using SyncfusionLiquidity.Server.Hubs;

namespace PortfolioManagementAPI.Controllers
{
   [Route("api/[controller]")]
   [ApiController]
   public class PortfolioManagementController : ControllerBase
   {
      private readonly ILogger<PortfolioManagementController> _logger;
      private readonly IPortfolioRepository _repository;
      private readonly IMapper _mapper;
     
      public PortfolioManagementController(ILoggerFactory loggerFactory, IPortfolioRepository repository, IMapper mapper
                                           )
      {
         _logger = loggerFactory.CreateLogger<PortfolioManagementController>();
         _repository = repository;
         _mapper = mapper;
       
      }

      [HttpGet]
      [Route("portfolios")]
      [ActionName("getPortfolios")]
      public async Task<ActionResult<IEnumerable<SPDTO>>> GetPortfolios()
      {
         var dtoList = new List<SPDTO>();
         var sps = await _repository.GetPortfolios();
         sps.ForEach(sp => dtoList.Add(_mapper.Map<SPDTO>(sp)));

         return Ok(dtoList);
      }

      [HttpPost]
      [Route("portfolios")]
      public async Task<ActionResult<SPDTO>> CreatePortfolio(SPDTO portfolio)
      {
         try
         {
            if (portfolio == null)
               return BadRequest();

            var createdRepository = await _repository.CreatePortfolio(portfolio);
            var dto = _mapper.Map<SPDTO>(createdRepository);

            return CreatedAtAction("getPortfolios",
                new { id = createdRepository.SPId }, dto);
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Error creating new portfolio");
         }
      }
      [HttpGet]
      [Route("coins")]
      [ActionName("getCoins")]
      public async Task<ActionResult<IEnumerable<CoinDTO>>> GetCoins()
      {
         try
         {
            var dtoList = new List<CoinDTO>();
            var coins = await _repository.GetCoins();
            coins.ForEach(coin => dtoList.Add(_mapper.Map<CoinDTO>(coin)));
            return Ok(dtoList);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting coins from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Error getting coins");
         }
      }

      [HttpPost]
      [Route("coins")]
      public async Task<ActionResult<CoinDTO>> CreateCoin(CoinDTO coin)
      {
         try
         {
            if (coin == null)
               return BadRequest();

            var createdCoin = await _repository.AddCoin(coin);
            //   var dto = _mapper.Map<CoinDTO>(createdCoin);
            var dto = new CoinDTO()
            {
               CoinId = createdCoin.CoinId,
               Name = coin.Name
            };

            return CreatedAtAction("getCoins",
                new { id = createdCoin.CoinId }, dto);
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Error creating new coin");
         }
      }

      [HttpGet]
      [Route("coinpairs/get")]
      [ActionName("getCoinPairs")]
      public async Task<ActionResult<IEnumerable<CoinPairDTO>>> GetCoinPairs()
      {
         try
         {
            var dtoList = new List<CoinPairDTO>();
            var coinpairs = await _repository.GetCoinPairs();
            coinpairs.ForEach(coinpair => dtoList.Add(_mapper.Map<CoinPairDTO>(coinpair)));
            return Ok(dtoList);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting coinpairs from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Error creating getting coinpairs"); ;
         }
      }

      [HttpPost]
      [Route("coinpairs")]
      public async Task<ActionResult<CoinPairDTO>> CreateCoinPair(CoinPairDTO coinpair)
      {
         try
         {
            if (coinpair == null)
               return BadRequest();

            var createdCoinPair = await _repository.AddCoinPair(coinpair);
            var dto = _mapper.Map<CoinPairDTO>(createdCoinPair);

            return CreatedAtAction("getCoinPairs",
                new { id = createdCoinPair.CoinPairId }, dto);
         }
         catch (Exception)
         {
            _logger.LogError("Error creating new coinpair {CoinPair}", coinpair.Name);
            return StatusCode(StatusCodes.Status500InternalServerError,
                $"Error creating new coinpair {coinpair.Name}");
         }
      }

      [HttpPost]
      [Route("venues")]
      public async Task<ActionResult<VenueDTO>> CreateVenue(VenueDTO venueDTO)
      {
         try
         {
            if (!string.IsNullOrEmpty(venueDTO.VenueName))
            {
               var venue = await _repository.AddVenue(venueDTO);
               var dto = _mapper.Map<VenueDTO>(venue);
               return CreatedAtAction("getVenues",
                  new { id = venue.VenueId }, dto);
            }
            return BadRequest();
         }
         catch (Exception e)
         {
            _logger.LogError("Error creating new venue {Venue}", venueDTO.VenueName);
            return StatusCode(StatusCodes.Status500InternalServerError,
                $"Error creating new venue {venueDTO.VenueName}");
         }
      }

      [HttpGet]
      [Route("venues")]
      [ActionName("getVenues")]
      public async Task<ActionResult<IEnumerable<VenueDTO>>> GetVenues()
      {
         try
         {
            var dtoList = new List<VenueDTO>();
            var venues = await _repository.GetVenues();
            venues.ForEach(venue => dtoList.Add(_mapper.Map<VenueDTO>(venue)));
            return Ok(dtoList);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting venues from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Error creating getting venue"); ;
         }
      }



      [HttpGet]
      [Route("exchangecoinpairs")]
      [ActionName("getCoinPairs")]
      public async Task<ActionResult<IEnumerable<ExchangeCoinpairLookupDTO>>> GetExchangeCoinPairLookups()
      {
         try
         {
            var dtoList = new List<ExchangeCoinpairLookupDTO>();
            var coinpairs = await _repository.GetExchangeCoinPairLookups();
            coinpairs.ForEach(coinpair => dtoList.Add(_mapper.Map<ExchangeCoinpairLookupDTO>(coinpair)));
            return Ok(dtoList);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting exchange coinpairs from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Error creating getting coinpairs"); ;
         }
      }

      [HttpPost]
      [Route("exchangecoinpairs")]
      public async Task<ActionResult<CoinPairDTO>> CreateExchangeCoinPairLookup(ExchangeCoinpairLookupDTO coinpair)
      {
         try
         {
            if (coinpair == null)
               return BadRequest();

            var createdCoinPair = await _repository.AddExchangeCoinPairLookup(coinpair);


            return CreatedAtAction("getCoinPairs",
                new { id = createdCoinPair.ExchangeCoinpairLookupId }, createdCoinPair);
         }
         catch (Exception)
         {
            _logger.LogError("Error creating new coinpair exchange lookup {CoinPair} for exchnage {Exchange}", coinpair.ExchangeCoinpairName, coinpair.ExchangeName);
            return StatusCode(StatusCodes.Status500InternalServerError,
                $"Error creating new exchange coinpair lookup {coinpair.ExchangeCoinpairName}");
         }
      }


   }
}