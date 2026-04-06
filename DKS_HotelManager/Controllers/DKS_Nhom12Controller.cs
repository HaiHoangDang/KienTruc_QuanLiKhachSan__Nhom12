using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using DKS_HotelManager.Models;

namespace DKS_HotelManager.Controllers
{
    public class DKS_Nhom12Controller : Controller
    {
        private DKS_HotelManagerEntities db = new DKS_HotelManagerEntities();

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult TrangChu()
        {
            // Thống kê tổng quan
            ViewBag.TongKhachSan = db.KHACHSANs.Count();
            ViewBag.TongPhong = db.PHONGs.Count();
            ViewBag.TongKhachHang = db.KHACHHANGs.Count();
            ViewBag.TongDatPhong = db.THUEPHONGs.Count();
            ViewBag.DatPhongDangHoatDong = db.THUEPHONGs.Where(t => t.TrangThai == "Đang sử dụng").Count();
            ViewBag.DatPhongDaHoanThanh = db.THUEPHONGs.Where(t => t.TrangThai == "Đã hoàn thành").Count();
            
            return View();
        }

        public ActionResult QuanLyKhachHang()
        {
            var khachHangs = db.KHACHHANGs.ToList();
            return View(khachHangs);
        }

        public ActionResult ChiTietKhachHang(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            KHACHHANG khachHang = db.KHACHHANGs.Find(id);
            if (khachHang == null)
            {
                return HttpNotFound();
            }
            
            // Lấy lịch sử đặt phòng của khách hàng
            var lichSuDatPhong = db.CTTHUEPHONGs
                .Where(ct => ct.KHACH == id)
                .Include("THUEPHONG")
                .Include("THUEPHONG.PHONG")
                .ToList();
            
            ViewBag.LichSuDatPhong = lichSuDatPhong;
            return View(khachHang);
        }
        public ActionResult ThemKhachHang()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemKhachHang([Bind(Include = "MKH,HoTen,SDT,Email,DiaChi,CMND,GioiTinh,NgaySinh")] KHACHHANG khachHang)
        {
            if (ModelState.IsValid)
            {
                db.KHACHHANGs.Add(khachHang);
                db.SaveChanges();
                return RedirectToAction("QuanLyKhachHang");
            }
            return View(khachHang);
        }

        public ActionResult SuaKhachHang(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            KHACHHANG khachHang = db.KHACHHANGs.Find(id);
            if (khachHang == null)
            {
                return HttpNotFound();
            }
            return View(khachHang);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SuaKhachHang([Bind(Include = "MKH,HoTen,SDT,Email,DiaChi,CMND,GioiTinh,NgaySinh")] KHACHHANG khachHang)
        {
            if (ModelState.IsValid)
            {
                db.Entry(khachHang).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("QuanLyKhachHang");
            }
            return View(khachHang);
        }

        public ActionResult XoaKhachHang(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            KHACHHANG khachHang = db.KHACHHANGs.Find(id);
            if (khachHang == null)
            {
                return HttpNotFound();
            }
            return View(khachHang);
        }

        [HttpPost, ActionName("XoaKhachHang")]
        [ValidateAntiForgeryToken]
        public ActionResult XacNhanXoaKhachHang(int id)
        {
            KHACHHANG khachHang = db.KHACHHANGs.Find(id);
            db.KHACHHANGs.Remove(khachHang);
            db.SaveChanges();
            return RedirectToAction("QuanLyKhachHang");
        }

        // GET: DKS_Nhom1/BaoCaoThongKe
        public ActionResult BaoCaoThongKe()
        {
            // Thống kê tổng quan
            ViewBag.TongKhachSan = db.KHACHSANs.Count();
            ViewBag.TongPhong = db.PHONGs.Count();
            ViewBag.TongKhachHang = db.KHACHHANGs.Count();
            ViewBag.TongDatPhong = db.THUEPHONGs.Count();
            ViewBag.TongDichVu = db.DICHVUs.Count();
            ViewBag.TongNhanVien = db.NHANVIENs.Count();

            // Thống kê theo trạng thái đặt phòng
            ViewBag.DatPhongDangCho = db.THUEPHONGs.Where(t => t.TrangThai == "Đang chờ").Count();
            ViewBag.DatPhongDangHoatDong = db.THUEPHONGs.Where(t => t.TrangThai == "Đang sử dụng").Count();
            ViewBag.DatPhongDaHoanThanh = db.THUEPHONGs.Where(t => t.TrangThai == "Đã hoàn thành").Count();
            ViewBag.DatPhongDaHuy = db.THUEPHONGs.Where(t => t.TrangThai == "Đã hủy").Count();

            // Thống kê doanh thu
            var doanhThuThang = db.THUEPHONGs
                .Where(t => t.NgayDat.Value.Month == DateTime.Now.Month && 
                           t.NgayDat.Value.Year == DateTime.Now.Year &&
                           t.TrangThai == "Đã hoàn thành")
                .Sum(t => (decimal?)t.DatCoc) ?? 0;
            ViewBag.DoanhThuThang = doanhThuThang;

            // Top 5 khách hàng đặt nhiều nhất
            var topKhachHang = db.CTTHUEPHONGs
                .GroupBy(ct => ct.KHACH)
                .Select(g => new { 
                    MKH = g.Key, 
                    SoLanDat = g.Count() 
                })
                .OrderByDescending(x => x.SoLanDat)
                .Take(5)
                .ToList();
            
            var topKhachHangList = new List<dynamic>();
            foreach (var item in topKhachHang)
            {
                var kh = db.KHACHHANGs.Find(item.MKH);
                if (kh != null)
                {
                    topKhachHangList.Add(new { 
                        HoTen = kh.CMND_CCCD, 
                        SoLanDat = item.SoLanDat 
                    });
                }
            }
            ViewBag.TopKhachHang = topKhachHangList;

            // Thống kê phòng theo loại
            var phongTheoLoai = db.PHONGs
                .GroupBy(p => p.LOAIPHONG.TenLoai)
                .Select(g => new { 
                    LoaiPhong = g.Key, 
                    SoLuong = g.Count() 
                })
                .ToList();
            ViewBag.PhongTheoLoai = phongTheoLoai;

            return View();
        }

        [HttpGet]
        public ActionResult SetLanguage(string lang, string returnUrl = "")
        {
            if (lang == "vi" || lang == "en")
            {
                Session["Language"] = lang;
                HttpCookie cookie = new HttpCookie("preferredLanguage", lang);
                cookie.Expires = DateTime.Now.AddYears(1);
                Response.Cookies.Add(cookie);
                
                // Update LanguageHelper
                Helpers.LanguageHelper.SetLanguage(lang);
            }
            
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            // If no return URL, go back to previous page or settings
            if (Request.UrlReferrer != null)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }
            
            return RedirectToAction("CaiDat");
        }

        [HttpPost]
        public ActionResult SetLanguagePost(string lang, string returnUrl = "")
        {
            return SetLanguage(lang, returnUrl);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}