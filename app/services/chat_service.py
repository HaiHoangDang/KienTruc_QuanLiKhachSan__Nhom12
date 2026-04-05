from app.services.hotel_info_service import detect_hotel_info_intent, answer_hotel_info
from app.services.qwen_service import (
    ask_qwen,
    rewrite_friendly_response,
    classify_hotel_info_intent,
)

DIRECT_RESPONSE_INTENTS = {
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
    "hotel_wifi",
    "hotel_parking",
    "hotel_pool",
    "hotel_restaurant",
    "hotel_cancel_policy",
}

REWRITE_RESPONSE_INTENTS = {
    "hotel_overview",
    "hotel_amenities",
}

async def handle_chat(message: str) -> str:
    if not message or not message.strip():
        return "Dạ, nội dung câu hỏi của anh/chị đang để trống."

    intent, score = detect_hotel_info_intent(message)

    if intent == "unknown" or score < 4:
        qwen_intent = await classify_hotel_info_intent(message)
        if qwen_intent != "unknown":
            intent = qwen_intent

    if intent != "unknown":
        raw_answer = answer_hotel_info(intent)

        if intent in DIRECT_RESPONSE_INTENTS:
            return raw_answer

        if intent in REWRITE_RESPONSE_INTENTS:
            return await rewrite_friendly_response(raw_answer)

        return raw_answer

    return await ask_qwen(message)