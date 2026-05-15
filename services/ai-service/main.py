"""
AI Service v4 — DKS Hotel
- Mỗi session đăng nhập = 1 conversation xuyên suốt (không tạo mới mỗi lần chat)
- Load lịch sử DB inject vào system prompt để AI nhớ ngữ cảnh
- Tạo conversation khi bắt đầu, xóa conversation sau mỗi request (ds2api stateless),
  nhưng lịch sử vẫn được duy trì qua DB và gửi kèm mỗi lần
"""

import os
import logging
from contextlib import asynccontextmanager
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import Optional
from dotenv import load_dotenv
import uvicorn

load_dotenv()

from services.ai_client    import AIClient
from services.chat_history import (
    init_db, save_message, get_history, get_history_display,
    clear_history, get_history_as_context_text
)

logging.basicConfig(level=logging.INFO, format="%(asctime)s [%(levelname)s] %(message)s")
logger = logging.getLogger(__name__)

DS2API_URL = os.getenv("DS2API_URL", "http://localhost:5001")
DS2API_KEY = os.getenv("DS2API_KEY", "hotel-ai-key")

ai_client = AIClient(DS2API_URL, DS2API_KEY)


@asynccontextmanager
async def lifespan(app: FastAPI):
    init_db()
    logger.info("✅ AI Service v4 sẵn sàng!")
    yield


app = FastAPI(
    title="DKS Hotel AI Service",
    version="4.0.0",
    lifespan=lifespan,
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


# ─── Models ───────────────────────────────────────────────────────────────────

class ChatRequest(BaseModel):
    question:      str
    customer_id:   str            = "guest"
    customer_name: Optional[str] = None   # Tên khách hàng (từ session đăng nhập)
    is_logged_in:  bool           = False  # Trạng thái đăng nhập
    db_context:    Optional[str]  = None   # DB data từ C# (phòng, giá, booking...)


class ChatResponse(BaseModel):
    answer:      str
    customer_id: str


class HistoryResponse(BaseModel):
    customer_id: str
    messages:    list[dict]


# ─── Endpoints ────────────────────────────────────────────────────────────────

@app.get("/health")
def health():
    return {"status": "ok", "service": "dks-hotel-ai", "version": "4.0.0"}


@app.post("/chat", response_model=ChatResponse)
@app.post("/chat/", response_model=ChatResponse, include_in_schema=False)
async def chat(req: ChatRequest):
    """
    Luồng hoạt động:
    1. Load toàn bộ lịch sử chat của customer từ DB
    2. Build system context: lịch sử cũ + trạng thái đăng nhập + DB data realtime
    3. Gửi lên ds2api (gồm system prompt đầy đủ + messages history + câu hỏi mới)
    4. Lưu Q&A mới vào DB
    5. Trả kết quả về frontend
    """
    if not req.question.strip():
        raise HTTPException(status_code=400, detail="Câu hỏi không được để trống")

    try:
        # 1. Lấy lịch sử dạng messages[] để gửi lên API (tối đa 20 lượt gần nhất)
        history = get_history(req.customer_id, limit=20) if req.customer_id != "guest" else []

        # 2. Lấy lịch sử dạng text để inject vào system prompt (tối đa 10 lượt)
        history_text = ""
        if req.customer_id != "guest" and history:
            history_text = get_history_as_context_text(req.customer_id, limit=10)

        # 3. Chuẩn bị hotel_context = db_context + login state + lịch sử text
        login_state = f"IS_LOGGED_IN={'true' if req.is_logged_in else 'false'}"
        customer_info = ""
        if req.customer_name:
            customer_info = f"CUSTOMER_NAME={req.customer_name}\nCUSTOMER_ID={req.customer_id}"

        hotel_context_parts = [
            login_state,
            customer_info,
            req.db_context or "Không có dữ liệu database.",
        ]

        # Inject lịch sử chat cũ vào context (để AI "nhớ" ngay trong system prompt)
        if history_text:
            hotel_context_parts.append(
                f"\n=== LỊCH SỬ CUỘC TRÒ CHUYỆN TRƯỚC ĐÓ ===\n{history_text}\n=== KẾT THÚC LỊCH SỬ ==="
            )

        hotel_context = "\n".join(filter(None, hotel_context_parts))

        logger.info(
            f"[{req.customer_id}] chat | logged_in={req.is_logged_in} | "
            f"history={len(history)} msgs | context={len(hotel_context)} chars"
        )

        # 4. Gọi AI — gửi cả history[] lẫn hotel_context (có lịch sử text)
        #    ds2api sẽ tạo conversation mới, dùng xong tự xóa (stateless)
        #    Trí nhớ được duy trì hoàn toàn qua DB của chúng ta
        answer = await ai_client.chat(
            question=req.question,
            hotel_context=hotel_context,
            history=history,       # messages[] — các lượt gần nhất dạng role/content
        )

        # 5. Lưu vào SQLite
        save_message(req.customer_id, "user",      req.question)
        save_message(req.customer_id, "assistant", answer)

        return ChatResponse(answer=answer, customer_id=req.customer_id)

    except Exception as e:
        logger.error(f"Lỗi chat [{req.customer_id}]: {e}")
        raise HTTPException(status_code=500, detail=f"Lỗi xử lý: {str(e)}")


@app.get("/history/{customer_id}", response_model=HistoryResponse)
def get_chat_history(customer_id: str, limit: int = 50):
    """
    Load lịch sử chat để hiển thị trên UI khi vào trang / đăng nhập lại.
    Frontend gọi endpoint này ngay sau khi user đăng nhập thành công.
    """
    if customer_id == "guest":
        return HistoryResponse(customer_id="guest", messages=[])
    messages = get_history_display(customer_id, limit=limit)
    return HistoryResponse(customer_id=customer_id, messages=messages)


@app.delete("/history/{customer_id}")
def delete_chat_history(customer_id: str):
    """Xóa lịch sử chat (dùng cho nút 'Xóa lịch sử' trên UI)."""
    clear_history(customer_id)
    return {"message": f"Đã xóa lịch sử của {customer_id}"}


if __name__ == "__main__":
    uvicorn.run("main:app", host="0.0.0.0", port=8000, reload=True)
