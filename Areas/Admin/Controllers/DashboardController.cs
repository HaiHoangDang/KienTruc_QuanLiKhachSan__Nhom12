using System.Web.Mvc;

namespace DKS_HotelManager.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Admin Dashboard";
            ViewBag.Message = "Trang quản trị dành cho quản lý khách sạn.";
            return View();
        }
    }
}
