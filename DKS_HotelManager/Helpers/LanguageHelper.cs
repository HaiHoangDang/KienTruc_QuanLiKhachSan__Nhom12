using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;

namespace DKS_HotelManager.Helpers
{
    public static class LanguageHelper
    {
        private static Dictionary<string, Dictionary<string, string>> translations = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "vi", new Dictionary<string, string>
                {
                    {"Login", "Đăng Nhập"},
                    {"Register", "Đăng Ký"},
                    {"Logout", "Đăng Xuất"},
                    {"Home", "Trang Chủ"},
                    {"Hotels", "Khách Sạn"},
                    {"About", "Giới Thiệu"},
                    {"Contact", "Liên Hệ"},
                    {"Comments", "Bình luận"},
                    {"Settings", "Cài Đặt"},
                    {"Language", "Ngôn Ngữ"},
                    {"Vietnamese", "Tiếng Việt"},
                    {"English", "Tiếng Anh"},
                    {"FullName", "Họ và Tên"},
                    {"PhoneNumber", "Số Điện Thoại"},
                    {"IDCard", "CMND/CCCD"},
                    {"Address", "Địa Chỉ"},
                    {"Password", "Mật Khẩu"},
                    {"RememberMe", "Ghi nhớ đăng nhập"},
                    {"Captcha", "Mã Xác Nhận (CAPTCHA)"},
                    {"EnterFullName", "Nhập họ và tên"},
                    {"EnterPhoneNumber", "Nhập số điện thoại"},
                    {"EnterIDCard", "Nhập số CMND/CCCD"},
                    {"EnterAddress", "Nhập địa chỉ"},
                    {"EnterPassword", "Nhập mật khẩu"},
                    {"EnterCaptcha", "Nhập mã xác nhận"},
                    {"CreateAccount", "Tạo tài khoản mới để sử dụng dịch vụ"},
                    {"PleaseLogin", "Vui lòng đăng nhập để tiếp tục"},
                    {"AlreadyHaveAccount", "Đã có tài khoản? Đăng nhập ngay"},
                    {"NoAccount", "Chưa có tài khoản? Đăng ký ngay"},
                    {"WelcomeSettings", "Chào mừng đến với Cài Đặt"},
                    {"SelectOption", "Vui lòng chọn một tùy chọn từ menu bên trái để bắt đầu"},
                    {"ChangeLanguage", "Chuyển Đổi Ngôn Ngữ"},
                    {"SelectLanguage", "Chọn ngôn ngữ hiển thị cho trang web của bạn"},
                    {"Account", "Tài Khoản"},
                    {"Services", "Dịch Vụ"},
                    {"BookRoom", "Đặt Phòng"},
                    {"ExploreHotels", "Khám Phá Khách Sạn"},
                    {"LuxuryHotels", "LUXURY HOTELS"},
                    {"WorldClassExperience", "Trải nghiệm sang trọng đẳng cấp thế giới"},
                    {"ExploreHotelsButton", "Khám Phá Khách Sạn"},
                    {"HotelsCount", "Khách Sạn"},
                    {"RoomsCount", "Phòng Nghỉ"},
                    {"CustomersCount", "Khách Hàng"},
                    {"SatisfactionRate", "Hài Lòng"},
                    {"WhyChooseUs", "Tại Sao Chọn Luxury Hotels?"},
                    {"FiveStarHotels", "Khách Sạn 5 Sao"},
                    {"FiveStarHotelsDesc", "Chuỗi khách sạn sang trọng với tiêu chuẩn quốc tế, đảm bảo trải nghiệm tuyệt vời nhất cho quý khách."},
                    {"PremiumService", "Dịch Vụ Đẳng Cấp"},
                    {"PremiumServiceDesc", "Đội ngũ nhân viên chuyên nghiệp, tận tâm phục vụ 24/7 để đáp ứng mọi nhu cầu của quý khách."},
                    {"MultipleLocations", "Nhiều Địa Điểm"},
                    {"MultipleLocationsDesc", "Hệ thống khách sạn rộng khắp tại các thành phố lớn, thuận tiện cho mọi chuyến đi của bạn."},
                    {"LuxuryRooms", "Phòng Nghỉ Sang Trọng"},
                    {"LuxuryRoomsDesc", "Phòng nghỉ được thiết kế tinh tế với đầy đủ tiện nghi hiện đại, không gian thoáng đãng và view đẹp."},
                    {"DiverseCuisine", "Ẩm Thực Đa Dạng"},
                    {"DiverseCuisineDesc", "Nhà hàng với menu đa dạng từ ẩm thực Á đến Âu, đảm bảo hương vị tuyệt hảo."},
                    {"SpecialValue", "Giá Trị Đặc Biệt"},
                    {"SpecialValueDesc", "Nhiều ưu đãi hấp dẫn, chương trình khách hàng thân thiết với nhiều đặc quyền độc quyền."},
                    {"ExploreOurHotels", "Khám Phá Khách Sạn Của Chúng Tôi"},
                    {"ViewAllHotels", "Xem Tất Cả Khách Sạn"},
                    {"AboutUs", "Về Chúng Tôi"},
                    {"AboutUsSubtitle", "Khám phá câu chuyện đằng sau Luxury Hotels"},
                    {"WelcomeToLuxuryHotels", "Chào Mừng Đến Với Luxury Hotels"},
                    {"AboutUsDescription", "Chúng tôi tự hào là một trong những chuỗi khách sạn hàng đầu, mang đến trải nghiệm nghỉ dưỡng đẳng cấp thế giới với dịch vụ tận tâm và không gian sang trọng."},
                    {"OurStory", "Câu Chuyện Của Chúng Tôi"},
                    {"OurStoryDesc1", "Luxury Hotels được thành lập với tầm nhìn tạo ra những không gian nghỉ dưỡng đẳng cấp, nơi mỗi khách hàng đều cảm nhận được sự chăm sóc tận tâm và trải nghiệm dịch vụ hoàn hảo. Với hơn 20 năm kinh nghiệm trong ngành khách sạn, chúng tôi đã xây dựng được một hệ thống khách sạn rộng khắp tại các thành phố lớn."},
                    {"OurStoryDesc2", "Từ những ngày đầu tiên, chúng tôi đã đặt chất lượng dịch vụ và sự hài lòng của khách hàng lên hàng đầu. Mỗi khách sạn trong hệ thống của chúng tôi đều được thiết kế với tiêu chuẩn 5 sao quốc tế, kết hợp giữa vẻ đẹp hiện đại và nét sang trọng truyền thống."},
                    {"InternationalAwards", "Giải Thưởng Quốc Tế"},
                    {"InternationalAwardsDesc", "Được công nhận là khách sạn tốt nhất trong khu vực với nhiều giải thưởng danh giá"},
                    {"FiveStarService", "Dịch Vụ 5 Sao"},
                    {"FiveStarServiceDesc", "Tiêu chuẩn dịch vụ đẳng cấp thế giới với đội ngũ nhân viên chuyên nghiệp, tận tâm"},
                    {"ModernAmenities", "Tiện Nghi Hiện Đại"},
                    {"ModernAmenitiesDesc", "Phòng nghỉ được trang bị đầy đủ tiện nghi cao cấp, đảm bảo sự thoải mái tuyệt đối"},
                    {"QualityCommitment", "Cam Kết Chất Lượng"},
                    {"QualityCommitmentDesc", "Cam kết mang đến trải nghiệm nghỉ dưỡng hoàn hảo và đáng nhớ cho mọi khách hàng"},
                    {"MissionVision", "Sứ Mệnh & Tầm Nhìn"},
                    {"Mission", "Sứ Mệnh"},
                    {"MissionDesc", "Chúng tôi cam kết mang đến trải nghiệm nghỉ dưỡng đẳng cấp thế giới cho mọi khách hàng. Với dịch vụ tận tâm, không gian sang trọng và tiện nghi hiện đại, chúng tôi luôn nỗ lực vượt qua mong đợi của khách hàng và tạo ra những kỷ niệm đáng nhớ."},
                    {"Vision", "Tầm Nhìn"},
                    {"VisionDesc", "Trở thành chuỗi khách sạn hàng đầu trong khu vực, được công nhận về chất lượng dịch vụ xuất sắc và sự đổi mới không ngừng. Chúng tôi hướng tới việc mở rộng hệ thống và nâng cao tiêu chuẩn dịch vụ, đóng góp vào sự phát triển của ngành du lịch và khách sạn."},
                    {"ReadyToExperience", "Sẵn Sàng Trải Nghiệm?"},
                    {"ReadyToExperienceDesc", "Hãy để chúng tôi mang đến cho bạn kỳ nghỉ đáng nhớ nhất"},
                    {"ContactUs", "Liên Hệ"},
                    {"ContactInfo", "Thông Tin Liên Hệ"},
                    {"Support", "Hỗ Trợ"},
                    {"Marketing", "Marketing"},
                    {"Phone", "Điện Thoại"},
                    {"Email", "Email"},
                    {"OurServices", "Dịch Vụ Của Chúng Tôi"},
                    {"Message", "Tin Nhắn"},
                    {"SendMessage", "Gửi Tin Nhắn"},
                    {"SelectHotel", "Chọn Khách Sạn"},
                    {"HideFilters", "Ẩn Bộ Lọc"},
                    {"Guests", "Khách"},
                    {"CheckIn", "Nhận Phòng"},
                    {"CheckOut", "Trả Phòng"},
                    {"Location", "Địa Điểm"},
                    {"ChooseDate", "Chọn Ngày"},
                    {"ChooseLocation", "Chọn Địa Điểm"},
                    {"PlanTripTitle", "Lên kế hoạch dễ dàng và nhanh chóng"},
                    {"PlanTripSubtitle", "Chọn điểm đến, phòng nghỉ và đặt chỗ chỉ trong vài giây"},
                    {"PopularDestinations", "Điểm đến đang thịnh hành"},
                    {"TrendingRooms", "Phòng nổi bật"},
                    {"BookNow", "Đặt Ngay"},
                    {"Detail", "Chi Tiết"},
                    {"Average", "Trung Bình"},
                    {"PerNight", "Mỗi Đêm"},
                    {"NoHotelsFound", "Không tìm thấy khách sạn phù hợp"},
                    {"RoomList", "Danh Sách Phòng"},
                    {"AddNewRoom", "Thêm Phòng Mới"},
                    {"RoomCode", "Mã Phòng"},
                    {"RoomName", "Tên Phòng"},
                    {"Hotel", "Khách Sạn"},
                    {"RoomType", "Loại Phòng"},
                    {"Capacity", "Sức Chứa"},
                    {"Floor", "Tầng"},
                    {"Area", "Diện Tích"},
                    {"PricePerDay", "Giá/Ngày"},
                    {"Actions", "Thao Tác"},
                    {"Details", "Chi Tiết"},
                    {"Edit", "Sửa"},
                    {"Delete", "Xóa"},
                    {"Username", "Tên đăng nhập"},
                    {"EnterUsername", "Nhập tên đăng nhập"},
                    {"EmailOptional", "Email (không bắt buộc)"},
                    {"ConfirmPassword", "Xác nhận mật khẩu"},
                    {"EnterConfirmPassword", "Nhập lại mật khẩu"},
                    {"CurrentPassword", "Mật khẩu hiện tại"},
                    {"NewPassword", "Mật khẩu mới"},
                    {"ConfirmNewPassword", "Xác nhận mật khẩu mới"},
                    {"UpdateProfile", "Cập nhật thông tin"},
                    {"SaveChanges", "Lưu thay đổi"},
                    {"Bookings", "Đặt phòng"},
                    {"BookingCode", "Mã đặt phòng"},
                    {"HotelName", "Khách sạn"},
                    {"RoomNameShort", "Phòng"},
                    {"StayDates", "Nhận/Trả"},
                    {"EditBooking", "Chỉnh sửa"},
                    {"CancelBooking", "Hủy"},
                    {"NoBookings", "Bạn chưa có đặt phòng nào"},
                    {"ViewDetails", "Xem chi tiết"},
                    {"AccountHeading", "Tài khoản của bạn"},
                    {"AccountSubtitle", "Cập nhật thông tin cá nhân và quản lý các đặt phòng đã thực hiện."},
                    {"ChangePassword", "Đổi mật khẩu"},
                    {"Status", "Trạng thái"},
                    {"YourBookings", "Đặt phòng của bạn"},
                    {"BookingsLabel", "đặt phòng"},

                    // Chatbot & booking/payment related
                    {"DepositPolicy", "Tiền đặt cọc khoảng 30% tổng giá trị đơn, thường không hoàn lại khi hủy sát giờ nhận phòng."},
                    {"CancellationPolicy", "Bạn nên hủy hoặc thay đổi đặt phòng trước ít nhất 24 giờ để tránh phát sinh chi phí."},
                    {"PaymentMethods", "Thanh toán qua VNPAY, tiền mặt tại quầy và một số hình thức khác theo từng khách sạn."},
                    {"AvailableRoomsSummary", "Tổng số phòng, số phòng trống và số phòng đang được giữ chỗ."},

                    // Generic UI phrases from views
                    {"SendRequest", "Gửi yêu cầu"},
                    {"LeaveComment", "Gửi bình luận"},
                    {"LatestComments", "Bình luận mới nhất"},
                    {"NoComments", "Chưa có bình luận nào."},
                    {"OptionalHotelSelect", "Chọn khách sạn (tùy chọn)"},
                    {"CommentContent", "Nội dung bình luận"},
                    {"SendComment", "Gửi bình luận"},

                    {"ContactQuick", "Liên hệ nhanh"},
                    {"ContactUsTitle", "Liên hệ với chúng tôi"},
                    {"ContactUsSubtitle", "Đội ngũ hỗ trợ luôn sẵn sàng phục vụ 24/7; hãy gọi hotline hoặc gửi email để được tư vấn trực tiếp."},
                    {"DirectContactInfo", "Thông tin liên hệ trực tiếp"},

                    {"BookingInfoTitle", "Thông tin đặt phòng"},
                    {"BookingSummary", "Thông tin nhanh"},
                    {"BookingCheckinDate", "Ngày nhận phòng"},
                    {"BookingCheckoutDate", "Ngày trả phòng"},
                    {"BookingGuestsCount", "Số khách dự kiến"},
                    {"BookingNote", "Ghi chú"},

                    {"RegionNorth", "Miền Bắc"},
                    {"RegionCentral", "Miền Trung"},
                    {"RegionSouth", "Miền Nam"}
                }
            },
            {
                "en", new Dictionary<string, string>
                {
                    {"Login", "Login"},
                    {"Register", "Register"},
                    {"Logout", "Logout"},
                    {"Home", "Home"},
                    {"Hotels", "Hotels"},
                    {"About", "About"},
                    {"Contact", "Contact"},
                    {"Comments", "Comments"},
                    {"Settings", "Settings"},
                    {"Language", "Language"},
                    {"Vietnamese", "Vietnamese"},
                    {"English", "English"},
                    {"FullName", "Full Name"},
                    {"PhoneNumber", "Phone Number"},
                    {"IDCard", "ID Card/CCCD"},
                    {"Address", "Address"},
                    {"Password", "Password"},
                    {"RememberMe", "Remember me"},
                    {"Captcha", "Verification Code (CAPTCHA)"},
                    {"EnterFullName", "Enter full name"},
                    {"EnterPhoneNumber", "Enter phone number"},
                    {"EnterIDCard", "Enter ID Card/CCCD number"},
                    {"EnterAddress", "Enter address"},
                    {"EnterPassword", "Enter password"},
                    {"EnterCaptcha", "Enter verification code"},
                    {"CreateAccount", "Create a new account to use the service"},
                    {"PleaseLogin", "Please login to continue"},
                    {"AlreadyHaveAccount", "Already have an account? Login now"},
                    {"NoAccount", "Don't have an account? Register now"},
                    {"WelcomeSettings", "Welcome to Settings"},
                    {"SelectOption", "Please select an option from the left menu to get started"},
                    {"ChangeLanguage", "Change Language"},
                    {"SelectLanguage", "Select the display language for your website"},
                    {"Account", "Account"},
                    {"Services", "Services"},
                    {"BookRoom", "Book Room"},
                    {"ExploreHotels", "Explore Hotels"},
                    {"LuxuryHotels", "LUXURY HOTELS"},
                    {"WorldClassExperience", "World-class luxury experience"},
                    {"ExploreHotelsButton", "Explore Hotels"},
                    {"HotelsCount", "Hotels"},
                    {"RoomsCount", "Rooms"},
                    {"CustomersCount", "Customers"},
                    {"SatisfactionRate", "Satisfaction"},
                    {"WhyChooseUs", "Why Choose Luxury Hotels?"},
                    {"FiveStarHotels", "5-Star Hotels"},
                    {"FiveStarHotelsDesc", "Luxury hotel chain with international standards, ensuring the best experience for our guests."},
                    {"PremiumService", "Premium Service"},
                    {"PremiumServiceDesc", "Professional staff team, dedicated 24/7 service to meet all your needs."},
                    {"MultipleLocations", "Multiple Locations"},
                    {"MultipleLocationsDesc", "Hotel system spread across major cities, convenient for all your trips."},
                    {"LuxuryRooms", "Luxury Rooms"},
                    {"LuxuryRoomsDesc", "Elegantly designed rooms with full modern amenities, spacious space and beautiful views."},
                    {"DiverseCuisine", "Diverse Cuisine"},
                    {"DiverseCuisineDesc", "Restaurant with diverse menu from Asian to European cuisine, ensuring excellent flavors."},
                    {"SpecialValue", "Special Value"},
                    {"SpecialValueDesc", "Many attractive offers, loyalty program with many exclusive privileges."},
                    {"ExploreOurHotels", "Explore Our Hotels"},
                    {"ViewAllHotels", "View All Hotels"},
                    {"AboutUs", "About Us"},
                    {"AboutUsSubtitle", "Discover the story behind Luxury Hotels"},
                    {"WelcomeToLuxuryHotels", "Welcome to Luxury Hotels"},
                    {"AboutUsDescription", "We are proud to be one of the leading hotel chains, bringing world-class resort experiences with dedicated service and luxurious spaces."},
                    {"OurStory", "Our Story"},
                    {"OurStoryDesc1", "Luxury Hotels was founded with the vision of creating world-class resort spaces where every customer feels dedicated care and perfect service experience. With over 20 years of experience in the hotel industry, we have built a hotel system spread across major cities."},
                    {"OurStoryDesc2", "From the very first days, we have placed service quality and customer satisfaction first. Every hotel in our system is designed with international 5-star standards, combining modern beauty and traditional luxury."},
                    {"InternationalAwards", "International Awards"},
                    {"InternationalAwardsDesc", "Recognized as the best hotel in the region with many prestigious awards"},
                    {"FiveStarService", "5-Star Service"},
                    {"FiveStarServiceDesc", "World-class service standards with professional, dedicated staff"},
                    {"ModernAmenities", "Modern Amenities"},
                    {"ModernAmenitiesDesc", "Rooms fully equipped with premium amenities, ensuring absolute comfort"},
                    {"QualityCommitment", "Quality Commitment"},
                    {"QualityCommitmentDesc", "Committed to bringing perfect and memorable resort experiences to all customers"},
                    {"MissionVision", "Mission & Vision"},
                    {"Mission", "Mission"},
                    {"MissionDesc", "We are committed to bringing world-class resort experiences to all customers. With dedicated service, luxurious spaces and modern amenities, we always strive to exceed customer expectations and create memorable memories."},
                    {"Vision", "Vision"},
                    {"VisionDesc", "Become the leading hotel chain in the region, recognized for excellent service quality and continuous innovation. We aim to expand the system and improve service standards, contributing to the development of the tourism and hotel industry."},
                    {"ReadyToExperience", "Ready to Experience?"},
                    {"ReadyToExperienceDesc", "Let us bring you the most memorable vacation"},
                    {"ContactUs", "Contact Us"},
                    {"ContactInfo", "Contact Information"},
                    {"Support", "Support"},
                    {"Marketing", "Marketing"},
                    {"Phone", "Phone"},
                    {"Email", "Email"},
                    {"OurServices", "Our Services"},
                    {"Message", "Message"},
                    {"SendMessage", "Send Message"},
                    {"SelectHotel", "Select a Hotel"},
                    {"HideFilters", "Hide Filters"},
                    {"Guests", "Guests"},
                    {"CheckIn", "Check-In"},
                    {"CheckOut", "Check-Out"},
                    {"Location", "Location"},
                    {"ChooseDate", "Choose Date"},
                    {"ChooseLocation", "Choose The Location"},
                    {"PlanTripTitle", "Plan your trip easily and quickly"},
                    {"PlanTripSubtitle", "Pick a destination, room, and reserve in seconds"},
                    {"PopularDestinations", "Popular destinations"},
                    {"TrendingRooms", "Trending rooms"},
                    {"BookNow", "Book now"},
                    {"Detail", "Detail"},
                    {"Average", "Average"},
                    {"PerNight", "Per Night"},
                    {"NoHotelsFound", "We couldn't find any matching hotels"},
                    {"RoomList", "Room List"},
                    {"AddNewRoom", "Add New Room"},
                    {"RoomCode", "Room Code"},
                    {"RoomName", "Room Name"},
                    {"Hotel", "Hotel"},
                    {"RoomType", "Room Type"},
                    {"Capacity", "Capacity"},
                    {"Floor", "Floor"},
                    {"Area", "Area"},
                    {"PricePerDay", "Price/Day"},
                    {"Actions", "Actions"},
                    {"Details", "Details"},
                    {"Edit", "Edit"},
                    {"Delete", "Delete"},
                    {"Username", "Username"},
                    {"EnterUsername", "Enter username"},
                    {"EmailOptional", "Email (optional)"},
                    {"ConfirmPassword", "Confirm password"},
                    {"EnterConfirmPassword", "Enter password again"},
                    {"CurrentPassword", "Current password"},
                    {"NewPassword", "New password"},
                    {"ConfirmNewPassword", "Confirm new password"},
                    {"UpdateProfile", "Update profile"},
                    {"SaveChanges", "Save changes"},
                    {"Bookings", "Bookings"},
                    {"BookingCode", "Booking code"},
                    {"HotelName", "Hotel"},
                    {"RoomNameShort", "Room"},
                    {"StayDates", "Check-in/Check-out"},
                    {"EditBooking", "Edit booking"},
                    {"CancelBooking", "Cancel"},
                    {"NoBookings", "You have no bookings yet"},
                    {"ViewDetails", "View details"},
                    {"AccountHeading", "Your account"},
                    {"AccountSubtitle", "Update your personal information and manage your bookings."},
                    {"ChangePassword", "Change password"},
                    {"Status", "Status"},
                    {"YourBookings", "Your bookings"},
                    {"BookingsLabel", "bookings"},

                    // Chatbot & booking/payment related
                    {"DepositPolicy", "The deposit is about 30% of the total stay cost and is usually non‑refundable for late cancellations."},
                    {"CancellationPolicy", "You should cancel or modify your reservation at least 24 hours before check‑in to avoid extra charges."},
                    {"PaymentMethods", "We accept VNPAY online payments, cash at the front desk and, in some hotels, bank transfer."},
                    {"AvailableRoomsSummary", "Total rooms, available rooms and rooms currently reserved or occupied."},

                    // Generic UI phrases from views
                    {"SendRequest", "Send request"},
                    {"LeaveComment", "Leave a comment"},
                    {"LatestComments", "Latest comments"},
                    {"NoComments", "There are no comments yet."},
                    {"OptionalHotelSelect", "Select a hotel (optional)"},
                    {"CommentContent", "Comment content"},
                    {"SendComment", "Send comment"},

                    {"ContactQuick", "Quick contact"},
                    {"ContactUsTitle", "Contact us"},
                    {"ContactUsSubtitle", "Our support team is available 24/7; call our hotline or send an email for immediate assistance."},
                    {"DirectContactInfo", "Direct contact information"},

                    {"BookingInfoTitle", "Booking information"},
                    {"BookingSummary", "Quick summary"},
                    {"BookingCheckinDate", "Check-in date"},
                    {"BookingCheckoutDate", "Check-out date"},
                    {"BookingGuestsCount", "Number of guests"},
                    {"BookingNote", "Notes"},

                    {"RegionNorth", "Northern Vietnam"},
                    {"RegionCentral", "Central Vietnam"},
                    {"RegionSouth", "Southern Vietnam"}
                }
            }
        };

        private const string DefaultLanguageCode = "vi";
        private static readonly Dictionary<string, LanguageDefinition> languageDefinitions;

        static LanguageHelper()
        {
            languageDefinitions = new Dictionary<string, LanguageDefinition>(StringComparer.OrdinalIgnoreCase)
            {
                { "vi", new LanguageDefinition("vi", "Tiếng Việt", "vi-VN", translations["vi"]) },
                { "en", new LanguageDefinition("en", "English", "en-US", translations["en"]) }
            };
        }

        public static IReadOnlyDictionary<string, LanguageDefinition> AvailableLanguages => languageDefinitions;

        public static string GetCurrentLanguage()
        {
            var langFromSession = HttpContext.Current?.Session?["Language"] as string;
            if (!string.IsNullOrWhiteSpace(langFromSession) && languageDefinitions.ContainsKey(langFromSession))
            {
                return langFromSession;
            }

            var langFromCookie = HttpContext.Current?.Request?.Cookies?["preferredLanguage"]?.Value;
            if (!string.IsNullOrWhiteSpace(langFromCookie) && languageDefinitions.ContainsKey(langFromCookie))
            {
                return langFromCookie;
            }

            return DefaultLanguageCode;
        }

        public static string Translate(string key, string languageCode = null)
        {
            var lang = !string.IsNullOrWhiteSpace(languageCode) && languageDefinitions.ContainsKey(languageCode)
                ? languageCode
                : GetCurrentLanguage();

            if (languageDefinitions.TryGetValue(lang, out var definition) && definition.Entries.TryGetValue(key, out var value))
            {
                return value;
            }

            if (languageDefinitions.TryGetValue(DefaultLanguageCode, out var fallback) && fallback.Entries.TryGetValue(key, out var fallbackValue))
            {
                return fallbackValue;
            }

            return key;
        }

        public static void SetLanguage(string lang)
        {
            if (string.IsNullOrWhiteSpace(lang) || !languageDefinitions.ContainsKey(lang))
            {
                lang = DefaultLanguageCode;
            }

            if (HttpContext.Current?.Session != null)
            {
                HttpContext.Current.Session["Language"] = lang;
            }

            var cookie = new HttpCookie("preferredLanguage", lang)
            {
                HttpOnly = true,
                Expires = DateTime.Now.AddDays(30)
            };
            HttpContext.Current?.Response?.Cookies?.Add(cookie);

            var culture = new CultureInfo(languageDefinitions[lang].CultureName);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        public static bool RegisterLanguage(string code, string displayName, string cultureName, Dictionary<string, string> entries)
        {
            if (string.IsNullOrWhiteSpace(code) || entries == null || entries.Count == 0)
            {
                return false;
            }

            languageDefinitions[code] = new LanguageDefinition(code, displayName ?? code, cultureName ?? "en-US", entries);
            return true;
        }

        public class LanguageDefinition
        {
            public string Code { get; }
            public string DisplayName { get; }
            public string CultureName { get; }
            public IReadOnlyDictionary<string, string> Entries { get; }

            public LanguageDefinition(string code, string displayName, string cultureName, IReadOnlyDictionary<string, string> entries)
            {
                Code = code;
                DisplayName = displayName;
                CultureName = cultureName;
                Entries = entries ?? new Dictionary<string, string>();
            }
        }
    }
}

