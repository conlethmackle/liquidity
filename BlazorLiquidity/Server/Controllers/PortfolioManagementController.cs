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
using BlazorLiquidity.Server.Hubs;
using Microsoft.AspNetCore.Authorization;

namespace PortfolioManagementAPI.Controllers
{
   [Route("api/[controller]")]
   [ApiController]
   [Authorize()]
   public class PortfolioManagementController : ControllerBase
   {
      private readonly ILogger<PortfolioManagementController> _logger;
      private readonly IPortfolioRepository _repository;
      private readonly IMapper _mapper;
     
      public PortfolioManagementController(ILoggerFactory loggerFactory, 
                                           IPortfolioRepository repository, 
                                           IMapper mapper
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
         try
         {
            var sps = await _repository.GetPortfolios();
            return Ok(sps);
         }
         catch (Exception e)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error getting portfolios - {e.Message}");
         }
         
      }

      [HttpGet]
      [Route("portfoliosGetByFundId")]
      [ActionName("getPortfolios")]
      public async Task<ActionResult<IEnumerable<SPDTO>>> GetPortfoliosByFundId(int FundId)
      {
         try
         {
            var sps = await _repository.GetPortfoliosByFundId(FundId);
            return Ok(sps);
         }
         catch (Exception e)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error getting portfolios - {e.Message}");
         }

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
         catch (Exception e)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
                $"Error creating new portfolio - {e.Message}");
         }
      }

      [HttpPut]
      [Route("portfolios/update")]
      public async Task<ActionResult> UpdatePortfolio(SPDTO sp)
      {
         try
         {
            if (sp == null)
               return BadRequest();

            await _repository.UpdatePortfolio(sp);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error creating Portfolio ");
         }
      }

      [HttpDelete]
      [Route("portfolios/deleteByName")]
      public async Task<ActionResult> DeletePortfolioByName(string name)
      {
         try
         {
            if (string.IsNullOrEmpty(name))
               return BadRequest();

            await _repository.DeletePortfolioByName(name);

            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error deleting Portfolio ");
         }
      }


      [HttpDelete] // This won't be easy if foreign key constraints considered
      [Route("portfolios/delete")]
      public async Task<ActionResult> DeletePortfolio(int Id)
      {
         try
         {
            await _repository.DeletePortfolio(Id);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error DeletePortfolio ");
         }
      }

      [HttpGet]
      [Route("coins/get")]
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
      [Route("coin/create")]
      [Authorize(Roles = "LiquidityCommands, Administrator")]
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

      [HttpDelete]
      [Route("coin/delete")]
      [Authorize(Roles = "LiquidityCommands, Administrator")]
      public async Task<ActionResult> DeleteCoin(int Id)
      {
         try
         {
            await _repository.DeleteCoin(Id);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error deleting ExchangeDetails");
         }
      }

      [HttpPut]
      [Route("coin/update")]
      [Authorize(Roles = "LiquidityCommands, Administrator")]
      public async Task<ActionResult> UpdateCoin(CoinDTO coin)
      {
         try
         {
            if (coin == null)
               return BadRequest();

            await _repository.UpdateCoin(coin);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error updating CoinPair ");
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
           
            return Ok(coinpairs);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting coinpairs from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Error creating getting coinpairs"); ;
         }
      }

      [HttpPost]
      [Route("coinpairs/create")]
      [Authorize(Roles = "LiquidityCommands, Administrator")]
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

      [HttpDelete]
      [Route("coinpairs/delete")]
      [Authorize(Roles = "LiquidityCommands, Administrator")]
      public async Task<ActionResult> DeleteCoinPair(int Id)
      {
         try
         {
            await _repository.DeleteCoinPair(Id);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error deleting CoinPair");
         }
      }

      [HttpPut]
      [Route("coinpair/update")]
      [Authorize(Roles = "LiquidityCommands, Administrator")]
      public async Task<ActionResult> UpdateCoinPair(CoinPairDTO coinPair)
      {
         try
         {
            if (coinPair == null)
               return BadRequest();

            await _repository.UpdateCoinPair(coinPair);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error updating CoinPair ");
         }
      }

      [HttpGet]
      [Route("exchangecoinpairs/get")]
      [ActionName("getExchangeCoinPairs")]
      
      public async Task<ActionResult<IEnumerable<ExchangeCoinpairMappingDTO>>> GetExchangeCoinPairLookups()
      {
         try
         {
            var dtoList = new List<ExchangeCoinpairMappingDTO>();
            var coinpairs = await _repository.GetExchangeCoinPairLookups();
            coinpairs.ForEach(coinpair => dtoList.Add(_mapper.Map<ExchangeCoinpairMappingDTO>(coinpair)));
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
      [Route("exchangecoinpairs/create")]
      [Authorize(Roles = "LiquidityCommands, Administrator")]
      public async Task<ActionResult<CoinPairDTO>> CreateExchangeCoinPairLookup(ExchangeCoinpairMappingDTO coinpair)
      {
         try
         {
            if (coinpair == null)
               return BadRequest();

            var createdCoinPair = await _repository.AddExchangeCoinPairLookup(coinpair);

            return CreatedAtAction("getExchangeCoinPairs",
                new { id = createdCoinPair.ExchangeCoinpairLookupId }, createdCoinPair);
         }
         catch (Exception e)
         {
            _logger.LogError("Error creating new coinpair exchange lookup {CoinPair} for exchnage {Exchange}", coinpair.ExchangeCoinpairName, coinpair.Venue.VenueName);
            return StatusCode(StatusCodes.Status500InternalServerError,
                $"Error creating new exchange coinpair lookup {coinpair.ExchangeCoinpairName}");
         }
      }

      [HttpPut]
      [Route("exchangecoinpairs/update")]
      [Authorize(Roles = "LiquidityCommands, Administrator")]
      public async Task<ActionResult> UpdateExchangeCoinPair(ExchangeCoinpairMappingDTO coinPair)
      {
         try
         {
            if (coinPair == null)
               return BadRequest();

            await _repository.UpdateExchangeCoinPair(coinPair);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error updating CoinPair ");
         }
      }

      [HttpDelete]
      [Authorize(Roles = "LiquidityCommands, Administrator")]
      [Route("exchangecoinpairs/delete")]
      public async Task<ActionResult> DeleteExchangeCoinPair(int Id)
      {
         try
         {
            await _repository.DeleteExchangeCoinPair(Id);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error deleting DeleteExchangeCoinPair");
         }
      }

      [HttpGet]
      [Route("exchangecoins/get")]
      [ActionName("getExchangeCoins")]
      public async Task<ActionResult<IEnumerable<ExchangeCoinMappingsDTO>>> GetExchangeCoinLookups()
      {
         try
         {
            var dtoList = new List<ExchangeCoinMappingsDTO>();
            var coinpairs = await _repository.GetExchangeCoinLookups();
            coinpairs.ForEach(coinpair => dtoList.Add(_mapper.Map<ExchangeCoinMappingsDTO>(coinpair)));
            return Ok(dtoList);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting exchange coins from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Error creating getting exchange coins"); ;
         }
      }

      [HttpPost]
      [Route("exchangecoins/create")]
      [Authorize(Roles = "LiquidityCommands, Administrator")]
      public async Task<ActionResult<CoinPairDTO>> CreateExchangeCoinLookup(ExchangeCoinMappingsDTO coin)
      {
         try
         {
            if (coin == null)
               return BadRequest();

            var createdCoinPair = await _repository.AddExchangeCoinLookup(coin);

            return CreatedAtAction("getExchangeCoins",
                new { id = createdCoinPair.Id }, createdCoinPair);
         }
         catch (Exception)
         {
            _logger.LogError("Error creating new coin exchange lookup {Coin} for exchange {Exchange}", coin.ExchangeCoinName, coin.Venue.VenueName);
            return StatusCode(StatusCodes.Status500InternalServerError,
                $"Error creating new exchange coin lookup {coin.ExchangeCoinName}");
         }
      }

      [HttpPut]
      [Route("exchangecoins/update")]
      [Authorize(Roles = "LiquidityCommands, Administrator")]
      public async Task<ActionResult> UpdateExchangeCoin(ExchangeCoinMappingsDTO coin)
      {
         try
         {
            if (coin == null)
               return BadRequest();

            await _repository.UpdateExchangeCoin(coin);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error updating CoinPair ");
         }
      }

      [HttpDelete]
      [Route("exchangecoins/delete")]
      [Authorize(Roles = "LiquidityCommands, Administrator")]
      public async Task<ActionResult> DeleteExchangeCoin(int Id)
      {
         try
         {
            await _repository.DeleteExchangeCoin(Id);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error deleting DeleteExchangeCoin");
         }
      }
   }
}