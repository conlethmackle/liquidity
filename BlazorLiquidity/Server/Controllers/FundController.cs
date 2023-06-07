using Common.Messages;
using Common.Models.DTOs;
using DataStore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlazorLiquidity.Server.Controllers
{
   [Authorize()]
   [Route("api/[controller]")]
   [ApiController]
   public class FundController : ControllerBase
   {
      private readonly ILogger<FundController> _logger;
      private readonly IPortfolioRepository _repository;
      

      public FundController(ILoggerFactory loggerFactory,
         IPortfolioRepository repository)
      {
         _logger = loggerFactory.CreateLogger<FundController>();
         _repository = repository;
         
      }

      [HttpPost]
      [Route("create_fund")]
      //   [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult<FundDTO>> CreateFund(FundDTO fundDTO)
      {
         try
         {
            if (fundDTO == null)
               return BadRequest();

            var createdFund = await _repository.CreateFund(fundDTO);
         

            return CreatedAtAction("get_funds",
               new { id = createdFund.FundId }, createdFund);
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error creating CreateFund");
         }
      }

      [HttpGet]
      [Route("get_funds")]
      [ActionName("get_funds")]
      //     [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult<FundDTO>> GetFunds()
      {
         try
         {
            var funds = await _repository.GetFunds();
            return Ok(funds);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting  funds from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error getting  funds");
         }
      }

      [HttpGet]
      [Route("get_fundById")]
      [ActionName("get_fundById")]
      //     [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult<FundDTO>> GetFundById(int id)
      {
         try
         {
            var fund = await _repository.GetFundById(id);
            if (fund == null)
               return NotFound();
            return Ok(fund);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting  GetFundById from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error getting  funds");
         }
      }

      [HttpPut]
      [Route("update_fund")]
      //     [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult> UpdateFund(FundDTO fundDTO)
      {
         try
         {
            if (fundDTO == null)
               return BadRequest();
            await _repository.UpdateFund(fundDTO);
            return Ok();
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error updating  fund to db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error updating  funds");
         }
      }

      [HttpDelete]
      [Route("delete_fund")]
      //      [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult> DeleteFund(int Id)
      {
         try
         {
            await _repository.DeleteFund(Id);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error deleting fund {Id}");
         }
      }

      [HttpPost]
      [Route("create_location")]
      //   [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult<LocationDTO>> CreateLocation(LocationDTO locationDTO)
      {
         try
         {
            if (locationDTO == null)
               return BadRequest();

            var createdLocation = await _repository.CreateLocation(locationDTO);
           

            return CreatedAtAction("get_locations",
               new { id = createdLocation.LocationId }, createdLocation);
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error creating  Location");
         }
      }

      [HttpGet]
      [Route("get_locations")]
      [ActionName("get_locations")]
      //     [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult<LocationDTO>> GetLocations()
      {
         try
         {
            var locations = await _repository.GetLocations();
            return Ok(locations);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error getting telegram locations from db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error getting telegram locations");
         }
      }

      [HttpPut]
      [Route("update_location")]
      //     [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult> UpdateLocation(LocationDTO locationDTO)
      {
         try
         {
            if (locationDTO == null)
               return BadRequest();
            await _repository.UpdateLocation(locationDTO);
            
            return Ok();
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error updating telegram location to db {Error} - {Stacktrace}", e.Message, e.StackTrace);
            return StatusCode(StatusCodes.Status500InternalServerError,
               "Error updating telegram locations");
         }
      }

      [HttpDelete]
      [Route("delete_location")]
      //      [Authorize(Roles = "LiquidityCommands")]
      public async Task<ActionResult> DeleteLocation(int Id)
      {
         try
         {
            await _repository.DeleteLocation(Id);
            return Ok();
         }
         catch (Exception)
         {
            return StatusCode(StatusCodes.Status500InternalServerError,
               $"Error deleting  location {Id}");
         }
      }


   }
}
