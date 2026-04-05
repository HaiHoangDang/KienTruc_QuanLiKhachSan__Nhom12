using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DKS_HotelManager.Areas.Staff.ViewModels
{
    public enum RoomStatusCategory
    {
        All,
        Empty,
        Reserved,
        Occupied,
        Paid,
        CheckedOut
    }

    public class StaffOperationsViewModel
    {
        public int? SelectedHotelId { get; set; }
        public string SelectedHotelName { get; set; }
        public string StaffName { get; set; }
        public DateTime ServerTime { get; set; }
        public string CustomerSearchQuery { get; set; }
        public List<SelectListItem> HotelOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> RoomOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> CheckInBookingOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ServiceBookingOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PaymentBookingOptions { get; set; } = new List<SelectListItem>();
        public List<RoomStatusTile> RoomTiles { get; set; } = new List<RoomStatusTile>();
        public List<RoomStatusSummary> StatusSummaries { get; set; } = new List<RoomStatusSummary>();
        public List<GuestStayInfo> ActiveGuests { get; set; } = new List<GuestStayInfo>();
        public List<ServiceOption> Services { get; set; } = new List<ServiceOption>();
        public List<CustomerRecord> CustomerSearchResults { get; set; } = new List<CustomerRecord>();
        public RoomBookingInputModel BookingInput { get; set; } = new RoomBookingInputModel();
        public PaymentInputModel PaymentInput { get; set; } = new PaymentInputModel();
    }

    public class StaffCheckoutViewModel
    {
        public List<StaffCheckoutCardViewModel> Cards { get; set; } = new List<StaffCheckoutCardViewModel>();
        public string AlertMessage { get; set; }
        public string SelectedHotelName { get; set; }
        public string StaffName { get; set; }
        public decimal TotalRemainingBalance { get; set; }
        public decimal TotalOutstandingBalance { get; set; }
    }

    public class StaffCheckoutCardViewModel
    {
        public int BookingId { get; set; }
        public string ReferenceNumber { get; set; }
        public string HotelName { get; set; }
        public string RoomName { get; set; }
        public string GuestName { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal RemainingBalance { get; set; }
        public decimal OutstandingDue { get; set; }
    }

    public class HotelSelectorViewModel
    {
        public int? SelectedHotelId { get; set; }
        public string SelectedHotelName { get; set; }
        public List<SelectListItem> Hotels { get; set; } = new List<SelectListItem>();
    }

    public class CheckInInputModel
    {
        [Required]
        public int BookingId { get; set; }

        [DataType(DataType.Date)]
        public DateTime? CheckIn { get; set; }

        [DataType(DataType.Date)]
        public DateTime? CheckOut { get; set; }

        [DataType(DataType.Currency)]
        public decimal? Deposit { get; set; }
    }

    public class CheckInReservationEditModel
    {
        [Required]
        public int BookingId { get; set; }

        [DataType(DataType.Date)]
        public DateTime? CheckIn { get; set; }

        [DataType(DataType.Date)]
        public DateTime? CheckOut { get; set; }

        [DataType(DataType.Currency)]
        public decimal? Deposit { get; set; }
    }

    public class ChangeRoomInputModel
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        public int TargetRoomId { get; set; }
    }

    public class CheckInViewModel
    {
        public int? SelectedHotelId { get; set; }
        public string SelectedHotelName { get; set; }
        public string StaffName { get; set; }
        public List<SelectListItem> BookingOptions { get; set; } = new List<SelectListItem>();
        public List<CheckInBookingInfo> ReservedBookings { get; set; } = new List<CheckInBookingInfo>();
        public List<SelectListItem> AvailableRoomOptions { get; set; } = new List<SelectListItem>();
    }

    public class CheckInBookingInfo
    {
        public int BookingId { get; set; }
        public string RoomCode { get; set; }
        public string RoomName { get; set; }
        public string GuestName { get; set; }
        public string BookingCode { get; set; }
        public DateTime? BookingTime { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public decimal EstimatedTotal { get; set; }
        public decimal Deposit { get; set; }
        public decimal RoomRate { get; set; }
        public string Status { get; set; }
    }

    public class DeskBookingViewModel
    {
        public int? SelectedHotelId { get; set; }
        public string SelectedHotelName { get; set; }
        public string StaffName { get; set; }
        public RoomBookingInputModel BookingInput { get; set; } = new RoomBookingInputModel();
        public List<RoomOptionInfo> RoomOptionsWithRates { get; set; } = new List<RoomOptionInfo>();
        public List<SelectListItem> RoomOptions { get; set; } = new List<SelectListItem>();
    }

    public class RoomOptionInfo
    {
        public int RoomId { get; set; }
        public string Text { get; set; }
        public decimal RatePerNight { get; set; }
    }

    public class RoomBookingInputModel
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc.")]
        public string CustomerName { get; set; }

        public string Phone { get; set; }
        public string Email { get; set; }
        public string IdentityNumber { get; set; }
        public int RoomId { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public decimal? Deposit { get; set; }
    }

    public class ServicePrebookingInputModel
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        [Required]
        [DataType(DataType.Date)]
        public DateTime? ScheduledDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan? ScheduledTime { get; set; }

        public DateTime? ScheduledAt { get; set; }
    }

    public class ServicePrebookingViewModel
    {
        public int? SelectedHotelId { get; set; }
        public string SelectedHotelName { get; set; }
        public string StaffName { get; set; }
        public ServicePrebookingInputModel Input { get; set; } = new ServicePrebookingInputModel();
        public List<SelectListItem> BookingOptions { get; set; } = new List<SelectListItem>();
        public List<ServiceOption> Services { get; set; } = new List<ServiceOption>();
    }

    public class ServiceOption
    {
        public int ServiceId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class PaymentInputModel
    {
        public int BookingId { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class StaffPaymentConfirmViewModel
    {
        public int BookingId { get; set; }
        public string ReferenceNumber { get; set; }
        public string HotelName { get; set; }
        public string RoomName { get; set; }
        public string GuestName { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public decimal RoomTotal { get; set; }
        public decimal ServicesTotal { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal RemainingBefore { get; set; }
        public decimal PaidAmount { get; set; }
        public string TransactionNo { get; set; }
        public string TxnRef { get; set; }
        public string PayDate { get; set; }
    }

    public class RoomStatusSummary
    {
        public string Label { get; set; }
        public RoomStatusCategory Category { get; set; }
        public int Count { get; set; }
    }

    public class RoomStatusTile
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public RoomStatusCategory Category { get; set; }
        public string BadgeText { get; set; }
        public string Detail { get; set; }
    }

    public class GuestStayInfo
    {
        public int RoomId { get; set; }
        public int BookingId { get; set; }
        public string RoomName { get; set; }
        public string GuestName { get; set; }
        public string BookingCode { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public string Status { get; set; }
    }

    public class CustomerRecord
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string IdentityNumber { get; set; }
        public string Email { get; set; }
    }
}
