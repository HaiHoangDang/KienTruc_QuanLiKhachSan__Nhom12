using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using DKS_HotelManager.Helpers;

namespace DKS_HotelManager.Areas.Admin.ViewModels
{
    public class AdminDashboardViewModel
    {
        public IEnumerable<HotelListItem> Hotels { get; set; } = new List<HotelListItem>();
        public IEnumerable<RoomListItem> Rooms { get; set; } = new List<RoomListItem>();
        public IEnumerable<ServiceListItem> Services { get; set; } = new List<ServiceListItem>();
        public IEnumerable<CommentListItem> Comments { get; set; } = new List<CommentListItem>();
        public IEnumerable<EmployeeListItem> Employees { get; set; } = new List<EmployeeListItem>();

        public IEnumerable<SelectListItem> HotelOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> RoomOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> RoomTypeOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> CustomerOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> EmployeeOptions { get; set; } = new List<SelectListItem>();

        public IEnumerable<RevenueChartPoint> RevenueSeries { get; set; } = new List<RevenueChartPoint>();
        public decimal TotalRevenue { get; set; }
    }

    public class HotelListItem
    {
        public int MaKS { get; set; }
        public string TenKS { get; set; }
        public string DiaDiem { get; set; }
        public string MoTa { get; set; }
        public int RoomsCount { get; set; }
        public int CommentCount { get; set; }
    }

    public class RoomListItem
    {
        public int MaPhong { get; set; }
        public int MaKS { get; set; }
        public int MaLoai { get; set; }
        public string TenPhong { get; set; }
        public string HotelName { get; set; }
        public string RoomType { get; set; }
        public decimal DGNgay { get; set; }
        public int SucChua { get; set; }
        public int Tang { get; set; }
        public double? DienTich { get; set; }
    }

    public class ServiceListItem
    {
        public int MaDV { get; set; }
        public string TenDV { get; set; }
        public decimal DGDV { get; set; }
        public int UsageCount { get; set; }
    }

    public class CommentListItem
    {
        public int MaBL { get; set; }
        public int MKH { get; set; }
        public int? MaPhong { get; set; }
        public int? MaKS { get; set; }
        public string CustomerName { get; set; }
        public string HotelName { get; set; }
        public string RoomName { get; set; }
        public string Content { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class EmployeeListItem
    {
        public int MaNV { get; set; }
        public string HoTen { get; set; }
        public string ChucVu { get; set; }
        public string Email { get; set; }
        public string SoDT { get; set; }
        public string Username { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string ActiveDuration { get; set; }
    }

    public class RevenueChartPoint
    {
        public string Label { get; set; }
        public decimal Value { get; set; }
    }

    public class HotelInputModel
    {
        public int? MaKS { get; set; }

        [Required(ErrorMessage = "Tên khách sạn là bắt buộc")]
        public string TenKS { get; set; }

        public string DiaDiem { get; set; }
        public string MoTa { get; set; }
    }

    public class RoomInputModel
    {
        public int? MaPhong { get; set; }

        [Required(ErrorMessage = "Tên phòng là bắt buộc")]
        public string TenPhong { get; set; }

        [Required(ErrorMessage = "Khách sạn là bắt buộc")]
        public int MaKS { get; set; }

        [Required(ErrorMessage = "Loại phòng là bắt buộc")]
        public int MaLoai { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Sức chứa phải lớn hơn 0")]
        public int SucChua { get; set; } = 1;

        [Range(0, int.MaxValue, ErrorMessage = "Tầng không hợp lệ")]
        public int Tang { get; set; } = 0;

        public double? DienTich { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá phòng phải lớn hơn hoặc bằng 0")]
        public decimal DGNgay { get; set; }
    }

    public class ServiceInputModel
    {
        public int? MaDV { get; set; }

        [Required(ErrorMessage = "Tên dịch vụ là bắt buộc")]
        public string TenDV { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá dịch vụ phải lớn hơn hoặc bằng 0")]
        public decimal DGDV { get; set; }
    }

    public class CommentInputModel
    {
        public int? MaBL { get; set; }
        public int? MaPhong { get; set; }
        public int? MaKS { get; set; }

        [Required(ErrorMessage = "Khách hàng là bắt buộc")]
        public int MKH { get; set; }

        [Required(ErrorMessage = "Nội dung bình luận là bắt buộc")]
        public string NoiDung { get; set; }
        public DateTime? NgayBL { get; set; }
    }

    public class EmployeeInputModel
    {
        public int? MaNV { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        public string HoTen { get; set; }
        public DateTime? NgaySinh { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string SoDT { get; set; }

        [Required(ErrorMessage = "Chức vụ là bắt buộc")]
        public string ChucVu { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        public string TenDN { get; set; }

        public string MatKhau { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }
    }

    public class DashboardSummaryViewModel
    {
        public int HotelCount { get; set; }
        public int RoomCount { get; set; }
        public int ServiceCount { get; set; }
        public int EmployeeCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public IEnumerable<RevenueChartPoint> RevenueSeries { get; set; } = new List<RevenueChartPoint>();
        public IEnumerable<StaffActivityRecord> StaffActivities { get; set; } = new List<StaffActivityRecord>();
    }

    public class HotelsPageViewModel
    {
        public IEnumerable<HotelListItem> Hotels { get; set; } = new List<HotelListItem>();
    }

    public class RoomsPageViewModel
    {
        public IEnumerable<RoomListItem> Rooms { get; set; } = new List<RoomListItem>();
        public IEnumerable<SelectListItem> HotelOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> RoomTypeOptions { get; set; } = new List<SelectListItem>();
    }

    public class ServicesPageViewModel
    {
        public IEnumerable<ServiceListItem> Services { get; set; } = new List<ServiceListItem>();
    }

    public class CommentsPageViewModel
    {
        public IEnumerable<CommentListItem> Comments { get; set; } = new List<CommentListItem>();
        public IEnumerable<SelectListItem> CustomerOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> HotelOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> RoomOptions { get; set; } = new List<SelectListItem>();
    }

    public class EmployeesPageViewModel
    {
        public IEnumerable<EmployeeListItem> Employees { get; set; } = new List<EmployeeListItem>();
    }

    public class RevenuePageViewModel
    {
        public decimal TotalRevenue { get; set; }
        public IEnumerable<RevenueChartPoint> RevenueSeries { get; set; } = new List<RevenueChartPoint>();
    }
}
