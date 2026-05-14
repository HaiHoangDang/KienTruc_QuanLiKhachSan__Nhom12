"""
ChatHistoryService: Lưu lịch sử chat theo từng customer_id vào SQLite.
- Mỗi customer có conversation riêng, persistent qua các session.
- Trả về lịch sử dạng messages[] để gửi kèm vào DeepSeek (trí nhớ).
"""

import sqlite3
import json
import logging
from datetime import datetime
from pathlib import Path
from typing import Optional

logger = logging.getLogger(__name__)

DB_PATH = Path(__file__).parent.parent / "data" / "chat_history.db"


def _get_conn() -> sqlite3.Connection:
    DB_PATH.parent.mkdir(parents=True, exist_ok=True)
    conn = sqlite3.connect(str(DB_PATH), check_same_thread=False)
    conn.row_factory = sqlite3.Row
    return conn


def init_db():
    """Tạo bảng nếu chưa có. Gọi khi startup."""
    with _get_conn() as conn:
        conn.execute("""
            CREATE TABLE IF NOT EXISTS chat_history (
                id          INTEGER PRIMARY KEY AUTOINCREMENT,
                customer_id TEXT    NOT NULL,
                role        TEXT    NOT NULL CHECK(role IN ('user','assistant')),
                content     TEXT    NOT NULL,
                created_at  TEXT    NOT NULL DEFAULT (datetime('now','localtime'))
            )
        """)
        conn.execute("CREATE INDEX IF NOT EXISTS idx_customer ON chat_history(customer_id, id)")
        conn.commit()
    logger.info(f"SQLite DB sẵn sàng tại: {DB_PATH}")


def save_message(customer_id: str, role: str, content: str):
    """Lưu 1 tin nhắn vào DB."""
    with _get_conn() as conn:
        conn.execute(
            "INSERT INTO chat_history (customer_id, role, content) VALUES (?, ?, ?)",
            (customer_id, role, content)
        )
        conn.commit()


def get_history(customer_id: str, limit: int = 20) -> list[dict]:
    """
    Lấy N tin nhắn gần nhất của customer.
    Trả về list[{"role": "user"|"assistant", "content": "..."}]
    để gửi thẳng vào messages[] của DeepSeek API.
    """
    with _get_conn() as conn:
        rows = conn.execute("""
            SELECT role, content FROM (
                SELECT role, content, id
                FROM chat_history
                WHERE customer_id = ?
                ORDER BY id DESC
                LIMIT ?
            ) ORDER BY id ASC
        """, (customer_id, limit)).fetchall()
    return [{"role": r["role"], "content": r["content"]} for r in rows]


def get_history_display(customer_id: str, limit: int = 50) -> list[dict]:
    """
    Lấy lịch sử để hiển thị trên UI (kèm timestamp).
    """
    with _get_conn() as conn:
        rows = conn.execute("""
            SELECT role, content, created_at
            FROM chat_history
            WHERE customer_id = ?
            ORDER BY id DESC
            LIMIT ?
        """, (customer_id, limit)).fetchall()
    result = [
        {"role": r["role"], "content": r["content"], "created_at": r["created_at"]}
        for r in reversed(rows)
    ]
    return result


def clear_history(customer_id: str):
    """Xóa toàn bộ lịch sử của 1 customer."""
    with _get_conn() as conn:
        conn.execute("DELETE FROM chat_history WHERE customer_id = ?", (customer_id,))
        conn.commit()
