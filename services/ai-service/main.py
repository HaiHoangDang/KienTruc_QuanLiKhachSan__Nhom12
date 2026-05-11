"""
AI Service - Quản Lí Khách Sạn Nhóm 12
Chạy thủ công: python main.py
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

logging.basicConfig(level=logging.INFO, format="%(asctime)s [%(levelname)s] %(message)s")
logger = logging.getLogger(__name__)

PROJECT_ROOT = os.getenv("PROJECT_ROOT", "../../")   # trỏ về root của project
DS2API_URL   = os.getenv("DS2API_URL", "http://localhost:5001")
DS2API_KEY   = os.getenv("DS2API_KEY", "hotel-ai-key")

project_reader  = ProjectReader(PROJECT_ROOT)
ai_client       = AIClient(DS2API_URL, DS2API_KEY)
context_builder = ContextBuilder(project_reader)

_project_context_cache: Optional[str] = None


def get_project_context(force_refresh: bool = False) -> str:
    global _project_context_cache
    if _project_context_cache is None or force_refresh:
        logger.info("Đang đọc toàn bộ project context...")
        _project_context_cache = context_builder.build_full_context()
        logger.info(f"Đã load project context: {len(_project_context_cache)} ký tự")
    return _project_context_cache


# ─── Lifespan (pre-warm cache khi khởi động) ─────────────────────────────────

@asynccontextmanager
async def lifespan(app: FastAPI):
    logger.info("AI Service đang khởi động, đang đọc project...")
    try:
        get_project_context()
        logger.info("✅ AI Service sẵn sàng!")
    except Exception as e:
        logger.warning(f"Không thể load context ngay lúc startup: {e}")
    yield


app = FastAPI(
    title="Hotel Manager AI Service",
    description="AI Assistant cho hệ thống Quản Lí Khách Sạn - Nhóm 12",
    version="1.0.0",
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
    question: str
    context_type: str = "summary"          # "full" | "summary" | "domain"
    domain: Optional[str] = None           # "room","booking","customer","payment","auth"
    db_context: Optional[str] = None       # Dữ liệu realtime từ SQL Server (do C# gửi lên)


class ChatResponse(BaseModel):
    answer: str
    context_length: int
    model: str


# ─── Routes ───────────────────────────────────────────────────────────────────

@app.get("/health")
def health():
    return {"status": "ok", "service": "hotel-ai-service"}


# Hỗ trợ cả /chat và /chat/ để khớp với ChatBotController (gọi http://127.0.0.1:8000/chat/)
@app.post("/chat", response_model=ChatResponse)
@app.post("/chat/", response_model=ChatResponse, include_in_schema=False)
async def chat(req: ChatRequest):
    if not req.question.strip():
        raise HTTPException(status_code=400, detail="Câu hỏi không được để trống")

    try:
        # Lấy project context (code, docs)
        if req.context_type == "domain" and req.domain:
            project_ctx = context_builder.build_domain_context(req.domain)
        elif req.context_type == "full":
            project_ctx = get_project_context()
        else:
            project_ctx = context_builder.build_summary_context()

        # Nếu C# gửi kèm db_context (dữ liệu realtime từ SQL Server),
        # đặt lên đầu để AI ưu tiên đọc trước
        if req.db_context:
            context = req.db_context + "\n\n---\n\n" + project_ctx
        else:
            context = project_ctx

        answer = await ai_client.chat(req.question, context)

        return ChatResponse(
            answer=answer,
            context_length=len(context),
            model="deepseek-chat",
        )

    except Exception as e:
        logger.error(f"Lỗi khi xử lý câu hỏi: {e}")
        raise HTTPException(status_code=500, detail=f"Lỗi xử lý: {str(e)}")


@app.post("/refresh-context")
async def refresh_context():
    """Làm mới cache (gọi khi code thay đổi)."""
    context = get_project_context(force_refresh=True)
    return {"message": "Đã làm mới project context", "context_length": len(context)}


@app.get("/context-info")
def context_info():
    ctx = get_project_context()
    return {"context_length": len(ctx), "stats": project_reader.get_stats()}


if __name__ == "__main__":
    uvicorn.run("main:app", host="0.0.0.0", port=8000, reload=True)
