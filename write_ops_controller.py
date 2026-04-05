from pathlib import Path

content = """using System.Web.Mvc;

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
"""

path = Path("Areas") / "Staff" / "Controllers" / "OperationsController.cs"
path.write_text(content, encoding="utf-8")
