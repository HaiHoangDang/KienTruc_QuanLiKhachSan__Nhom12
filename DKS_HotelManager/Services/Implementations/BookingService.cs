using System;
using System.Collections.Generic;
using System.Linq;
using DKS_HotelManager.DTOs.Booking;
using DKS_HotelManager.Models;
using DKS_HotelManager.Repositories.Interfaces;
using DKS_HotelManager.Services.Interfaces;

namespace DKS_HotelManager.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;

        public BookingService(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public List<BookingResponse> GetAll()
        {
            return _bookingRepository.GetAll()
                .Select(MapToResponse)
                .ToList();
        }

        public BookingResponse GetById(int id)
        {
            var booking = _bookingRepository.GetById(id);
            if (booking == null) return null;
            return MapToResponse(booking);
        }

        public BookingResponse Create(CreateBookingRequest request)
        {
            if (request.NgayVao >= request.NgayTra)
            {
                throw new Exception("Ngày vào phải nhỏ hơn ngày trả.");
            }

            if (_bookingRepository.ExistsConflict(request.MaPhong, request.NgayVao, request.NgayTra))
            {
                throw new Exception("Phòng đã có lịch trùng.");
            }

            var booking = new THUEPHONG
            {
                MaThue = GenerateNextMaThue(),
                MaNV = request.MaNV,
                MaPhong = request.MaPhong,
                NgayDat = DateTime.Now,
                NgayVao = request.NgayVao,
                NgayTra = request.NgayTra,
                DatCoc = request.DatCoc,
                MaDatPhong = "DP" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                TrangThai = "Dang o"
            };

            _bookingRepository.Add(booking);
            _bookingRepository.Save();

            return MapToResponse(booking);
        }

        public void Cancel(int id)
        {
            var booking = _bookingRepository.GetById(id);
            if (booking == null)
            {
                throw new Exception("Không tìm thấy đơn thuê.");
            }

            booking.TrangThai = "Huy";
            _bookingRepository.Update(booking);
            _bookingRepository.Save();
        }

        public void UpdateStatus(int id, string trangThai)
        {
            var booking = _bookingRepository.GetById(id);
            if (booking == null)
            {
                throw new Exception("Không tìm thấy đơn thuê.");
            }

            booking.TrangThai = trangThai;
            _bookingRepository.Update(booking);
            _bookingRepository.Save();
        }

        private BookingResponse MapToResponse(THUEPHONG booking)
        {
            return new BookingResponse
            {
                MaThue = booking.MaThue,
                MaDatPhong = booking.MaDatPhong,
                MaNV = booking.MaNV,
                MaPhong = booking.MaPhong,
                NgayDat = booking.NgayDat,
                NgayVao = booking.NgayVao,
                NgayTra = booking.NgayTra,
                DatCoc = booking.DatCoc,
                TrangThai = booking.TrangThai
            };
        }

        private int GenerateNextMaThue()
        {
            var all = _bookingRepository.GetAll();
            return all.Any() ? all.Max(x => x.MaThue) + 1 : 1;
        }
    }
}