using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DKS_HotelManager.Services.ApiClients
{
    public class PaymentApiClient
    {
        public async Task<(bool Success, string Message, JObject Data)> CreatePaymentAsync(
            int maThue,
            string hinhThucTT,
            decimal thanhTien)
        {
            var payload = new
            {
                maThue = maThue,
                hinhThucTT = hinhThucTT,
                thanhTien = thanhTien
            };

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    var json = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(
                        GatewayConfig.BaseUrl + "/api/payment",
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

                    return (true, "Thanh toán thành công.", data);
                }
            }
            catch (Exception ex)
            {
                return (false, "Không kết nối được payment-service: " + ex.Message, null);
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