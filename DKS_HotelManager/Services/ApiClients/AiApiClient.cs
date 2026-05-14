using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DKS_HotelManager.Services.ApiClients
{
    public class AiApiClient
    {
        public async Task<(bool Success, string Answer)> AskAsync(object payload)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(120);

                    var json = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(
                        GatewayConfig.BaseUrl + "/api/ai/chat",
                        content
                    );

                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        return (false, responseBody);
                    }

                    var result = JObject.Parse(responseBody);

                    var answer =
                        result["answer"]?.ToString()
                        ?? result["reply"]?.ToString()
                        ?? responseBody;

                    return (true, answer);
                }
            }
            catch (Exception ex)
            {
                return (false, "Không kết nối được ai-service: " + ex.Message);
            }
        }
    }
}