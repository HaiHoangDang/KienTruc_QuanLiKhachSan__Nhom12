"""
AIClient: Wrapper gọi ds2api (OpenAI-compatible API).
AI đóng vai nhân viên tư vấn khách sạn, không phải trợ lý lập trình.

Luồng trí nhớ:
- ds2api KHÔNG lưu lịch sử (mỗi request = conversation mới, tự xóa ngay sau)
- Lịch sử được duy trì hoàn toàn từ SQLite của chúng ta
- Mỗi request gửi lên gồm:
    1. system prompt (vai trò AI + quy tắc)
    2. hotel_context (trạng thái login + DB data + lịch sử text từ DB)
    3. history[] (các messages gần nhất dạng role/content)
    4. câu hỏi mới của khách
"""

import asyncio
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

QUY TẮC VỀ TRÍ NHỚ VÀ LỊCH SỬ:
- Dữ liệu bạn nhận được có thể chứa "LỊCH SỬ CUỘC TRÒ CHUYỆN TRƯỚC ĐÓ" — đây là các cuộc trao đổi thật giữa bạn và khách hàng trong những lần trước.
- Hãy đọc kỹ lịch sử đó để hiểu ngữ cảnh: khách đã hỏi gì, bạn đã tư vấn gì, có booking nào đang tiến hành không.
- Tiếp tục hỗ trợ mạch lạc, KHÔNG hỏi lại những thông tin khách đã cung cấp trước đó.
- Nếu khách đang giữa quy trình đặt phòng (đã chọn phòng, chưa xác nhận), hãy tiếp tục từ điểm đó.

QUY TẮC ĐẶT PHÒNG (quan trọng):
- Nếu dữ liệu chứa IS_LOGGED_IN=true → khách đã đăng nhập → được phép tư vấn và hướng dẫn đặt phòng chi tiết.
- Nếu IS_LOGGED_IN=false → khách CHƯA đăng nhập → khi khách hỏi đặt phòng, chỉ nói: "Để đặt phòng, bạn vui lòng đăng nhập vào tài khoản trước nhé!" và dừng lại, KHÔNG hướng dẫn thêm.

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

KHÔNG thêm markdown. KHÔNG giải thích thêm.

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
        self.api_key  = api_key
        self.headers  = {
            "Authorization": f"Bearer {api_key}",
            "Content-Type":  "application/json",
        }

    async def chat(
        self,
        question:      str,
        hotel_context: str,
        history:       Optional[list[dict]] = None,
        model:         str = "deepseek-v4-flash",
    ) -> str:
        """
        Gửi request lên ds2api.
        
        ds2api hoạt động stateless — mỗi request tạo conversation mới trên DeepSeek
        rồi xóa ngay sau. Trí nhớ AI được đảm bảo hoàn toàn bởi:
          - hotel_context: chứa lịch sử text từ SQLite (inject vào system message)
          - history[]:     chứa messages gần nhất dạng role/content (gửi kèm payload)
        """
        MAX_CTX  = 4_000   # Giới hạn hotel_context để tránh lỗi ds2api
        MAX_HIST = 20      # Tối đa 10 lượt = 20 messages

        if len(hotel_context) > MAX_CTX:
            hotel_context = hotel_context[:MAX_CTX] + "\n[dữ liệu bị cắt bớt]"

        if history and len(history) > MAX_HIST:
            history = history[-MAX_HIST:]

        # Cấu trúc messages gửi lên ds2api:
        # [system] → [user: hotel_data] → [assistant: greeting] → [history...] → [user: câu hỏi]
        messages = [{"role": "system", "content": SYSTEM_PROMPT}]

        # Inject hotel_context (gồm login state + DB data + lịch sử text)
        messages.append({
            "role":    "user",
            "content": f"<hotel_data>\n{hotel_context}\n</hotel_data>\nBắt đầu hỗ trợ.",
        })
        messages.append({
            "role":    "assistant",
            "content": "Xin chào! Tôi là trợ lý AI của DKS Hotel. Tôi có thể giúp gì cho bạn?",
        })

        # Thêm lịch sử messages gần nhất (dạng role/content)
        # Lưu ý: lịch sử TEXT đã có trong hotel_context, history[] ở đây
        # giúp AI nhận diện đúng turn-taking của conversation
        if history:
            messages.extend(history)

        # Câu hỏi mới nhất của khách
        messages.append({"role": "user", "content": question})

        payload = {
            "model":       model,
            "messages":    messages,
            "stream":      False,
            "temperature": 0.4,
            "max_tokens":  600,
        }

        total_chars = sum(len(m["content"]) for m in messages)
        logger.info(f"Gửi {len(messages)} messages (~{total_chars} chars) lên ds2api")

        # Retry 1 lần nếu lỗi 500 (ds2api đôi khi flaky)
        for attempt in range(2):
            try:
                async with httpx.AsyncClient(timeout=90) as client:
                    response = await client.post(
                        f"{self.base_url}/v1/chat/completions",
                        headers=self.headers,
                        json=payload,
                    )

                    if response.status_code == 500:
                        err = response.text
                        logger.error(f"ds2api 500 (attempt {attempt+1}): {err}")
                        if attempt == 0:
                            await asyncio.sleep(2)
                            continue
                        return self._fallback_reply(question, hotel_context)

                    response.raise_for_status()
                    data = response.json()
                    return data["choices"][0]["message"]["content"]

            except httpx.TimeoutException:
                logger.warning(f"Timeout (attempt {attempt+1})")
                if attempt == 0:
                    continue
                return "Xin lỗi, hệ thống AI đang bận. Vui lòng thử lại sau ít phút."
            except Exception as e:
                logger.error(f"Chat error (attempt {attempt+1}): {e}")
                if attempt == 0:
                    await asyncio.sleep(1)
                    continue
                return self._fallback_reply(question, hotel_context)

    def _fallback_reply(self, question: str, context: str) -> str:
        """
        Trả lời cơ bản từ DB data khi ds2api không khả dụng.
        Tránh để người dùng thấy lỗi kỹ thuật.
        """
        q = question.lower()

        if any(k in q for k in ["phòng trống", "còn phòng", "trống", "giá phòng"]):
            lines = [l for l in context.splitlines() if l.startswith("• ") and "₫/đêm" in l]
            if lines:
                result = "Các phòng trống hiện tại:\n" + "\n".join(lines[:10])
                if len(lines) > 10:
                    result += f"\n... và {len(lines)-10} phòng khác."
                return result

        if any(k in q for k in ["khách sạn", "địa điểm", "ở đâu"]):
            lines = [l for l in context.splitlines()
                     if l.startswith("• ") and "₫/đêm" not in l]
            if lines:
                return "Các khách sạn DKS:\n" + "\n".join(lines[:8])

        if "IS_LOGGED_IN=false" in context and any(k in q for k in ["đặt", "book"]):
            return "Để đặt phòng, bạn vui lòng đăng nhập vào tài khoản trước nhé!"

        return (
            "Xin lỗi, hệ thống AI đang tạm thời gián đoạn. "
            "Vui lòng thử lại sau hoặc liên hệ lễ tân để được hỗ trợ trực tiếp."
        )
