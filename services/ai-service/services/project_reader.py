"""
ProjectReader: Đọc toàn bộ source code và tài liệu của project.
Tối ưu cho project DKS_HotelManager (ASP.NET + Next.js).
"""

import os
from pathlib import Path
from typing import Optional
import logging

logger = logging.getLogger(__name__)

# Thư mục/file không cần đọc
IGNORE_DIRS = {
    ".git", "node_modules", "__pycache__", ".venv", "venv",
    "bin", "obj",          # ASP.NET build output
    ".next", "dist", "build", "out",  # Next.js build output
    ".vs",                 # Visual Studio cache
    "static",              # ds2api static files
    "migrations",          # DB migrations (thường dài)
    ".mypy_cache", ".pytest_cache",
    "wwwroot",             # ASP.NET static assets
}

IGNORE_EXTENSIONS = {
    # Binary / compiled
    ".dll", ".exe", ".pdb", ".suo", ".user", ".ncb", ".cache",
    # Ảnh / font / media
    ".jpg", ".jpeg", ".png", ".gif", ".ico", ".svg", ".webp",
    ".woff", ".woff2", ".ttf", ".eot",
    # Lock / generated
    ".lock", ".sum", ".map",
    # Compiled bytecode
    ".pyc", ".class",
    # Database binary
    ".mdf", ".ldf",
    # WASM
    ".wasm",
}

# Extensions ưu tiên đọc (quan trọng nhất)
PRIORITY_EXTENSIONS = {".cs", ".py", ".ts", ".tsx", ".js", ".jsx", ".sql"}

# Kích thước tối đa mỗi file
MAX_FILE_SIZE_KB = 80


class ProjectReader:
    def __init__(self, root_dir: str):
        self.root = Path(root_dir).resolve()
        self._file_count = 0
        self._total_size = 0

    def get_stats(self) -> dict:
        return {
            "root": str(self.root),
            "files_read": self._file_count,
            "total_chars": self._total_size,
        }

    def _should_ignore(self, path: Path) -> bool:
        """Kiểm tra có nên bỏ qua file/thư mục này không."""
        for part in path.relative_to(self.root).parts:
            if part in IGNORE_DIRS:
                return True
        if path.suffix.lower() in IGNORE_EXTENSIONS:
            return True
        return False

    def read_all(self, max_chars: Optional[int] = None) -> str:
        """
        Đọc toàn bộ project, ưu tiên file code quan trọng trước.
        """
        self._file_count = 0
        self._total_size = 0
        chunks = [f"# PROJECT: DKS Hotel Manager - Nhóm 12\n# Root: {self.root}\n\n"]

        # Thu thập tất cả file, sắp xếp theo độ ưu tiên
        priority_files = []
        normal_files = []

        for path in sorted(self.root.rglob("*")):
            if path.is_dir() or self._should_ignore(path):
                continue
            if path.stat().st_size > MAX_FILE_SIZE_KB * 1024:
                continue
            if path.suffix.lower() in PRIORITY_EXTENSIONS:
                priority_files.append(path)
            else:
                normal_files.append(path)

        # Đọc file ưu tiên trước, rồi đến file thường
        for path in priority_files + normal_files:
            try:
                content = path.read_text(encoding="utf-8", errors="ignore").strip()
                if not content:
                    continue

                rel = path.relative_to(self.root)
                ext = path.suffix.lstrip(".")
                chunk = f"\n## FILE: {rel}\n```{ext}\n{content}\n```\n"

                if max_chars and (self._total_size + len(chunk)) > max_chars:
                    chunks.append("\n\n[... Đã đạt giới hạn ký tự, các file còn lại bị bỏ qua ...]\n")
                    break

                chunks.append(chunk)
                self._file_count += 1
                self._total_size += len(chunk)

            except Exception as e:
                logger.debug(f"Bỏ qua {path}: {e}")

        result = "".join(chunks)
        logger.info(f"Đã đọc {self._file_count} files, {self._total_size} ký tự")
        return result

    def read_summary(self) -> str:
        """
        Chỉ đọc các file quan trọng nhất (README, config, schema).
        Dùng khi project quá lớn.
        """
        important_names = {
            "README.md", "README.MD", ".env", ".env.example",
            "package.json", "appsettings.json", "appsettings.Development.json",
            "Program.cs", "Startup.cs",
        }
        important_patterns = ["*.sql", "*.md", "appsettings*.json"]

        chunks = ["# PROJECT SUMMARY: DKS Hotel Manager\n\n"]
        seen = set()

        # Đọc file theo tên quan trọng
        for path in self.root.rglob("*"):
            if path.is_file() and path.name in important_names and not self._should_ignore(path):
                if path in seen:
                    continue
                seen.add(path)
                try:
                    content = path.read_text(encoding="utf-8", errors="ignore").strip()
                    rel = path.relative_to(self.root)
                    ext = path.suffix.lstrip(".")
                    chunks.append(f"\n## {rel}\n```{ext}\n{content}\n```\n")
                except Exception:
                    pass

        # Đọc file theo pattern
        for pattern in important_patterns:
            for path in self.root.rglob(pattern):
                if path.is_dir() or self._should_ignore(path) or path in seen:
                    continue
                if path.stat().st_size > MAX_FILE_SIZE_KB * 1024:
                    continue
                seen.add(path)
                try:
                    content = path.read_text(encoding="utf-8", errors="ignore").strip()
                    rel = path.relative_to(self.root)
                    ext = path.suffix.lstrip(".")
                    chunks.append(f"\n## {rel}\n```{ext}\n{content}\n```\n")
                except Exception:
                    pass

        return "".join(chunks)

    def read_domain(self, domain: str) -> str:
        """
        Đọc code liên quan đến một domain cụ thể.
        domain: "room", "booking", "customer", "payment", "auth"
        """
        domain_keywords = {
            "room":     ["room", "phong", "Room", "Phong"],
            "booking":  ["booking", "reservation", "datphong", "Booking", "Reservation"],
            "customer": ["customer", "guest", "khach", "Customer", "Guest"],
            "payment":  ["payment", "invoice", "bill", "thanhtoan", "Payment", "Invoice"],
            "auth":     ["auth", "login", "user", "account", "Auth", "Login", "User"],
        }

        keywords = domain_keywords.get(domain.lower(), [domain])
        chunks = [f"# DOMAIN CONTEXT: {domain.upper()}\n\n"]

        for path in sorted(self.root.rglob("*")):
            if path.is_dir() or self._should_ignore(path):
                continue
            if path.suffix.lower() not in PRIORITY_EXTENSIONS:
                continue
            if path.stat().st_size > MAX_FILE_SIZE_KB * 1024:
                continue

            # Kiểm tra tên file có chứa keyword domain không
            filename = path.name.lower()
            if not any(kw.lower() in filename for kw in keywords):
                continue

            try:
                content = path.read_text(encoding="utf-8", errors="ignore").strip()
                rel = path.relative_to(self.root)
                ext = path.suffix.lstrip(".")
                chunks.append(f"\n## {rel}\n```{ext}\n{content}\n```\n")
            except Exception:
                pass

        return "".join(chunks)
