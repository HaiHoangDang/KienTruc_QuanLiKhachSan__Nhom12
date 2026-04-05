using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using DKS_HotelManager.Areas.Admin.ViewModels;
using DKS_HotelManager.Models;
using DKS_HotelManager.Helpers;

namespace DKS_HotelManager.Areas.Admin.Controllers
{
    public abstract class AdminBaseController : Controller
    {
        protected readonly DKS_HotelManagerEntities db = new DKS_HotelManagerEntities();

        protected IEnumerable<HotelListItem> GetHotelItems()
        {
            return db.KHACHSANs
                .Select(h => new HotelListItem
                {
                    MaKS = h.MaKS,
                    TenKS = h.TenKS,
                    DiaDiem = h.DiaDiem,
                    MoTa = h.MoTa,
                    RoomsCount = h.PHONGs.Count(),
                    CommentCount = h.BINHLUANs.Count()
                })
                .ToList();
        }

        protected IEnumerable<RoomListItem> GetRoomItems()
        {
            return db.PHONGs
                .Select(p => new RoomListItem
                {
                    MaPhong = p.MaPhong,
                    MaKS = p.MaKS,
                    MaLoai = p.MaLoai,
                    TenPhong = p.TenPhong,
                    HotelName = p.KHACHSAN != null ? p.KHACHSAN.TenKS : "Chưa gắn khách sạn",
                    RoomType = p.LOAIPHONG != null ? p.LOAIPHONG.TenLoai : "Chưa phân loại",
                    DGNgay = p.DGNgay,
                    SucChua = p.SucChua,
                    Tang = p.Tang,
                    DienTich = p.DienTich
                })
                .OrderBy(r => r.MaPhong)
                .ToList();
        }

        protected IEnumerable<ServiceListItem> GetServiceItems()
        {
            return db.DICHVUs
                .Select(d => new ServiceListItem
                {
                    MaDV = d.MaDV,
                    TenDV = d.TenDV,
                    DGDV = d.DGDV,
                    UsageCount = d.SDDICHVUs.Sum(s => s.SoLuot ?? 0)
                })
                .ToList();
        }

        protected IEnumerable<CommentListItem> GetCommentItems()
        {
            return db.BINHLUANs
                .OrderByDescending(b => b.NgayBL)
                .Take(30)
                .Select(b => new CommentListItem
                {
                    MaBL = b.MaBL,
                    MKH = b.MKH,
                    MaPhong = b.MaPhong,
                    MaKS = b.MaKS,
                    CustomerName = b.KHACHHANG != null ? b.KHACHHANG.TKH : "Khách vãng lai",
                    HotelName = b.KHACHSAN != null ? b.KHACHSAN.TenKS : "-",
                    RoomName = b.PHONG != null ? b.PHONG.TenPhong : "-",
                    Content = b.NoiDung,
                    CreatedAt = b.NgayBL
                })
                .ToList();
        }

        protected IEnumerable<EmployeeListItem> GetEmployeeItems()
        {
            int? currentStaffId = null;
            var currentIdObj = Session != null ? Session["AdminId"] : null;
            if (currentIdObj is int)
            {
                currentStaffId = (int)currentIdObj;
            }
            else if (currentIdObj is int?)
            {
                currentStaffId = (int?)currentIdObj;
            }

            return db.NHANVIENs
                .ToList()
                .Select(n =>
                {
                    TimeSpan? duration = null;
                    if (currentStaffId.HasValue && n.MaNV == currentStaffId.Value)
                    {
                        duration = StaffActivityTracker.GetCurrentSessionDuration(n.MaNV);
                    }

                    return new EmployeeListItem
                    {
                        MaNV = n.MaNV,
                        HoTen = n.HoTen,
                        ChucVu = n.ChucVu,
                        Email = n.Email,
                        SoDT = n.SoDT,
                        Username = n.TenDN,
                        NgaySinh = n.NgaySinh,
                        ActiveDuration = FormatActiveDuration(duration)
                    };
                })
                .ToList();
        }

        private string FormatActiveDuration(TimeSpan? duration)
        {
            if (!duration.HasValue || duration.Value <= TimeSpan.Zero)
            {
                return "-";
            }

            var d = duration.Value;

            if (d.TotalMinutes < 1)
            {
                return "< 1 phút";
            }

            var hours = (int)d.TotalHours;
            var minutes = d.Minutes;

            if (hours <= 0)
            {
                return minutes + " phút";
            }

            if (minutes <= 0)
            {
                return hours + " giờ";
            }

            return string.Format("{0} giờ {1} phút", hours, minutes);
        }

        protected IEnumerable<SelectListItem> GetHotelSelectList()
        {
            return db.KHACHSANs
                .OrderBy(h => h.TenKS)
                .Select(h => new SelectListItem
                {
                    Value = h.MaKS.ToString(),
                    Text = h.TenKS
                })
                .ToList();
        }

        protected IEnumerable<SelectListItem> GetRoomSelectList()
        {
            var rooms = db.PHONGs
                .OrderBy(p => p.TenPhong)
                .Select(p => new
                {
                    p.MaPhong,
                    p.TenPhong,
                    HotelName = p.KHACHSAN.TenKS
                })
                .ToList();

            return rooms.Select(p => new SelectListItem
            {
                Value = p.MaPhong.ToString(),
                Text = string.IsNullOrWhiteSpace(p.HotelName) ? p.TenPhong : $"{p.TenPhong} ({p.HotelName})"
            });
        }

        protected IEnumerable<SelectListItem> GetRoomTypeSelectList()
        {
            return db.LOAIPHONGs
                .OrderBy(l => l.MaLoai)
                .Select(l => new SelectListItem
                {
                    Value = l.MaLoai.ToString(),
                    Text = l.TenLoai
                })
                .ToList();
        }

        protected IEnumerable<SelectListItem> GetCustomerSelectList()
        {
            return db.KHACHHANGs
                .OrderBy(k => k.TKH)
                .Select(k => new SelectListItem
                {
                    Value = k.MKH.ToString(),
                    Text = k.TKH
                })
                .ToList();
        }

        protected IEnumerable<RevenueChartPoint> BuildRevenueSeries()
        {
            var revenueSummary = db.THANHTOANs
                .Where(t => t.NgayTT.HasValue)
                .GroupBy(t => new { t.NgayTT.Value.Year, t.NgayTT.Value.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Total = g.Sum(t => t.ThanhTien ?? 0)
                })
                .ToList();

            var startMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var culture = CultureInfo.GetCultureInfo("vi-VN");

            return Enumerable.Range(0, 6)
                .Select(offset => startMonth.AddMonths(offset - 5))
                .Select(month =>
                {
                    var match = revenueSummary.FirstOrDefault(r => r.Year == month.Year && r.Month == month.Month);
                    return new RevenueChartPoint
                    {
                        Label = month.ToString("MMM yyyy", culture),
                        Value = match?.Total ?? 0m
                    };
                })
                .ToList();
        }

        protected IEnumerable<SelectListItem> GetEmployeeSelectList()
        {
            return db.NHANVIENs
                .OrderBy(n => n.HoTen)
                .Select(n => new SelectListItem
                {
                    Value = n.MaNV.ToString(),
                    Text = n.HoTen
                })
                .ToList();
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
