"""
ChatHistoryService: Lưu lịch sử chat theo từng customer_id vào SQLite.
- Mỗi customer có conversation riêng, persistent qua các session.
- Trả về lịch sử dạng messages[] để gửi kèm vào DeepSeek (trí nhớ).
- Trả về lịch sử dạng text để inject vào system prompt (AI nhớ ngữ cảnh).
"""

import sqlite3
import logging
from pathlib import Path

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
        conn.execute(
            "CREATE INDEX IF NOT EXISTS idx_customer ON chat_history(customer_id, id)"
        )
        conn.commit()
    logger.info(f"SQLite DB sẵn sàng tại: {DB_PATH}")


def save_message(customer_id: str, role: str, content: str):
    """Lưu 1 tin nhắn vào DB."""
    with _get_conn() as conn:
        conn.execute(
            "INSERT INTO chat_history (customer_id, role, content) VALUES (?, ?, ?)",
            (customer_id, role, content),
        )
        conn.commit()


def get_history(customer_id: str, limit: int = 20) -> list[dict]:
    """
    Lấy N tin nhắn gần nhất của customer dạng messages[].
    Dùng để gửi thẳng vào messages[] của DeepSeek API.
    """
    with _get_conn() as conn:
        rows = conn.execute(
            """
            SELECT role, content FROM (
                SELECT role, content, id
                FROM chat_history
                WHERE customer_id = ?
                ORDER BY id DESC
                LIMIT ?
            ) ORDER BY id ASC
            """,
            (customer_id, limit),
        ).fetchall()
    return [{"role": r["role"], "content": r["content"]} for r in rows]


def get_history_as_context_text(customer_id: str, limit: int = 10) -> str:
    """
    Lấy lịch sử dạng plain text để inject vào system prompt.
    Giúp AI hiểu ngữ cảnh ngay từ đầu, không bị "mất trí nhớ" dù ds2api
    tạo conversation mới mỗi request.

    Ví dụ output:
        [2026-05-15 09:00] Khách: Tôi muốn đặt phòng tại Vũng Tàu
        [2026-05-15 09:00] Nhân viên AI: Dạ, hiện còn phòng P402...
        [2026-05-15 09:01] Khách: Giá bao nhiêu?
        [2026-05-15 09:01] Nhân viên AI: P402 giá 1.000.000₫/đêm...
    """
    with _get_conn() as conn:
        rows = conn.execute(
            """
            SELECT role, content, created_at FROM (
                SELECT role, content, created_at, id
                FROM chat_history
                WHERE customer_id = ?
                ORDER BY id DESC
                LIMIT ?
            ) ORDER BY id ASC
            """,
            (customer_id, limit * 2),  # limit * 2 vì mỗi lượt = 2 rows (user + assistant)
        ).fetchall()

    if not rows:
        return ""

    lines = []
    for r in rows:
        label = "Khách" if r["role"] == "user" else "Nhân viên AI"
        # Rút gọn nội dung dài để tránh vượt token limit
        content = r["content"]
        if len(content) > 300:
            content = content[:300] + "..."
        lines.append(f"[{r['created_at']}] {label}: {content}")

    return "\n".join(lines)


def get_history_display(customer_id: str, limit: int = 50) -> list[dict]:
    """
    Lấy lịch sử để hiển thị trên UI (kèm timestamp).
    """
    with _get_conn() as conn:
        rows = conn.execute(
            """
            SELECT role, content, created_at
            FROM chat_history
            WHERE customer_id = ?
            ORDER BY id DESC
            LIMIT ?
            """,
            (customer_id, limit),
        ).fetchall()
    return [
        {"role": r["role"], "content": r["content"], "created_at": r["created_at"]}
        for r in reversed(rows)
    ]


def clear_history(customer_id: str):
    """Xóa toàn bộ lịch sử của 1 customer."""
    with _get_conn() as conn:
        conn.execute("DELETE FROM chat_history WHERE customer_id = ?", (customer_id,))
        conn.commit()
    logger.info(f"Đã xóa lịch sử chat của customer: {customer_id}")
