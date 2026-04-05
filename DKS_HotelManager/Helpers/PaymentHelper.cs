using System.Linq;
using DKS_HotelManager.Models;

namespace DKS_HotelManager.Helpers
{
    public static class PaymentHelper
    {
        public static int GetNextThanhToanId(DKS_HotelManagerEntities db)
        {
            var maxId = db.THANHTOANs.Select(t => (int?)t.MaTT).Max();
            return (maxId ?? 0) + 1;
        }
    }
}