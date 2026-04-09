using System;
using System.Collections.Generic;
using DKS_HotelManager.Models;

namespace DKS_HotelManager.Repositories.Interfaces
{
    public interface IBookingRepository
    {
        List<THUEPHONG> GetAll();
        THUEPHONG GetById(int id);
        void Add(THUEPHONG booking);
        void Update(THUEPHONG booking);
        void Delete(int id);
        bool ExistsConflict(int maPhong, DateTime ngayVao, DateTime ngayTra);
        void Save();
    }
}