using Common.Models.DTOs;
using DataStore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorLiquidity.Server.Controllers
{
   [Route("api/[controller]")]
   [ApiController]
   [Authorize()]
   public class MakerTakerFeeController : ControllerBase
   {
      private readonly ILogger<FairValueConfigUIController> _logger;
      private readonly IPortfolioRepository _repository;
      public MakerTakerFeeController(ILoggerFactory loggerFactory,
                                         IPortfolioRepository repository)
      {
         _logger = loggerFactory.CreateLogger<FairValueConfigUIController>();
         _repository = repository;
      }

      [HttpPost("create")]
      
      public async Task<ActionResult<MakerTakerFeeDTO>> CreateMakerTakerFee(MakerTakerFeeDTO makerTakerFee)
      {
         try
         {
            if (makerTakerFee == null)
               return BadRequest();
            var result = await _repository.CreateMakerTakerFee(makerTakerFee);
            return CreatedAtAction("getMakerTakerFees",
               new { id = result.Id }, result);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error in CreateMakerTakerFee    {Instance} {Error}", makerTakerFee.Venue.VenueName, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error in CreateMakerTakerFee for {makerTakerFee.Venue.VenueName}");
         }
      }

      [HttpGet]
      [Route("get")]
      [ActionName("getMakerTakerFees")]
      public async Task<ActionResult<IEnumerable<MakerTakerFeeDTO>>> GetMakerTakerFees()
      {
         try
         {
            var makerTakerFees = await _repository.GetMakerTakerFees();
            return Ok(makerTakerFees);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting GetMakerTakerFees from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error getting GetMakerTakerFees from db"); ;
         }
      }

      [HttpPut]
      [Route("update")]
      
      public async Task<ActionResult> UpdateMakerTakerFees(MakerTakerFeeDTO makerTakerDTO)
      {
         try
         {
            if (makerTakerDTO == null)
               return BadRequest();

            await _repository.UpdateMakerTakerFees(makerTakerDTO);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error updating makerTakerFees ");
         }
      }

      [HttpDelete]
      [Route("delete")]
    
      public async Task<ActionResult> DeleteFairValueConfig(int Id)
      {
         try
         {
            await _repository.DeleteMakerTakerFees(Id);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error deleting DeleteMakerTakerFees {Id}");
         }
      }
   }
}
