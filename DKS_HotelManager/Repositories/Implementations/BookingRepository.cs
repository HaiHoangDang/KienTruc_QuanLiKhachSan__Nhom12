using System;
using System.Collections.Generic;
using System.Linq;
using DKS_HotelManager.Models;
using DKS_HotelManager.Repositories.Interfaces;

namespace DKS_HotelManager.Repositories.Implementations
{
    public class BookingRepository : IBookingRepository
    {
        private readonly DKS_HotelManagerEntities _context;

        public BookingRepository(DKS_HotelManagerEntities context)
        {
            _context = context;
        }

        public List<THUEPHONG> GetAll()
        {
            return _context.THUEPHONGs.ToList();
        }

        public THUEPHONG GetById(int id)
        {
            return _context.THUEPHONGs.FirstOrDefault(x => x.MaThue == id);
        }

        public void Add(THUEPHONG booking)
        {
            _context.THUEPHONGs.Add(booking);
        }

        public void Update(THUEPHONG booking)
        {
            _context.Entry(booking).State = System.Data.Entity.EntityState.Modified;
        }

        public void Delete(int id)
        {
            var entity = _context.THUEPHONGs.FirstOrDefault(x => x.MaThue == id);
            if (entity != null)
            {
                _context.THUEPHONGs.Remove(entity);
            }
        }

        public bool ExistsConflict(int maPhong, DateTime ngayVao, DateTime ngayTra)
        {
            return _context.THUEPHONGs.Any(x =>
                x.MaPhong == maPhong &&
                x.TrangThai != "Huy" &&
                !(x.NgayTra <= ngayVao || x.NgayVao >= ngayTra));
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}