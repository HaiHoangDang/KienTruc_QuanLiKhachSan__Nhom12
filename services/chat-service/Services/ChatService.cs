public class ChatService
{
	private readonly HttpClient _http;

	public ChatService(HttpClient http)
	{
		_http = http;
	}

	public async Task<string> Handle(string message)
	{
		var lower = message.ToLower();

		if (lower.Contains("phòng"))
		{
			var res = await _http.GetStringAsync("http://room-service/api/rooms/available");
			return $"Danh sách phòng: {res}";
		}

		if (lower.Contains("dịch vụ"))
		{
			var res = await _http.GetStringAsync("http://room-service/api/services");
			return $"Dịch vụ: {res}";
		}

		return "Xin lỗi, tôi chưa hiểu câu hỏi.";
	}
}