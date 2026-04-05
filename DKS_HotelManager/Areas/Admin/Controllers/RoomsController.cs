using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using DKS_HotelManager.Areas.Admin.ViewModels;
using DKS_HotelManager.Helpers;
using DKS_HotelManager.Models;

namespace DKS_HotelManager.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class RoomsController : AdminBaseController
    {
        public ActionResult Index()
        {
            return View(new RoomsPageViewModel
            {
                Rooms = GetRoomItems(),
                HotelOptions = GetHotelSelectList(),
                RoomTypeOptions = GetRoomTypeSelectList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveRoom(RoomInputModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["AdminError"] = "Thông tin phòng không hợp lệ.";
                return RedirectToAction("Index");
            }

            var hotel = db.KHACHSANs.Find(model.MaKS);
            if (hotel == null)
            {
                TempData["AdminError"] = "Khách sạn chọn không hợp lệ.";
                return RedirectToAction("Index");
            }

            var room = model.MaPhong.HasValue && model.MaPhong.Value > 0
                ? db.PHONGs.FirstOrDefault(p => p.MaPhong == model.MaPhong.Value)
                : new PHONG();

            if (room == null)
            {
                TempData["AdminError"] = "Không tìm thấy phòng để cập nhật.";
                return RedirectToAction("Index");
            }

            if (!model.MaPhong.HasValue || model.MaPhong.Value == 0)
            {
                db.PHONGs.Add(room);
            }

            room.TenPhong = model.TenPhong?.Trim();
            room.MaKS = model.MaKS;
            room.MaLoai = model.MaLoai;
            room.SucChua = model.SucChua;
            room.Tang = model.Tang;
            room.DienTich = model.DienTich;
            room.DGNgay = model.DGNgay;
            db.SaveChanges();

            TempData["AdminSuccess"] = model.MaPhong.HasValue ? "Đã cập nhật phòng." : "Đã thêm phòng mới.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRoom(int maPhong)
        {
            var room = db.PHONGs.Include(r => r.THUEPHONGs).FirstOrDefault(r => r.MaPhong == maPhong);
            if (room == null)
            {
                TempData["AdminError"] = "Phòng không tồn tại.";
                return RedirectToAction("Index");
            }

            if (room.THUEPHONGs.Any())
            {
                TempData["AdminError"] = "Không thể xóa phòng đang có đặt phòng.";
                return RedirectToAction("Index");
            }

            db.PHONGs.Remove(room);
            db.SaveChanges();
            TempData["AdminSuccess"] = "Đã xóa phòng.";
            return RedirectToAction("Index");
        }
    }
}
