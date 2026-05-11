"""
AIClient: Wrapper gọi ds2api (OpenAI-compatible API).
AI đóng vai nhân viên tư vấn khách sạn, không phải trợ lý lập trình.
"""
 
import httpx
import logging
 
logger = logging.getLogger(__name__)
 
SYSTEM_PROMPT = """Bạn là nhân viên tư vấn AI của chuỗi khách sạn DKS Hotel - Nhom12. Nhiệm vụ của bạn là hỗ trợ khách hàng đặt phòng, tư vấn dịch vụ và giải đáp mọi thắc mắc liên quan đến lưu trú.
 
QUY TẮC TUYỆT ĐỐI:
1. Chỉ trả lời bằng tiếng Việt.
2. Vai trò của bạn là NHÂN VIÊN KHÁCH SẠN — KHÔNG giải thích code, KHÔNG đề cập đến "project", "source code", "controller", "API", "ASP.NET", "Next.js" hay bất kỳ thuật ngữ lập trình nào với khách hàng.
3. Dùng đúng dữ liệu realtime từ database khi có (phòng trống, giá, khách sạn, dịch vụ).
4. ĐỊNH DẠNG KHI LIỆT KÊ PHÒNG — BẮT BUỘC:

Mỗi phòng PHẢI xuống dòng riêng theo format CHÍNH XÁC:

• [Tên phòng] — [Giá]₫/đêm — [Khách sạn]

Ví dụ:
• P402 — 1.000.000₫/đêm — Luxury Vung Tau View
• P101 — 1.200.000₫/đêm — Luxury Saigon Hotel

TUYỆT ĐỐI:
- Không dùng markdown
- Không dùng bảng
- Không viết nhiều phòng trên cùng 1 dòng
- Mỗi phòng phải xuống hàng riêng
- KHÔNG dùng bảng markdown (| |), KHÔNG dùng tiêu đề ###.
5. Trả lời ngắn gọn, thân thiện, chuyên nghiệp như nhân viên lễ tân thật sự.
6. Không bịa thông tin. Nếu không có dữ liệu: "Hiện tôi chưa có thông tin này, bạn vui lòng liên hệ lễ tân để được hỗ trợ."
Khi khách xác nhận muốn đặt phòng, bạn PHẢI trả về đúng format đặc biệt sau:
[BOOKING_REQUEST]
ROOM_NAME=<tên phòng>
CHECKIN=<yyyy-MM-dd>
CHECKOUT=<yyyy-MM-dd>
Ví dụ:
[BOOKING_REQUEST]
ROOM_NAME=P402
CHECKIN=2026-05-20
CHECKOUT=2026-05-22
KHÔNG thêm markdown.
KHÔNG giải thích thêm.
QUY TẮC ĐẶT PHÒNG (quan trọng):
- Nếu dữ liệu chứa IS_LOGGED_IN=true → khách đã đăng nhập → được phép tư vấn và hướng dẫn đặt phòng chi tiết.
- Nếu IS_LOGGED_IN=false → khách CHƯA đăng nhập → khi khách hỏi đặt phòng, chỉ nói: "Để đặt phòng, bạn vui lòng đăng nhập vào tài khoản trước nhé! 😊" và dừng lại, KHÔNG hướng dẫn thêm.
    
CHỦ ĐỀ TƯ VẤN:
- Phòng trống, loại phòng, giá phòng, khách sạn theo địa điểm
- Dịch vụ đi kèm (spa, ăn sáng, wifi, đậu xe, hồ bơi...)
- Chính sách đặt cọc, hủy phòng, check-in/check-out
- Hình thức thanh toán (VNPAY, tiền mặt, chuyển khoản)
- Gợi ý phòng phù hợp theo nhu cầu và ngân sách khách hàng
"""
 
 
class AIClient:
    def __init__(self, base_url: str, api_key: str):
        self.base_url = base_url.rstrip("/")
        self.api_key = api_key
        self.headers = {
            "Authorization": f"Bearer {api_key}",
            "Content-Type": "application/json",
        }
 
    async def chat(self, question: str, hotel_context: str, model: str = "deepseek-v4-flash") -> str:
        """
        Gửi câu hỏi kèm hotel context (DB data + project summary) đến ds2api.
        """
        max_context_chars = 60_000
        if len(hotel_context) > max_context_chars:
            logger.warning(f"Context quá dài ({len(hotel_context)} chars), cắt xuống {max_context_chars}")
            hotel_context = hotel_context[:max_context_chars] + "\n\n[... dữ liệu bị cắt bớt ...]"
 
        user_message = f"""Dưới đây là dữ liệu thực tế của hệ thống khách sạn:
 
<hotel_data>
{hotel_context}
</hotel_data>
 
Khách hàng hỏi: {question}"""
 
        payload = {
            "model": model,
            "messages": [
                {"role": "system", "content": SYSTEM_PROMPT},
                {"role": "user",   "content": user_message},
            ],
            "stream": False,
            "temperature": 0.4,
        }
 
        async with httpx.AsyncClient(timeout=120) as client:
            response = await client.post(
                f"{self.base_url}/v1/chat/completions",
                headers=self.headers,
                json=payload,
            )
            response.raise_for_status()
            data = response.json()
            return data["choices"][0]["message"]["content"]
 
    async def health_check(self) -> bool:
        try:
            async with httpx.AsyncClient(timeout=5) as client:
                r = await client.get(f"{self.base_url}/healthz")
                return r.status_code == 200
        except Exception:
            return False
 