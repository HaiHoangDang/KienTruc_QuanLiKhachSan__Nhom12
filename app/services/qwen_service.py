import httpx
from app.core.config import settings


async def ask_qwen(user_message: str) -> str:
    if not settings.QWEN_API_URL:
        return "Chưa cấu hình Qwen API URL."

    headers = {"Content-Type": "application/json"}

    if settings.QWEN_API_KEY and settings.QWEN_API_KEY != "EMPTY":
        headers["Authorization"] = f"Bearer {settings.QWEN_API_KEY}"

    payload = {
        "model": settings.QWEN_MODEL,
        "messages": [
            {
                "role": "system",
                "content": (
                    "Bạn là trợ lý ảo chính thức của khách sạn DSD. "
                    "Trả lời bằng tiếng Việt, lịch sự, tự nhiên, thân thiện. "
                    "Không tự nhận là Anthropic, OpenAI hay hệ thống khác. "
                    "Không bịa thông tin. Nếu không đủ dữ liệu, hãy nói chưa đủ thông tin."
                ),
            },
            {
                "role": "user",
                "content": user_message,
            },
        ],
        "temperature": 0.5,
    }

    try:
        async with httpx.AsyncClient(timeout=settings.QWEN_TIMEOUT) as client:
            response = await client.post(
                settings.QWEN_API_URL,
                json=payload,
                headers=headers,
            )
            response.raise_for_status()
            data = response.json()
            return data["choices"][0]["message"]["content"].strip()

    except httpx.TimeoutException:
        return "Hệ thống AI phản hồi quá chậm, vui lòng thử lại sau."

    except httpx.HTTPStatusError as e:
        return f"Lỗi từ Qwen API: {e.response.status_code}"

    except Exception:
        return "Không thể kết nối tới hệ thống AI."


async def rewrite_friendly_response(raw_answer: str) -> str:
    if not settings.QWEN_API_URL:
        return raw_answer

    headers = {"Content-Type": "application/json"}

    if settings.QWEN_API_KEY and settings.QWEN_API_KEY != "EMPTY":
        headers["Authorization"] = f"Bearer {settings.QWEN_API_KEY}"

    payload = {
        "model": settings.QWEN_MODEL,
        "messages": [
            {
                "role": "system",
                "content": (
                    "Bạn là trợ lý ảo của khách sạn DSD. "
                    "Hãy diễn đạt lại câu trả lời theo phong cách lịch sự, tự nhiên, ngắn gọn. "
                    "Không thêm thông tin mới."
                ),
            },
            {
                "role": "user",
                "content": f"Hãy diễn đạt lại câu sau cho tự nhiên hơn nhưng giữ nguyên ý: {raw_answer}",
            },
        ],
        "temperature": 0.3,
    }

    try:
        async with httpx.AsyncClient(timeout=settings.QWEN_TIMEOUT) as client:
            response = await client.post(
                settings.QWEN_API_URL,
                json=payload,
                headers=headers,
            )
            response.raise_for_status()
            data = response.json()
            return data["choices"][0]["message"]["content"].strip()
    except Exception:
        return raw_answer


async def classify_hotel_info_intent(message: str) -> str:
    if not settings.QWEN_API_URL:
        return "unknown"

    headers = {"Content-Type": "application/json"}

    if settings.QWEN_API_KEY and settings.QWEN_API_KEY != "EMPTY":
        headers["Authorization"] = f"Bearer {settings.QWEN_API_KEY}"

    intent_list = [
        "bot_identity",
        "bot_name",
        "bot_role",
        "bot_capability",
        "hotel_name",
        "hotel_address",
        "hotel_hotline",
        "hotel_email",
        "hotel_contact_info",
        "checkin_time",
        "checkout_time",
        "hotel_overview",
        "hotel_amenities",
        "hotel_wifi",
        "hotel_parking",
        "hotel_pool",
        "hotel_restaurant",
        "hotel_cancel_policy",
        "unknown",
    ]

    payload = {
        "model": settings.QWEN_MODEL,
        "messages": [
            {
                "role": "system",
                "content": (
                    "Bạn là bộ phân loại intent cho chatbot khách sạn. "
                    "Chỉ trả về đúng 1 intent trong danh sách sau, không giải thích: "
                    + ", ".join(intent_list)
                ),
            },
            {
                "role": "user",
                "content": message,
            },
        ],
        "temperature": 0.0,
    }

    try:
        async with httpx.AsyncClient(timeout=settings.QWEN_TIMEOUT) as client:
            response = await client.post(
                settings.QWEN_API_URL,
                json=payload,
                headers=headers,
            )
            response.raise_for_status()
            data = response.json()
            content = data["choices"][0]["message"]["content"].strip().lower()

            first_token = content.split()[0]
            if first_token in intent_list:
                return first_token

            return "unknown"
    except Exception:
        return "unknown"