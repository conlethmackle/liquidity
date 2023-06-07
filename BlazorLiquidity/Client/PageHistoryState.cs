namespace BlazorLiquidity.Client
{
   public class PageHistoryState
   {
      private Stack<string> _previousPages;

      public PageHistoryState()
      {
         _previousPages = new Stack<string>();
      }
      public void AddPageToHistory(string pageName)
      {
         Console.WriteLine($"Adding the page {pageName} to history");
         _previousPages.Push(pageName);
      }

      public string GetGoBackPage()
      {
         if (_previousPages.Count > 0)
         {
            // You add a page on initialization, so you need to return the 2nd from the last
            return _previousPages.Pop();
         }

         // Can't go back because you didn't navigate enough
         return "/";
      }

      public bool CanGoBack()
      {
         return _previousPages.Count > 1;
      }
   }
}
