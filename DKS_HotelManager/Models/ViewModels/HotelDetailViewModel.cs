using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DKS_HotelManager.Models.ViewModels
{
    public class HotelDetailViewModel
    {
        public int HotelId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string HeroImage { get; set; }
        public int Guests { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public List<HotelRoomViewModel> Rooms { get; set; } = new List<HotelRoomViewModel>();
        public IEnumerable<HotelRoomViewModel> AllRooms { get; set; } = new List<HotelRoomViewModel>();
        public IEnumerable<string> RoomTypes { get; set; } = new List<string>();
        public decimal MinAvailablePrice { get; set; }
        public decimal MaxAvailablePrice { get; set; }
        public RoomFilterViewModel Filter { get; set; } = new RoomFilterViewModel();
        public HotelBookingFormViewModel BookingForm { get; set; } = new HotelBookingFormViewModel();
        public List<ServiceOptionViewModel> Services { get; set; } = new List<ServiceOptionViewModel>();
        public IEnumerable<SelectListItem> RoomOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<string> GalleryImages { get; set; } = new List<string>();
        public IEnumerable<CommentViewModel> Comments { get; set; } = new List<CommentViewModel>();
        public IEnumerable<HotelRoomViewModel> SimilarRooms { get; set; } = new List<HotelRoomViewModel>();
    }

    public class HotelRoomViewModel
    {
        public int RoomId { get; set; }
        public int HotelId { get; set; }
        public string Name { get; set; }
        public string RoomType { get; set; }
        public int Capacity { get; set; }
        public double Area { get; set; }
        public decimal PricePerNight { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public List<string> Amenities { get; set; } = new List<string>();
    }

    public class RoomFilterViewModel
    {
        public string Keyword { get; set; }
        public string RoomType { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinCapacity { get; set; }
    }

    public class ServiceOptionViewModel
    {
        public int ServiceId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
    }

    public class HotelBookingFormViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn khách sạn.")]
        public int HotelId { get; set; }

        [Display(Name = "Chọn phòng")]
        [Required(ErrorMessage = "Vui lòng chọn phòng.")]
        public int? RoomId { get; set; }

        [Display(Name = "Ngày nhận phòng")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Vui lòng chọn ngày nhận phòng.")]
        public DateTime? CheckIn { get; set; }

        [Display(Name = "Ngày trả phòng")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Vui lòng chọn ngày trả phòng.")]
        public DateTime? CheckOut { get; set; }

        [Display(Name = "Số khách")]
        [Range(1, 20, ErrorMessage = "Số khách phải từ 1 - 20.")]
        public int Guests { get; set; } = 1;

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; }

        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        public string CustomerName { get; set; }

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string Phone { get; set; }

        [Display(Name = "Địa chỉ")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ.")]
        public string Address { get; set; }

        [Display(Name = "CMND/CCCD")]
        [Required(ErrorMessage = "Vui lòng nhập CMND/CCCD.")]
        public string IdNumber { get; set; }

        [Display(Name = "Đặt cọc (VNĐ)")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "Đặt cọc không hợp lệ.")]
        public decimal? Deposit { get; set; }

        [Display(Name = "Ghi chú")]
        [StringLength(500)]
        public string Notes { get; set; }

        public List<int> SelectedServices { get; set; } = new List<int>();
    }

    public class CommentViewModel
    {
        public string CustomerName { get; set; }
        public string RoomName { get; set; }
        public string Content { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
    public class HotelBookingPageViewModel
    {
        public int HotelId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string HeroImage { get; set; }
        public HotelBookingFormViewModel BookingForm { get; set; } = new HotelBookingFormViewModel();
        public IEnumerable<SelectListItem> RoomOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<ServiceOptionViewModel> Services { get; set; } = new List<ServiceOptionViewModel>();
        public int Guests { get; set; }
        public IEnumerable<string> GalleryImages { get; set; } = new List<string>();
    }

    public class HotelBookingInvoiceViewModel
    {
        public int HotelId { get; set; }
        public string HotelName { get; set; }
        public string HotelLocation { get; set; }
        public string RoomName { get; set; }
        public decimal RoomPricePerNight { get; set; }
        public int Nights { get; set; }
        public decimal RoomTotal { get; set; }
        public decimal ServicesTotal { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal GrandTotal => RoomTotal + ServicesTotal;
        public HotelBookingFormViewModel BookingForm { get; set; } = new HotelBookingFormViewModel();
        public List<ServiceInvoiceItem> Services { get; set; } = new List<ServiceInvoiceItem>();
    }

    public class ServiceInvoiceItem
    {
        public int ServiceId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
