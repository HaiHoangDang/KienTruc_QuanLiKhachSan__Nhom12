using DKS_HotelManager.DTOs.Booking;
using DKS_HotelManager.Models;
using DKS_HotelManager.Repositories.Implementations;
using DKS_HotelManager.Repositories.Interfaces;
using DKS_HotelManager.Services.Implementations;
using DKS_HotelManager.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DKS_HotelManager.Controllers
{
    public class DKS_BookingController : Controller
    {
        private readonly DKS_HotelManagerEntities db = new DKS_HotelManagerEntities();
        private readonly IBookingService _bookingService;

        public DKS_BookingController()
        {
            IBookingRepository bookingRepository = new BookingRepository(db);
            _bookingService = new BookingService(bookingRepository);
        }

        public ActionResult Index()
        {
            var bookings = _bookingService.GetAll();
            return View(bookings);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            var booking = _bookingService.GetById(id.Value);
            if (booking == null)
            {
                return HttpNotFound();
            }

            return View(booking);
        }

        public ActionResult Create()
        {
            ViewBag.MaNV = new SelectList(db.NHANVIENs, "MaNV", "HoTen");
            ViewBag.MaPhong = new SelectList(db.PHONGs, "MaPhong", "TenPhong");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateBookingRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.MaNV = new SelectList(db.NHANVIENs, "MaNV", "HoTen", request.MaNV);
                    ViewBag.MaPhong = new SelectList(db.PHONGs, "MaPhong", "TenPhong", request.MaPhong);
                    return View(request);
                }

                var result = _bookingService.Create(request);
                return RedirectToAction("Details", new { id = result.MaThue });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.MaNV = new SelectList(db.NHANVIENs, "MaNV", "HoTen", request.MaNV);
                ViewBag.MaPhong = new SelectList(db.PHONGs, "MaPhong", "TenPhong", request.MaPhong);
                return View(request);
            }
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            var booking = _bookingService.GetById(id.Value);
            if (booking == null)
            {
                return HttpNotFound();
            }

            return View(booking);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _bookingService.Cancel(id);
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
        //private DKS_HotelManagerEntities db = new DKS_HotelManagerEntities();
        //public ActionResult Index()
        //{
        //    var bookings = db.THUEPHONGs.Include("PHONG").Include("NHANVIEN").ToList();
        //    return View(bookings);
        //}

        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    THUEPHONG booking = db.THUEPHONGs.Find(id);
        //    if (booking == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(booking);
        //}

        //// GET: Booking/Create
        //public ActionResult Create()
        //{
        //    ViewBag.MaNV = new SelectList(db.NHANVIENs, "MaNV", "HoTen");
        //    ViewBag.MaPhong = new SelectList(db.PHONGs, "MaPhong", "TenPhong");
        //    return View();
        //}

        //// POST: Booking/Create
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

        //// GET: Booking/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    THUEPHONG booking = db.THUEPHONGs.Find(id);
        //    if (booking == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.MaNV = new SelectList(db.NHANVIENs, "MaNV", "HoTen", booking.MaNV);
        //    ViewBag.MaPhong = new SelectList(db.PHONGs, "MaPhong", "TenPhong", booking.MaPhong);
        //    return View(booking);
        //}

        //// POST: Booking/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "MaThue,MaNV,MaPhong,NgayDat,NgayVao,NgayTra,DatCoc,MaDatPhong,TrangThai")] THUEPHONG booking)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(booking).State = System.Data.Entity.EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.MaNV = new SelectList(db.NHANVIENs, "MaNV", "HoTen", booking.MaNV);
        //    ViewBag.MaPhong = new SelectList(db.PHONGs, "MaPhong", "TenPhong", booking.MaPhong);
        //    return View(booking);
        //}

        //// GET: Booking/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    THUEPHONG booking = db.THUEPHONGs.Find(id);
        //    if (booking == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(booking);
        //}

        //// POST: Booking/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    THUEPHONG booking = db.THUEPHONGs.Find(id);
        //    db.THUEPHONGs.Remove(booking);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
    }
}

