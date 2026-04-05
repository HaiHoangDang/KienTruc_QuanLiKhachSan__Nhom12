using System.Linq;
using System.Web.Mvc;
using DKS_HotelManager.Areas.Admin.ViewModels;
using DKS_HotelManager.Helpers;

namespace DKS_HotelManager.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class RevenueController : AdminBaseController
    {
        public ActionResult Index()
        {
            return View(new RevenuePageViewModel
            {
                TotalRevenue = db.THANHTOANs.Sum(t => t.ThanhTien) ?? 0m,
                RevenueSeries = BuildRevenueSeries()
            });
        }
    }
}
