using System.Linq;
using DKS_HotelManager.Models;

namespace DKS_HotelManager.Helpers
{
    public static class CustomerIdHelper
    {
        public static int GetNextCustomerId(DKS_HotelManagerEntities db)
        {
            if (db.KHACHHANGs == null || !db.KHACHHANGs.Any())
            {
                return 1;
            }

            return db.KHACHHANGs.Max(c => c.MKH) + 1;
        }
    }
}
