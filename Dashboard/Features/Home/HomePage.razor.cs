using Dashboard.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Dashboard.Features.Home
{
   public partial class HomePage
   {
      private List<SP> _sps;
      private SP _selectedSP;
      [Inject]
      public HttpClient Http { get; set; } = default!;

      protected override async Task OnInitializedAsync()
      {
         try
         {
            _sps = await Http.GetFromJsonAsync<List<SP>>("sample-data/sp.json");
         }
         catch (HttpRequestException ex)
         {           
            Console.WriteLine($"Problem loading data {ex.Message}");
         }
      }

      private void HandleAccountSelected(SP account)
      {
         _selectedSP = account;
      }


      


}
}
