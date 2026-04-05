# app/services/hotel_info_service.py

import re
import unicodedata


HOTEL_INFO = {
    "hotel_name": "Khách sạn DSD",
    "address": "123 Nguyễn Huệ, Quận 1, TP. Hồ Chí Minh",
    "hotline": "0909 123 456",
    "email": "support@dsdhotel.vn",
    "website": "https://www.dsdhotel.vn",
    "checkin_time": "14:00",
    "checkout_time": "12:00",
    "description": (
        "Khách sạn DSD là khách sạn lưu trú với không gian tiện nghi, "
        "phù hợp cho khách du lịch, khách công tác và gia đình."
    ),
    "amenities": [
        "wifi",
        "bãi đỗ xe",
        "hồ bơi",
        "nhà hàng",
        "lễ tân hỗ trợ",
        "dịch vụ dọn phòng"
    ],
    "wifi_info": "Khách sạn có cung cấp wifi cho khách lưu trú.",
    "parking_info": "Khách sạn có hỗ trợ khu vực đỗ xe cho khách lưu trú.",
    "pool_info": "Khách sạn có khu vực hồ bơi phục vụ khách lưu trú.",
    "restaurant_info": "Khách sạn có khu vực nhà hàng và hỗ trợ dịch vụ ăn uống.",
    "cancel_policy": (
        "Khách sạn áp dụng chính sách hủy và thay đổi đặt phòng tùy theo "
        "loại phòng và thời điểm đặt. Vui lòng liên hệ để được hỗ trợ chi tiết."
    ),
}


INTENT_KEYWORDS = {
    "hotel_name": {
        "primary": ["ten", "khach san"],
        "secondary": ["la gi", "ten gi", "nao", "ban la ai", "bot la ai", "em la ai"],
    },
    "hotel_address": {
        "primary": ["dia chi", "o dau", "nam o dau", "vi tri", "dia diem"],
        "secondary": ["khach san", "hotel", "duong di", "ban do", "map"],
    },
    "hotel_hotline": {
        "primary": ["hotline", "so dien thoai", "sdt", "so dt", "lien he"],
        "secondary": ["khach san", "le tan", "so nao", "dien thoai"],
    },
    "hotel_email": {
        "primary": ["email", "mail", "gmail"],
        "secondary": ["khach san", "lien he", "dia chi"],
    },
    "hotel_contact_info": {
        "primary": ["thong tin lien he", "thong tin lien lac", "website", "trang web", "web"],
        "secondary": ["khach san", "email", "hotline"],
    },
    "checkin_time": {
        "primary": ["check in", "check-in", "nhan phong", "gio nhan phong", "vao phong"],
        "secondary": ["luc may gio", "tu may gio", "bao gio"],
    },
    "checkout_time": {
        "primary": ["check out", "check-out", "tra phong", "gio tra phong"],
        "secondary": ["luc may gio", "truoc may gio", "bao gio"],
    },
    "hotel_overview": {
        "primary": ["thong tin khach san", "gioi thieu", "tong quan", "mo ta", "noi bat"],
        "secondary": ["khach san", "nhu the nao", "co gi"],
    },
    "hotel_amenities": {
        "primary": ["tien ich", "tien nghi", "dich vu", "co nhung gi", "cung cap nhung gi"],
        "secondary": ["khach san", "hotel"],
    },
    "hotel_wifi": {
        "primary": ["wifi", "internet", "mang"],
        "secondary": ["mien phi", "khach san", "co khong"],
    },
    "hotel_parking": {
        "primary": ["bai do xe", "cho dau xe", "gui xe", "parking", "de xe"],
        "secondary": ["oto", "xe may", "co khong"],
    },
    "hotel_pool": {
        "primary": ["ho boi", "be boi", "swimming pool"],
        "secondary": ["co khong", "su dung", "mo cua"],
    },
    "hotel_restaurant": {
        "primary": ["nha hang", "an uong", "buffet", "bua sang"],
        "secondary": ["co khong", "phuc vu", "dich vu"],
    },
    "hotel_cancel_policy": {
        "primary": ["huy phong", "huy dat phong", "doi lich", "hoan tien", "booking"],
        "secondary": ["mat phi", "co duoc", "chinh sach"],
    },
}


def normalize_text(text: str) -> str:
    if not text:
        return ""

    text = text.strip().lower()
    text = unicodedata.normalize("NFD", text)
    text = "".join(ch for ch in text if unicodedata.category(ch) != "Mn")
    text = re.sub(r"[^a-z0-9\s\-]", " ", text)
    text = re.sub(r"\s+", " ", text).strip()
    return text


def _keyword_score(msg: str, keywords: list[str], weight: int) -> int:
    score = 0
    for kw in keywords:
        if kw in msg:
            score += weight
    return score


def _regex_boost(msg: str, intent: str) -> int:
    if intent == "hotel_email" and re.search(r"\b(email|mail|gmail)\b", msg):
        return 5
    if intent == "hotel_hotline" and re.search(r"\b(hotline|sdt|so dien thoai|so dt)\b", msg):
        return 5
    if intent == "checkin_time" and re.search(r"\b(check in|check-in|nhan phong)\b", msg):
        return 5
    if intent == "checkout_time" and re.search(r"\b(check out|check-out|tra phong)\b", msg):
        return 5
    if intent == "hotel_address" and re.search(r"\b(dia chi|o dau|vi tri|dia diem)\b", msg):
        return 5
    return 0


def detect_hotel_info_intent(message: str) -> tuple[str, int]:
    msg = normalize_text(message)

    scores = {}

    for intent, groups in INTENT_KEYWORDS.items():
        score = 0
        score += _keyword_score(msg, groups.get("primary", []), 3)
        score += _keyword_score(msg, groups.get("secondary", []), 1)
        score += _regex_boost(msg, intent)
        scores[intent] = score

    best_intent = max(scores, key=scores.get)
    best_score = scores[best_intent]

    if best_score < 3:
        return "unknown", 0

    return best_intent, best_score


def answer_hotel_info(intent: str) -> str:
    hotel_name = HOTEL_INFO["hotel_name"]
    address = HOTEL_INFO["address"]
    hotline = HOTEL_INFO["hotline"]
    email = HOTEL_INFO["email"]
    website = HOTEL_INFO["website"]
    checkin_time = HOTEL_INFO["checkin_time"]
    checkout_time = HOTEL_INFO["checkout_time"]
    description = HOTEL_INFO["description"]
    amenities = ", ".join(HOTEL_INFO["amenities"])

    if intent == "hotel_name":
        return (
            f"Dạ, đây là {hotel_name} ạ. "
            f"Em là trợ lý ảo và sẵn sàng hỗ trợ anh/chị về thông tin khách sạn, phòng và dịch vụ."
        )

    elif intent == "hotel_address":
        return (
            f"Dạ, {hotel_name} nằm tại {address} ạ. "
            f"Nếu anh/chị cần, em có thể hỗ trợ thêm thông tin đường đi."
        )

    elif intent == "hotel_hotline":
        return f"Dạ, anh/chị có thể liên hệ {hotel_name} qua số {hotline} ạ."

    elif intent == "hotel_email":
        return f"Dạ, email liên hệ của {hotel_name} là {email} ạ."

    elif intent == "hotel_contact_info":
        return (
            f"Dạ, thông tin liên hệ của {hotel_name} như sau: "
            f"hotline {hotline}, email {email}, website {website}."
        )

    elif intent == "checkin_time":
        return f"Dạ, thời gian nhận phòng tại {hotel_name} là từ {checkin_time} ạ."

    elif intent == "checkout_time":
        return f"Dạ, thời gian trả phòng là trước {checkout_time} ạ."

    elif intent == "hotel_overview":
        return (
            f"Dạ, {hotel_name} nằm tại {address}. "
            f"{description} "
            f"Khách sạn hiện có các tiện ích như {amenities}. "
            f"Thời gian nhận phòng từ {checkin_time} và trả phòng trước {checkout_time}."
        )

    elif intent == "hotel_amenities":
        return (
            f"Dạ, {hotel_name} hiện có các tiện ích như {amenities}. "
            f"Nếu anh/chị muốn, em có thể hỗ trợ thêm chi tiết về từng tiện ích."
        )

    elif intent == "hotel_wifi":
        return f"Dạ, {HOTEL_INFO['wifi_info']}"

    elif intent == "hotel_parking":
        return f"Dạ, {HOTEL_INFO['parking_info']}"

    elif intent == "hotel_pool":
        return f"Dạ, {HOTEL_INFO['pool_info']}"

    elif intent == "hotel_restaurant":
        return f"Dạ, {HOTEL_INFO['restaurant_info']}"

    elif intent == "hotel_cancel_policy":
        return f"Dạ, {HOTEL_INFO['cancel_policy']}"

    return (
        "Dạ, hiện tại em chưa có đủ thông tin để trả lời câu hỏi này. "
        "Anh/chị có thể đặt câu hỏi khác để em hỗ trợ tốt hơn ạ."
    )