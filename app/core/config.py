import os
from dotenv import load_dotenv

load_dotenv()

class Settings:
    APP_NAME: str = os.getenv("APP_NAME", "Hotel Chatbot API")
    DEBUG: bool = os.getenv("DEBUG", "False").lower() == "true"

    QWEN_API_URL: str = os.getenv("QWEN_API_URL", "")
    QWEN_API_KEY: str = os.getenv("QWEN_API_KEY", "EMPTY")
    QWEN_MODEL: str = os.getenv("QWEN_MODEL", "Qwen/Qwen2.5-1.5B-Instruct")
    QWEN_TIMEOUT: int = int(os.getenv("QWEN_TIMEOUT", "60"))

settings = Settings()