"""
AIClient: Wrapper gọi ds2api (OpenAI-compatible API).
Ràng buộc model trả lời bằng tiếng Việt, hiểu context khách sạn.
"""

import httpx
import logging

logger = logging.getLogger(__name__)

SYSTEM_PROMPT = """Bạn là trợ lý AI thông minh của hệ thống Quản Lí Khách Sạn (DKS Hotel Manager) - Nhóm 12.

QUY TẮC BẮT BUỘC:
1. LUÔN trả lời bằng tiếng Việt, dù câu hỏi được viết bằng ngôn ngữ nào khác.
2. Phân tích và trả lời DỰA TRÊN source code và dữ liệu project được cung cấp trong context.
3. Khi đề cập đến tên class, method, endpoint kỹ thuật thì giữ nguyên tên gốc (tiếng Anh) nhưng giải thích bằng tiếng Việt.
4. Nếu không tìm thấy thông tin trong project, hãy nói rõ: "Thông tin này không có trong project".
5. Khi giải thích code C# (ASP.NET) hoặc JavaScript/React, dùng ví dụ cụ thể từ code trong project.
6. Trả lời ngắn gọn, rõ ràng, có cấu trúc (dùng danh sách, tiêu đề khi cần).

DOMAIN KNOWLEDGE:
- Project là hệ thống quản lý khách sạn gồm: quản lý phòng, đặt phòng, khách hàng, thanh toán.
- Backend: ASP.NET Core C# (MVC + Areas).
- Frontend: Next.js/React với Tailwind CSS.
- Database: SQL Server.
- Kiến trúc: Microservices/monorepo với nhiều thư mục service.
"""


class AIClient:
    def __init__(self, base_url: str, api_key: str):
        self.base_url = base_url.rstrip("/")
        self.api_key = api_key
        self.headers = {
            "Authorization": f"Bearer {api_key}",
            "Content-Type": "application/json",
        }

    async def chat(self, question: str, project_context: str, model: str = "deepseek-chat") -> str:
        """
        Gửi câu hỏi kèm project context đến ds2api và nhận câu trả lời tiếng Việt.
        """
        # Giới hạn context để tránh vượt token limit (~60K chars ~ 15K tokens)
        max_context_chars = 60_000
        if len(project_context) > max_context_chars:
            logger.warning(
                f"Context quá dài ({len(project_context)} chars), cắt xuống {max_context_chars} chars"
            )
            project_context = project_context[:max_context_chars] + "\n\n[... nội dung còn lại đã bị cắt bớt do giới hạn token ...]"

        user_message = f"""Dưới đây là toàn bộ nội dung source code và tài liệu của project:

<project_context>
{project_context}
</project_context>

---
Câu hỏi của tôi: {question}

Hãy trả lời bằng tiếng Việt dựa trên nội dung project ở trên."""

        payload = {
            "model": model,
            "messages": [
                {"role": "system", "content": SYSTEM_PROMPT},
                {"role": "user", "content": user_message},
            ],
            "stream": False,
            "temperature": 0.3,  # Thấp hơn để câu trả lời nhất quán hơn
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
        """Kiểm tra ds2api có đang chạy không."""
        try:
            async with httpx.AsyncClient(timeout=5) as client:
                r = await client.get(f"{self.base_url}/healthz")
                return r.status_code == 200
        except Exception:
            return False
