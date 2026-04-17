using DKS_HotelManager.DTOs.Common;
using DKS_HotelManager.DTOs.Room;
using DKS_HotelManager.Models;
using DKS_HotelManager.Repositories.Interfaces;
using DKS_HotelManager.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DKS_HotelManager.Services.Implementations
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepository;

        public RoomService(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }
        public List<DropdownItem> GetHotels()
        {
            return _roomRepository.GetHotels()
                .Select(x => new DropdownItem
                {
                    Value = x.MaKS,
                    Text = x.TenKS
                })
                .ToList();
        }

        public List<DropdownItem> GetRoomTypes()
        {
            return _roomRepository.GetRoomTypes()
                .Select(x => new DropdownItem
                {
                    Value = x.MaLoai,
                    Text = x.TenLoai
                })
                .ToList();
        }
        //public List<KHACHSAN> GetHotels()
        //{
        //    return _roomRepository.GetHotels();
        //}

        //public List<LOAIPHONG> GetRoomTypes()
        //{
        //    return _roomRepository.GetRoomTypes();
        //}
        public List<RoomResponse> GetAll()
        {
            return _roomRepository.GetAll()
                .Select(MapToResponse)
                .ToList();
        }

        public RoomResponse GetById(int id)
        {
            var room = _roomRepository.GetById(id);
            if (room == null) return null;
            return MapToResponse(room);
        }

        public RoomResponse Create(CreateRoomRequest request)
        {
            var room = new PHONG
            {
                MaPhong = _roomRepository.GetNextMaPhong(),
                TenPhong = request.TenPhong,
                MaKS = request.MaKS,
                MaLoai = request.MaLoai,
                SucChua = request.SucChua,
                Tang = request.Tang,
                DienTich = request.DienTich,
                DGNgay = request.DGNgay
            };

            _roomRepository.Add(room);
            _roomRepository.Save();

            var created = _roomRepository.GetById(room.MaPhong);
            return MapToResponse(created);
        }

        public void Update(UpdateRoomRequest request)
        {
            var room = _roomRepository.GetById(request.MaPhong);
            if (room == null)
            {
                throw new Exception("Không tìm thấy phòng.");
            }

            room.TenPhong = request.TenPhong;
            room.MaKS = request.MaKS;
            room.MaLoai = request.MaLoai;
            room.SucChua = request.SucChua;
            room.Tang = request.Tang;
            room.DienTich = request.DienTich;
            room.DGNgay = request.DGNgay;

            _roomRepository.Update(room);
            _roomRepository.Save();
        }

        public void Delete(int id)
        {
            var room = _roomRepository.GetById(id);
            if (room == null)
            {
                throw new Exception("Không tìm thấy phòng.");
            }

            _roomRepository.Delete(id);
            _roomRepository.Save();
        }

        private RoomResponse MapToResponse(PHONG room)
        {
            return new RoomResponse
            {
                MaPhong = room.MaPhong,
                TenPhong = room.TenPhong,
                MaKS = room.MaKS,
                MaLoai = room.MaLoai,
                SucChua = room.SucChua,
                Tang = room.Tang,
                DienTich = room.DienTich,
                DGNgay = room.DGNgay,
                TenKS = room.KHACHSAN != null ? room.KHACHSAN.TenKS : "",
                TenLoai = room.LOAIPHONG != null ? room.LOAIPHONG.TenLoai : "",
                FirstImagePath = room.HINHANHs != null && room.HINHANHs.Any()
                    ? room.HINHANHs.OrderBy(h => h.MaHinh).Select(h => h.DuongDan).FirstOrDefault()
                    : null
            };
        }
    }
}