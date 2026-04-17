using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DKS_HotelManager.Models;
using DKS_HotelManager.Repositories.Interfaces;

namespace DKS_HotelManager.Repositories.Implementations
{
    public class RoomRepository : IRoomRepository
    {
        private readonly DKS_HotelManagerEntities _context;

        public RoomRepository(DKS_HotelManagerEntities context)
        {
            _context = context;
        }
        public List<KHACHSAN> GetHotels()
        {
            return _context.KHACHSANs.ToList();
        }

        public List<LOAIPHONG> GetRoomTypes()
        {
            return _context.LOAIPHONGs.ToList();
        }
        public List<PHONG> GetAll()
        {
            return _context.PHONGs
                .Include("KHACHSAN")
                .Include("LOAIPHONG")
                .Include("HINHANHs")
                .ToList();
        }

        public PHONG GetById(int id)
        {
            return _context.PHONGs
                .Include("KHACHSAN")
                .Include("LOAIPHONG")
                .Include("HINHANHs")
                .FirstOrDefault(r => r.MaPhong == id);
        }

        public void Add(PHONG room)
        {
            _context.PHONGs.Add(room);
        }

        public void Update(PHONG room)
        {
            _context.Entry(room).State = EntityState.Modified;
        }

        public void Delete(int id)
        {
            var room = _context.PHONGs.Find(id);
            if (room != null)
            {
                _context.PHONGs.Remove(room);
            }
        }

        public int GetNextMaPhong()
        {
            return _context.PHONGs.Any() ? _context.PHONGs.Max(x => x.MaPhong) + 1 : 1;
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}