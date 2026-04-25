[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
	private readonly ChatService _chatService;

	public ChatController(ChatService chatService)
	{
		_chatService = chatService;
	}

	[HttpPost]
	public async Task<IActionResult> Ask([FromBody] ChatRequest request)
	{
		var reply = await _chatService.Handle(request.Message);
		return Ok(new { reply });
	}
}