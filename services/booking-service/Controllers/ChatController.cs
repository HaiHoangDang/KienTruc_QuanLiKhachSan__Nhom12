using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using booking_service.Data;
using booking_service.Models;
using booking_service.DTOs;

namespace booking_service.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChatController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            int mkh = request.MKH;

            // tìm conversation theo khách hàng
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(x => x.MKH == mkh);

            // nếu chưa có thì tạo mới
            if (conversation == null)
            {
                conversation = new Conversation
                {
                    MKH = mkh,
                    Title = "Chat khách sạn",
                    CreatedAt = DateTime.Now
                };

                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();
            }

            // lấy lịch sử chat
            var history = await _context.Messages
                .Where(x => x.ConversationId == conversation.Id)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            // build context gửi AI
            string context = "";

            foreach (var msg in history)
            {
                context += $"{msg.Role}: {msg.Content}\n";
            }

            context += $"user: {request.Message}";

            // lưu user message
            var userMessage = new Message
            {
                ConversationId = conversation.Id,
                Role = "user",
                Content = request.Message,
                CreatedAt = DateTime.Now
            };

            _context.Messages.Add(userMessage);
            await _context.SaveChangesAsync();

            // gọi AI-service
            // gọi AI-service
            var httpClient = new HttpClient();

            var aiRequest = new
            {
                question = request.Message,
                context_type = "summary",
                db_context = context
            };

            var response = await httpClient.PostAsJsonAsync(
                "http://localhost:8000/chat",
                aiRequest
            );

            var json = await response.Content.ReadAsStringAsync();

            dynamic result =
                Newtonsoft.Json.JsonConvert.DeserializeObject(json);

            string aiReply =
                result?.answer ?? "AI không phản hồi.";

            // lưu assistant message
            var assistantMessage = new Message
            {
                ConversationId = conversation.Id,
                Role = "assistant",
                Content = aiReply,
                CreatedAt = DateTime.Now
            };

            _context.Messages.Add(assistantMessage);
            await _context.SaveChangesAsync();

            // backup log
            _context.MessageLogs.Add(new MessageLog
            {
                MessageId = assistantMessage.Id,
                MKH = mkh,
                ConversationId = conversation.Id,
                Role = "assistant",
                Content = aiReply,
                LoggedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                reply = aiReply,
                conversationId = conversation.Id
            });
        }
        [HttpGet("history/{mkh}")]
        public async Task<IActionResult> GetHistory(int mkh)
        {
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(x => x.MKH == mkh);

            if (conversation == null)
            {
                return Ok(new List<object>());
            }

            var messages = await _context.Messages
                .Where(x => x.ConversationId == conversation.Id)
                .OrderBy(x => x.CreatedAt)
                .Select(x => new
                {
                    role = x.Role,
                    content = x.Content,
                    createdAt = x.CreatedAt
                })
                .ToListAsync();

            return Ok(messages);
        }
    }
}