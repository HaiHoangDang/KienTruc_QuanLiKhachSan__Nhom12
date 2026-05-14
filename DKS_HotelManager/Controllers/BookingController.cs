using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using DKS_HotelManager.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
namespace DKS_HotelManager.Controllers
{
    public class DKS_BookingController : Controller
    {
        private DKS_HotelManagerEntities db = new DKS_HotelManagerEntities();

        // GET: Booking
        public ActionResult Index()
        {
            var bookings = db.THUEPHONGs.Include("PHONG").Include("NHANVIEN").ToList();
            return View(bookings);
        }

        // GET: Booking/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            THUEPHONG booking = db.THUEPHONGs.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // GET: Booking/Create
        public ActionResult Create()
        {
            ViewBag.MaNV = new SelectList(db.NHANVIENs, "MaNV", "HoTen");
            ViewBag.MaPhong = new SelectList(db.PHONGs, "MaPhong", "TenPhong");
            return View();
        }

        // POST: Booking/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "MaThue,MaNV,MaPhong,NgayDat,NgayVao,NgayTra,DatCoc,MaDatPhong,TrangThai")] THUEPHONG booking)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        booking.NgayDat = DateTime.Now;
        //        booking.MaDatPhong = "DP" + DateTime.Now.ToString("yyyyMMddHHmmss");
        //        db.THUEPHONGs.Add(booking);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.MaNV = new SelectList(db.NHANVIENs, "MaNV", "HoTen", booking.MaNV);
        //    ViewBag.MaPhong = new SelectList(db.PHONGs, "MaPhong", "TenPhong", booking.MaPhong);
        //    return View(booking);
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "MaNV,MaPhong,NgayVao,NgayTra,DatCoc")] THUEPHONG booking)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MaNV = new SelectList(db.NHANVIENs, "MaNV", "HoTen", booking.MaNV);
                ViewBag.MaPhong = new SelectList(db.PHONGs, "MaPhong", "TenPhong", booking.MaPhong);
                return View(booking);
            }

            var token = Session["token"]?.ToString();

            if (string.IsNullOrWhiteSpace(token))
            {
                TempData["BookingError"] = "Bạn cần đăng nhập trước khi đặt phòng.";
                return RedirectToAction("Login", "Authentication");
            }

            var payload = new
            {
                maNV = booking.MaNV,
                maPhong = booking.MaPhong,
                ngayVao = booking.NgayVao,
                ngayTra = booking.NgayTra,
                datCoc = booking.DatCoc
            };

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var json = JsonConvert.SerializeObject(payload);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(
                    "http://localhost:6000/api/booking",
                    content
                );
                System.Diagnostics.Debug.WriteLine("[BOOKING MVC] Đang gọi API qua Ocelot: http://localhost:6000/api/booking");
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("[BOOKING MVC] API lỗi: " + responseBody);
                    TempData["BookingError"] = responseBody;
                    ViewBag.MaNV = new SelectList(db.NHANVIENs, "MaNV", "HoTen", booking.MaNV);
                    ViewBag.MaPhong = new SelectList(db.PHONGs, "MaPhong", "TenPhong", booking.MaPhong);
                    return View(booking);
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            THUEPHONG booking = db.THUEPHONGs.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaNV = new SelectList(db.NHANVIENs, "MaNV", "HoTen", booking.MaNV);
            ViewBag.MaPhong = new SelectList(db.PHONGs, "MaPhong", "TenPhong", booking.MaPhong);
            return View(booking);
        }

        // POST: Booking/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaThue,MaNV,MaPhong,NgayDat,NgayVao,NgayTra,DatCoc,MaDatPhong,TrangThai")] THUEPHONG booking)
        {
            if (ModelState.IsValid)
            {
                db.Entry(booking).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MaNV = new SelectList(db.NHANVIENs, "MaNV", "HoTen", booking.MaNV);
            ViewBag.MaPhong = new SelectList(db.PHONGs, "MaPhong", "TenPhong", booking.MaPhong);
            return View(booking);
        }

        // GET: Booking/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            THUEPHONG booking = db.THUEPHONGs.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // POST: Booking/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            THUEPHONG booking = db.THUEPHONGs.Find(id);
            db.THUEPHONGs.Remove(booking);
            db.SaveChanges();
            return RedirectToAction("Index");
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

