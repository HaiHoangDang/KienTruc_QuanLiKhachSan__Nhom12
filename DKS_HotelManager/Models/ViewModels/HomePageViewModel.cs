using System.Collections.Generic;
using System.Linq;

namespace DKS_HotelManager.Models.ViewModels
{
    public class HomePageViewModel
    {
        public List<RoomCardViewModel> FeaturedRooms { get; set; } = new List<RoomCardViewModel>();
        public List<DestinationCardViewModel> Destinations { get; set; } = new List<DestinationCardViewModel>();
        public List<StatCardViewModel> Stats { get; set; } = new List<StatCardViewModel>();
        public List<HotelCardViewModel> FeaturedHotels { get; set; } = new List<HotelCardViewModel>();
        public List<HotelOptionViewModel> HotelOptions { get; set; } = new List<HotelOptionViewModel>();

        public RoomCardViewModel HeroRoom => FeaturedRooms?.FirstOrDefault();
    }

    public class RoomCardViewModel
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public int HotelId { get; set; }
        public string HotelName { get; set; }
        public string Location { get; set; }
        public string RoomType { get; set; }
        public decimal PricePerNight { get; set; }
        public string ImageUrl { get; set; }
        public string Status { get; set; }
        public string AvailabilityBadge { get; set; }
        public int Capacity { get; set; }
        public double Area { get; set; }
        public List<string> Amenities { get; set; } = new List<string>();
        public int BookingCount { get; set; }
        public string RatingSummary { get; set; }
    }

    public class DestinationCardViewModel
    {
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string ImageUrl { get; set; }
    }

    public class StatCardViewModel
    {
        public string LabelKey { get; set; }
        public string Value { get; set; }
    }

    public class HotelOptionViewModel
    {
        public int HotelId { get; set; }
        public string HotelName { get; set; }
        public string Location { get; set; }
    }
}
