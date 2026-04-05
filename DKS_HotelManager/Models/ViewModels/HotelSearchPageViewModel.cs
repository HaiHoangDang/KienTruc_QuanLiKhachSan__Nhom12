using System.Collections.Generic;

namespace DKS_HotelManager.Models.ViewModels
{
    public class HotelSearchPageViewModel
    {
        public List<HotelCardViewModel> HotelCards { get; set; } = new List<HotelCardViewModel>();
        public List<RoomCardViewModel> Rooms { get; set; } = new List<RoomCardViewModel>();
        public List<HotelOptionViewModel> Hotels { get; set; } = new List<HotelOptionViewModel>();
        public List<string> Destinations { get; set; } = new List<string>();
        public List<string> RoomTypes { get; set; } = new List<string>();
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
    }

    public class HotelCardViewModel
    {
        public int HotelId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string Thumbnail { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? FilteredMinPrice { get; set; }
        public string Badge { get; set; }
        public int RoomCount { get; set; }
    }
}
