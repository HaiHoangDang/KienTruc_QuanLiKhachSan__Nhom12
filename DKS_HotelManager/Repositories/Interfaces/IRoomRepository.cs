using System.Collections.Generic;
using DKS_HotelManager.Models;

namespace DKS_HotelManager.Repositories.Interfaces
{
    public interface IRoomRepository
    {
        List<PHONG> GetAll();
        PHONG GetById(int id);
        void Add(PHONG room);
        void Update(PHONG room);
        void Delete(int id);
        int GetNextMaPhong();
        void Save();
    }
}