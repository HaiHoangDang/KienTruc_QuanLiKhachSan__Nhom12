using System.Linq;
using System.Web.Mvc;
using DKS_HotelManager.Areas.Admin.ViewModels;
using DKS_HotelManager.Helpers;
using DKS_HotelManager.Models;

namespace DKS_HotelManager.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class ServicesController : AdminBaseController
    {
        public ActionResult Index()
        {
            return View(new ServicesPageViewModel
            {
                Services = GetServiceItems()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveService(ServiceInputModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["AdminError"] = "Thông tin dịch vụ không hợp lệ.";
                return RedirectToAction("Index");
            }

            var service = model.MaDV.HasValue && model.MaDV.Value > 0
                ? db.DICHVUs.FirstOrDefault(d => d.MaDV == model.MaDV.Value)
                : new DICHVU();

            if (service == null)
            {
                TempData["AdminError"] = "Không tìm thấy dịch vụ để cập nhật.";
                return RedirectToAction("Index");
            }

            if (!model.MaDV.HasValue || model.MaDV.Value == 0)
            {
                db.DICHVUs.Add(service);
            }

            service.TenDV = model.TenDV?.Trim();
            service.DGDV = model.DGDV;
            db.SaveChanges();

            TempData["AdminSuccess"] = model.MaDV.HasValue ? "Đã cập nhật dịch vụ." : "Đã thêm dịch vụ mới.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteService(int maDV)
        {
            var service = db.DICHVUs.FirstOrDefault(d => d.MaDV == maDV);
            if (service == null)
            {
                TempData["AdminError"] = "Dịch vụ không tồn tại.";
                return RedirectToAction("Index");
            }

            if (db.SDDICHVUs.Any(s => s.DV == maDV))
            {
                TempData["AdminError"] = "Không thể xóa dịch vụ đang được sử dụng.";
                return RedirectToAction("Index");
            }

            db.DICHVUs.Remove(service);
            db.SaveChanges();
            TempData["AdminSuccess"] = "Đã xóa dịch vụ.";
            return RedirectToAction("Index");
        }
    }
}
