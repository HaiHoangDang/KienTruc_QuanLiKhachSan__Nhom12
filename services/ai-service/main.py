"""
AI Service v3 — DKS Hotel
Fix: context nhẹ hơn, load lịch sử khi vào trang, bỏ project_context khỏi payload chat
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
from services.chat_history import init_db, save_message, get_history, get_history_display, clear_history

logging.basicConfig(level=logging.INFO, format="%(asctime)s [%(levelname)s] %(message)s")
logger = logging.getLogger(__name__)

DS2API_URL = os.getenv("DS2API_URL", "http://localhost:5001")
DS2API_KEY = os.getenv("DS2API_KEY", "hotel-ai-key")

ai_client = AIClient(DS2API_URL, DS2API_KEY)


@asynccontextmanager
async def lifespan(app: FastAPI):
    init_db()
    logger.info("✅ AI Service v3 sẵn sàng!")
    yield


app = FastAPI(
    title="DKS Hotel AI Service",
    version="3.0.0",
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
    question:    str
    customer_id: str            = "guest"
    db_context:  Optional[str] = None   # DB data từ C# (phòng, giá, booking...)


class ChatResponse(BaseModel):
    answer:      str
    customer_id: str


# ─── Endpoints ────────────────────────────────────────────────────────────────

@app.get("/health")
def health():
    return {"status": "ok", "service": "dks-hotel-ai", "version": "3.0.0"}


@app.post("/chat", response_model=ChatResponse)
@app.post("/chat/", response_model=ChatResponse, include_in_schema=False)
async def chat(req: ChatRequest):
    if not req.question.strip():
        raise HTTPException(status_code=400, detail="Câu hỏi không được để trống")

    try:
        # Chỉ dùng db_context (nhẹ, realtime) — KHÔNG gửi project code lên ds2api
        hotel_context = req.db_context or "Không có dữ liệu database."

        # Lịch sử hội thoại của customer
        history = get_history(req.customer_id, limit=10)

        answer = await ai_client.chat(
            question=req.question,
            hotel_context=hotel_context,
            history=history,
        )

        # Lưu vào SQLite
        save_message(req.customer_id, "user",      req.question)
        save_message(req.customer_id, "assistant", answer)

        return ChatResponse(answer=answer, customer_id=req.customer_id)

    except Exception as e:
        logger.error(f"Lỗi chat: {e}")
        raise HTTPException(status_code=500, detail=f"Lỗi xử lý: {str(e)}")


@app.get("/history/{customer_id}")
def get_chat_history(customer_id: str, limit: int = 50):
    """Load lịch sử chat để hiển thị trên UI khi vào trang / đăng nhập lại."""
    if customer_id == "guest":
        return {"customer_id": "guest", "messages": []}
    messages = get_history_display(customer_id, limit=limit)
    return {"customer_id": customer_id, "messages": messages}


@app.delete("/history/{customer_id}")
def delete_chat_history(customer_id: str):
    clear_history(customer_id)
    return {"message": f"Đã xóa lịch sử của {customer_id}"}


if __name__ == "__main__":
    uvicorn.run("main:app", host="0.0.0.0", port=8000, reload=True)
