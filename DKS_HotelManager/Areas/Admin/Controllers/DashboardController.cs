using System.Linq;
using System.Web.Mvc;
using DKS_HotelManager.Areas.Admin.ViewModels;
using DKS_HotelManager.Helpers;

namespace DKS_HotelManager.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class DashboardController : AdminBaseController
    {
        public ActionResult Index()
        {
            return View(new DashboardSummaryViewModel
            {
                HotelCount = db.KHACHSANs.Count(),
                RoomCount = db.PHONGs.Count(),
                ServiceCount = db.DICHVUs.Count(),
                EmployeeCount = db.NHANVIENs.Count(),
                TotalRevenue = db.THANHTOANs.Sum(t => t.ThanhTien) ?? 0m,
                RevenueSeries = BuildRevenueSeries(),
                StaffActivities = StaffActivityTracker.GetRecent(8)
            });
        }
    }
}
