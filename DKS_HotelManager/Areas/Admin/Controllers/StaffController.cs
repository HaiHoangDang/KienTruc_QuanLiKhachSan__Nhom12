using System.Linq;
using System.Web.Mvc;
using DKS_HotelManager.Areas.Admin.ViewModels;
using DKS_HotelManager.Helpers;
using DKS_HotelManager.Models;

namespace DKS_HotelManager.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class StaffController : AdminBaseController
    {
        public ActionResult Index()
        {
            return View(new EmployeesPageViewModel
            {
                Employees = GetEmployeeItems()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveEmployee(EmployeeInputModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["AdminError"] = "Thông tin nhân viên không hợp lệ.";
                return RedirectToAction("Index");
            }

            if (db.NHANVIENs.Any(n => n.TenDN == model.TenDN && n.MaNV != model.MaNV))
            {
                TempData["AdminError"] = "Tên đăng nhập nhân viên đã tồn tại.";
                return RedirectToAction("Index");
            }

            if (!model.MaNV.HasValue && string.IsNullOrWhiteSpace(model.MatKhau))
            {
                TempData["AdminError"] = "Mật khẩu là bắt buộc khi tạo nhân viên mới.";
                return RedirectToAction("Index");
            }

            var employee = model.MaNV.HasValue && model.MaNV.Value > 0
                ? db.NHANVIENs.FirstOrDefault(n => n.MaNV == model.MaNV.Value)
                : new NHANVIEN();

            if (employee == null)
            {
                TempData["AdminError"] = "Không tìm thấy nhân viên để cập nhật.";
                return RedirectToAction("Index");
            }

            if (!model.MaNV.HasValue || model.MaNV.Value == 0)
            {
                db.NHANVIENs.Add(employee);
            }

            employee.HoTen = model.HoTen?.Trim();
            employee.NgaySinh = model.NgaySinh;
            employee.SoDT = string.IsNullOrWhiteSpace(model.SoDT) ? null : model.SoDT.Trim();
            employee.ChucVu = model.ChucVu?.Trim();
            employee.TenDN = model.TenDN?.Trim();
            if (!string.IsNullOrWhiteSpace(model.MatKhau))
            {
                employee.MatKhau = PasswordHasher.HashPassword(model.MatKhau);
            }

            employee.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
            db.SaveChanges();

            TempData["AdminSuccess"] = model.MaNV.HasValue ? "Đã cập nhật nhân viên." : "Đã thêm nhân viên mới.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteEmployee(int maNV)
        {
            var employee = db.NHANVIENs.FirstOrDefault(n => n.MaNV == maNV);
            if (employee == null)
            {
                TempData["AdminError"] = "Nhân viên không tồn tại.";
                return RedirectToAction("Index");
            }

            if (db.THUEPHONGs.Any(t => t.MaNV == maNV))
            {
                TempData["AdminError"] = "Không thể xóa nhân viên đang có đơn thuê phòng.";
                return RedirectToAction("Index");
            }

            db.NHANVIENs.Remove(employee);
            db.SaveChanges();
            TempData["AdminSuccess"] = "Đã xóa nhân viên.";
            return RedirectToAction("Index");
        }
    }
}
