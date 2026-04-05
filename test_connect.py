from sqlalchemy import text
from app.db.database import engine

try:
    with engine.connect() as conn:
        result = conn.execute(text("SELECT 1 AS ok"))
        print("Kết nối thành công:", result.fetchone())
except Exception as e:
    print("Lỗi kết nối:", e)