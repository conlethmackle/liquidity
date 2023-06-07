namespace CustomerIdAllocation
{
   public interface ICustomerIdAllocator
   {
      int AllocateCustomerId(string venue);
   }
   public class CustomerIdAllocator : ICustomerIdAllocator
   {
      private Dictionary<string, int> _currentIdTable = new();
      Random _random = new ();

      public int AllocateCustomerId(string venue)
      {
         // Just for now - will link to database - ideally this would give out ranges for each venue to get round multiple strategies using same venue
         if (_currentIdTable.ContainsKey(venue))
         {
           var id = _random.Next(10, 1000000);
            //var id =  _currentIdTable[venue]++;
            _currentIdTable[venue] = id;
            return id;
         }
         else
         {
            _currentIdTable.Add(venue, _random.Next(1, 427000000));
            return 1;
         }
      }
   }
}