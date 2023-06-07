using Common.Models.DTOs;
using DataStore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlazorLiquidity.Server.Controllers
{
   [Route("api/[controller]")]
   [ApiController]
   [Authorize()]
   public class FairValueConfigUIController : ControllerBase
   {
      private readonly ILogger<FairValueConfigUIController> _logger;
      private readonly IPortfolioRepository _repository;
      public FairValueConfigUIController(ILoggerFactory loggerFactory,
                                         IPortfolioRepository repository)
      {
         _logger = loggerFactory.CreateLogger<FairValueConfigUIController>();
         _repository = repository;
      }

      [HttpPost("create")]
      
      public async Task<ActionResult<FairValueConfigForUiDTO>> CreateFairValueConfigForUI(FairValueConfigForUiDTO fairValueConfig)
      {
         try
         {
            if (fairValueConfig == null)
               return BadRequest();
            var result = await _repository.CreateFairValueConfigForUI(fairValueConfig);
            return CreatedAtAction("getFairValueConfig",
               new { id = result.Id }, result);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error in CreateFairValueConfigForUI    {Instance} {Error}", fairValueConfig.CoinPair.Name, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error in CreateFairValueConfigForUI for {fairValueConfig.CoinPair.Name}");
         }
      }

      [HttpGet]
      [Route("get")]
      [ActionName("getFairValueConfig")]
      public async Task<ActionResult<IEnumerable<FairValueConfigForUiDTO>>> GetFairValueConfigs()
      {
         try
         {
            var fairValueConfigs = await _repository.GetFairValueConfigForUI();
            return Ok(fairValueConfigs);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting fair value configs from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error getting fair value configs from db"); ;
         }
      }

      [HttpPut]
      [Route("update")]
      
      public async Task<ActionResult> UpdateFairValueConfig(FairValueConfigForUiDTO fairValueConfig)
      {
         try
         {
            if (fairValueConfig == null)
               return BadRequest();

            await _repository.UpdateFairValueConfig(fairValueConfig);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error updating fairValueConfig ");
         }
      }

      [HttpDelete]
      [Route("delete")]
     
      public async Task<ActionResult> DeleteFairValueConfig(int Id)
      {
         try
         {
            await _repository.DeleteFairValueConfigForUI(Id);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error deleting DeleteFairValueConfig {Id}");
         }
      }
   }
}
