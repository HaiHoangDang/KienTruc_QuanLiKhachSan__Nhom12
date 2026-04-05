using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using DKS_HotelManager.Areas.Admin.ViewModels;
using DKS_HotelManager.Helpers;
using DKS_HotelManager.Models;

namespace DKS_HotelManager.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class HotelsController : AdminBaseController
    {
        public ActionResult Index()
        {
            return View(new HotelsPageViewModel { Hotels = GetHotelItems() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveHotel(HotelInputModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["AdminError"] = "Thông tin khách sạn không hợp lệ.";
                return RedirectToAction("Index");
            }

            KHACHSAN hotel;
            if (model.MaKS.HasValue && model.MaKS.Value > 0)
            {
                hotel = db.KHACHSANs.FirstOrDefault(h => h.MaKS == model.MaKS.Value);
                if (hotel == null)
                {
                    TempData["AdminError"] = "Không tìm thấy khách sạn để cập nhật.";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                hotel = new KHACHSAN();
                db.KHACHSANs.Add(hotel);
            }

            hotel.TenKS = model.TenKS?.Trim();
            hotel.DiaDiem = string.IsNullOrWhiteSpace(model.DiaDiem) ? null : model.DiaDiem.Trim();
            hotel.MoTa = string.IsNullOrWhiteSpace(model.MoTa) ? null : model.MoTa.Trim();
            db.SaveChanges();

            TempData["AdminSuccess"] = model.MaKS.HasValue ? "Cập nhật khách sạn thành công." : "Tạo khách sạn mới thành công.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteHotel(int maKS)
        {
            var hotel = db.KHACHSANs.Include(h => h.PHONGs).FirstOrDefault(h => h.MaKS == maKS);
            if (hotel == null)
            {
                TempData["AdminError"] = "Khách sạn không tồn tại.";
                return RedirectToAction("Index");
            }

            if (hotel.PHONGs.Any())
            {
                TempData["AdminError"] = "Không thể xóa khách sạn đang có phòng gắn với nó.";
                return RedirectToAction("Index");
            }

            db.KHACHSANs.Remove(hotel);
            db.SaveChanges();
            TempData["AdminSuccess"] = "Đã xóa khách sạn.";
            return RedirectToAction("Index");
        }
    }
}
