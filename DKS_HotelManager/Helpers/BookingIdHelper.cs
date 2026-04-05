using System.Linq;
using DKS_HotelManager.Models;

namespace DKS_HotelManager.Helpers
{
    public static class BookingIdHelper
    {
        public static int GetNextBookingId(DKS_HotelManagerEntities db)
        {
            if (db.THUEPHONGs == null || !db.THUEPHONGs.Any())
            {
                return 1;
            }

            return db.THUEPHONGs.Max(t => t.MaThue) + 1;
        }
    }
}
