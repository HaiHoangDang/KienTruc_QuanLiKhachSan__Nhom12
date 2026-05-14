"""
AIClient: Wrapper gọi ds2api (OpenAI-compatible API).
AI đóng vai nhân viên tư vấn khách sạn, không phải trợ lý lập trình.
"""
 
import httpx
import logging
from typing import Optional
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
- Nếu IS_LOGGED_IN=false → khách CHƯA đăng nhập → khi khách hỏi đặt phòng, chỉ nói: "Để đặt phòng, bạn vui lòng đăng nhập vào tài khoản trước nhé!" và dừng lại, KHÔNG hướng dẫn thêm.
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

    async def chat(
        self,
        question: str,
        hotel_context: str,
        history: Optional[list[dict]] = None,   # ← lịch sử hội thoại
        model: str = "deepseek-v4-flash",
    ) -> str:
        """
        Gửi câu hỏi + hotel context + lịch sử hội thoại → AI trả lời có trí nhớ.

        history: list[{"role": "user"|"assistant", "content": "..."}]
                 do chat_history.get_history() cung cấp.
        """
        max_context_chars = 50_000
        if len(hotel_context) > max_context_chars:
            hotel_context = hotel_context[:max_context_chars] + "\n[... dữ liệu bị cắt bớt ...]"

        # ── Xây dựng messages[] với đầy đủ lịch sử ──────────────────────────
        messages = [{"role": "system", "content": SYSTEM_PROMPT}]

        # Đưa hotel context vào tin nhắn đầu tiên của assistant (invisible to user)
        # để AI luôn có data mới nhất dù đang ở giữa cuộc trò chuyện
        messages.append({
            "role": "user",
            "content": f"[DỮ LIỆU HỆ THỐNG - cập nhật mới nhất]\n{hotel_context}"
        })
        messages.append({
            "role": "assistant",
            "content": "Đã nhận dữ liệu hệ thống. Tôi sẵn sàng hỗ trợ khách hàng."
        })

        # Chèn lịch sử hội thoại (tối đa 20 lượt = 40 messages)
        if history:
            messages.extend(history)

        # Câu hỏi hiện tại
        messages.append({"role": "user", "content": question})

        payload = {
            "model": model,
            "messages": messages,
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
