using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using DKS_HotelManager.Models;

namespace DKS_HotelManager.Controllers
{
    public class DKS_RoomController : Controller
    {
        private DKS_HotelManagerEntities db = new DKS_HotelManagerEntities();

        // GET: Room
        public ActionResult Index()
        {
            var rooms = db.PHONGs
                .Include("KHACHSAN")
                .Include("LOAIPHONG")
                .Include("HINHANHs")
                .ToList();
            return View(rooms);
        }

        // GET: Room/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            PHONG room = db.PHONGs
                .Include("KHACHSAN")
                .Include("LOAIPHONG")
                .Include("HINHANHs")
                .FirstOrDefault(r => r.MaPhong == id);
            if (room == null)
            {
                return HttpNotFound();
            }
            return View(room);
        }

        // GET: Room/Create
        public ActionResult Create()
        {
            ViewBag.MaKS = new SelectList(db.KHACHSANs, "MaKS", "TenKS");
            ViewBag.MaLoai = new SelectList(db.LOAIPHONGs, "MaLoai", "TenLoai");
            return View();
        }

        // POST: Room/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaPhong,TenPhong,MaKS,MaLoai,SucChua,Tang,DienTich,DGNgay")] PHONG room)
        {
            if (ModelState.IsValid)
            {
                db.PHONGs.Add(room);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MaKS = new SelectList(db.KHACHSANs, "MaKS", "TenKS", room.MaKS);
            ViewBag.MaLoai = new SelectList(db.LOAIPHONGs, "MaLoai", "TenLoai", room.MaLoai);
            return View(room);
        }

        // GET: Room/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            PHONG room = db.PHONGs
                .Include("HINHANHs")
                .FirstOrDefault(r => r.MaPhong == id);
            if (room == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaKS = new SelectList(db.KHACHSANs, "MaKS", "TenKS", room.MaKS);
            ViewBag.MaLoai = new SelectList(db.LOAIPHONGs, "MaLoai", "TenLoai", room.MaLoai);
            return View(room);
        }

        // POST: Room/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaPhong,TenPhong,MaKS,MaLoai,SucChua,Tang,DienTich,DGNgay")] PHONG room)
        {
            if (ModelState.IsValid)
            {
                db.Entry(room).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MaKS = new SelectList(db.KHACHSANs, "MaKS", "TenKS", room.MaKS);
            ViewBag.MaLoai = new SelectList(db.LOAIPHONGs, "MaLoai", "TenLoai", room.MaLoai);
            return View(room);
        }

        // GET: Room/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            PHONG room = db.PHONGs.Find(id);
            if (room == null)
            {
                return HttpNotFound();
            }
            return View(room);
        }

        // POST: Room/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PHONG room = db.PHONGs.Find(id);
            db.PHONGs.Remove(room);
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

