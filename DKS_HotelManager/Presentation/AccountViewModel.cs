using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DKS_HotelManager.Presentation
{
    public class AccountProfileViewModel
    {
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập CMND/CCCD.")]
        public string IdNumber { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [MinLength(4, ErrorMessage = "Mật khẩu mới tối thiểu 4 ký tự.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }
    }

    public class BookingSummaryViewModel
    {
        public int BookingId { get; set; }
        public string BookingCode { get; set; }
        public string HotelName { get; set; }
        public string RoomName { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public string Status { get; set; }
        public decimal? Deposit { get; set; }
    }

    public class AccountPageViewModel
    {
        public AccountProfileViewModel Profile { get; set; } = new AccountProfileViewModel();
        public ChangePasswordViewModel Password { get; set; } = new ChangePasswordViewModel();
        public List<BookingSummaryViewModel> Bookings { get; set; } = new List<BookingSummaryViewModel>();
    }

    public class AccountBookingEditViewModel
    {
        public int BookingId { get; set; }
        public int HotelId { get; set; }
        public string HotelName { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phòng.")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày nhận phòng.")]
        [DataType(DataType.Date)]
        public DateTime? CheckIn { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày trả phòng.")]
        [DataType(DataType.Date)]
        public DateTime? CheckOut { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Đặt cọc không hợp lệ.")]
        public decimal? Deposit { get; set; }

        public IEnumerable<SelectListItem> RoomOptions { get; set; } = new List<SelectListItem>();
    }
}
