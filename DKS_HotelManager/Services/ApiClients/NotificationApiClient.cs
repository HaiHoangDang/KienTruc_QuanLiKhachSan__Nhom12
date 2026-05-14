using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DKS_HotelManager.Services.ApiClients
{
    public class NotificationApiClient
    {
        public async Task<bool> SendAsync(string receiver, string title, string message, string type)
        {
            var payload = new
            {
                receiver = receiver,
                title = title,
                message = message,
                type = type
            };

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(15);

                    var json = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(
                        GatewayConfig.BaseUrl + "/api/notification/send",
                        content
                    );

                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}