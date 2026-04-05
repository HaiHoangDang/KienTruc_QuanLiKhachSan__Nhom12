using System.Web.Mvc;

namespace DKS_HotelManager.Areas.Staff.Controllers
{
    [Authorize(Roles = "Staff")]
    public class OperationsController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Nhân viên lễ tân";
            return View();
        }
    }
}
