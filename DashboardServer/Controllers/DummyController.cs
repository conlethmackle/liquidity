using DashboardServer.Sender;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace DashboardServer.Controllers
{
   [Route("api/[controller]")]
   [ApiController]
   public class DummyController : ControllerBase
   {
      public DummyController(IFakeOrderBookSender fakeOrderBook)
      {

      }

      [HttpGet]
      [Route("/dummy")]
      public void DoSomething()
      {
         Console.WriteLine("In here");

      }
   }
}
