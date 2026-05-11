"""
ContextBuilder: Tổng hợp context từ ProjectReader.
Thêm metadata về project để AI hiểu rõ hơn.
"""

from services.project_reader import ProjectReader


PROJECT_METADATA = """
# THÔNG TIN PROJECT

## Tên project
DKS Hotel Manager - Hệ thống Quản Lí Khách Sạn (Nhóm 12)
Môn học: Kiến Trúc Thiết Kế Phần Mềm

## Stack công nghệ
- Backend: ASP.NET Core C# - kiến trúc MVC + Areas
- Frontend: Next.js (React) + Tailwind CSS
- Database: SQL Server
- AI Service (service này): Python FastAPI + DeepSeek qua ds2api

## Cấu trúc thư mục chính
- `DKS_HotelManager/`  → ASP.NET Core backend (Controllers, Models, Views, Areas)
- `app/`               → Next.js frontend
- `Database/`          → SQL scripts, schema
- `Areas/`             → ASP.NET Areas (module hóa theo chức năng)
- `services/ai-service/` → Service AI này (Python FastAPI)
- `packages/`          → Shared packages
- `docs/`              → Tài liệu

## Các chức năng chính
1. Quản lý phòng (Room Management)
2. Đặt phòng / trả phòng (Booking / Check-in / Check-out)
3. Quản lý khách hàng (Customer Management)
4. Thanh toán / hóa đơn (Payment / Invoice)
5. Báo cáo thống kê (Reports)
6. Phân quyền người dùng (Authentication / Authorization)

"""


class ContextBuilder:
    def __init__(self, reader: ProjectReader):
        self.reader = reader

    def build_full_context(self) -> str:
        """Context đầy đủ: metadata + toàn bộ source code."""
        code_context = self.reader.read_all(max_chars=55_000)
        return PROJECT_METADATA + "\n---\n\n" + code_context

    def build_summary_context(self) -> str:
        """Context tóm tắt: metadata + các file quan trọng nhất."""
        summary = self.reader.read_summary()
        return PROJECT_METADATA + "\n---\n\n" + summary

    def build_domain_context(self, domain: str) -> str:
        """Context theo domain cụ thể: metadata + code liên quan domain."""
        domain_code = self.reader.read_domain(domain)
        return PROJECT_METADATA + f"\n---\n\n# DOMAIN: {domain.upper()}\n\n" + domain_code
