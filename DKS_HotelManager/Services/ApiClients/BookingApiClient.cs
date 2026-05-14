using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DKS_HotelManager.Services.ApiClients
{
    public class BookingApiClient
    {
        public async Task<(bool Success, string Message, JObject Data)> CreateBookingAsync(
            string token,
            int maPhong,
            int maNV,
            DateTime ngayVao,
            DateTime ngayTra,
            decimal datCoc)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return (false, "Bạn cần đăng nhập trước khi đặt phòng.", null);
            }

            var payload = new
            {
                maPhong = maPhong,
                maNV = maNV,
                ngayVao = ngayVao,
                ngayTra = ngayTra,
                datCoc = datCoc
            };

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    var json = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(
                        GatewayConfig.BaseUrl + "/api/booking",
                        content
                    );

                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        return (false, ExtractMessage(responseBody), null);
                    }

                    JObject data = null;

                    if (!string.IsNullOrWhiteSpace(responseBody))
                    {
                        data = JObject.Parse(responseBody);
                    }

                    return (true, "Đặt phòng thành công.", data);
                }
            }
            catch (Exception ex)
            {
                return (false, "Không kết nối được booking-service: " + ex.Message, null);
            }
        }

        private string ExtractMessage(string responseBody)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
            {
                return "API không trả về nội dung lỗi.";
            }

            try
            {
                var json = JObject.Parse(responseBody);
                return json["message"]?.ToString() ?? responseBody;
            }
            catch
            {
                return responseBody;
            }
        }
    }
}