"""
AI Service v2 — DKS Hotel Manager
Tính năng mới:
  - Lịch sử chat per-customer lưu SQLite
  - Trí nhớ: gửi kèm history vào mỗi lượt chat
  - Endpoint xem/xóa lịch sử
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

from services.project_reader import ProjectReader
from services.ai_client import AIClient
from services.context_builder import ContextBuilder
from services.chat_history import init_db, save_message, get_history, get_history_display, clear_history

logging.basicConfig(level=logging.INFO, format="%(asctime)s [%(levelname)s] %(message)s")
logger = logging.getLogger(__name__)

PROJECT_ROOT = os.getenv("PROJECT_ROOT", "../../")
DS2API_URL   = os.getenv("DS2API_URL",   "http://localhost:5001")
DS2API_KEY   = os.getenv("DS2API_KEY",   "hotel-ai-key")

project_reader  = ProjectReader(PROJECT_ROOT)
ai_client       = AIClient(DS2API_URL, DS2API_KEY)
context_builder = ContextBuilder(project_reader)

_project_context_cache: Optional[str] = None


def get_project_context(force_refresh: bool = False) -> str:
    global _project_context_cache
    if _project_context_cache is None or force_refresh:
        logger.info("Đang đọc project context...")
        _project_context_cache = context_builder.build_summary_context()
        logger.info(f"Project context: {len(_project_context_cache)} ký tự")
    return _project_context_cache


@asynccontextmanager
async def lifespan(app: FastAPI):
    init_db()                    # Khởi tạo SQLite
    get_project_context()        # Pre-warm cache
    logger.info("✅ AI Service sẵn sàng!")
    yield


app = FastAPI(
    title="DKS Hotel AI Service",
    description="AI tư vấn khách sạn — có trí nhớ theo từng tài khoản",
    version="2.0.0",
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
    question:     str
    customer_id:  str = "guest"        # ID tài khoản, "guest" nếu chưa đăng nhập
    context_type: str = "summary"
    domain:       Optional[str] = None
    db_context:   Optional[str] = None  # Dữ liệu realtime từ SQL Server (C# gửi lên)


class ChatResponse(BaseModel):
    answer:         str
    customer_id:    str
    context_length: int
    model:          str


# ─── Routes ───────────────────────────────────────────────────────────────────

@app.get("/health")
def health():
    return {"status": "ok", "service": "dks-hotel-ai", "version": "2.0.0"}


@app.post("/chat", response_model=ChatResponse)
@app.post("/chat/", response_model=ChatResponse, include_in_schema=False)
async def chat(req: ChatRequest):
    if not req.question.strip():
        raise HTTPException(status_code=400, detail="Câu hỏi không được để trống")

    try:
        # 1. Lấy project context (code/docs)
        project_ctx = get_project_context()

        # 2. Ghép db_context (realtime SQL Server) lên đầu
        if req.db_context:
            hotel_context = req.db_context + "\n\n---\n\n" + project_ctx
        else:
            hotel_context = project_ctx

        # 3. Lấy lịch sử hội thoại của customer này (tối đa 20 lượt)
        history = get_history(req.customer_id, limit=20)

        # 4. Gọi AI với đầy đủ context + history
        answer = await ai_client.chat(
            question=req.question,
            hotel_context=hotel_context,
            history=history,
        )

        # 5. Lưu câu hỏi + trả lời vào SQLite
        save_message(req.customer_id, "user",      req.question)
        save_message(req.customer_id, "assistant", answer)

        return ChatResponse(
            answer=answer,
            customer_id=req.customer_id,
            context_length=len(hotel_context),
            model="deepseek-v4-flash",
        )

    except Exception as e:
        logger.error(f"Lỗi chat: {e}")
        raise HTTPException(status_code=500, detail=f"Lỗi xử lý: {str(e)}")


@app.get("/history/{customer_id}")
def get_chat_history(customer_id: str, limit: int = 50):
    """Lấy lịch sử chat của 1 customer để hiển thị trên UI."""
    if customer_id == "guest":
        return {"customer_id": "guest", "messages": [], "note": "Khách chưa đăng nhập"}
    messages = get_history_display(customer_id, limit=limit)
    return {"customer_id": customer_id, "messages": messages, "total": len(messages)}


@app.delete("/history/{customer_id}")
def delete_chat_history(customer_id: str):
    """Xóa toàn bộ lịch sử chat của 1 customer."""
    clear_history(customer_id)
    return {"message": f"Đã xóa lịch sử chat của {customer_id}"}


@app.post("/refresh-context")
async def refresh_context():
    ctx = get_project_context(force_refresh=True)
    return {"message": "Đã làm mới context", "context_length": len(ctx)}


if __name__ == "__main__":
    uvicorn.run("main:app", host="0.0.0.0", port=8000, reload=True)
