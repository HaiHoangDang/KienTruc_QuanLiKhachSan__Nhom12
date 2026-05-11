# AI Service - DKS Hotel Manager

Service AI tích hợp DeepSeek (qua ds2api), trả lời **tiếng Việt** dựa trên toàn bộ source code của project.

## Cấu trúc

```
services/ai-service/
├── main.py                    # FastAPI app (entry point)
├── services/
│   ├── ai_client.py           # Gọi ds2api, system prompt tiếng Việt
│   ├── project_reader.py      # Đọc toàn bộ codebase
│   └── context_builder.py     # Build context cho AI
├── examples/
│   ├── ai.ts                  # Utility cho Next.js
│   ├── AIChatBot.tsx          # Component chatbot
│   └── route.ts               # Next.js API route proxy
├── requirements.txt
└── Dockerfile
```

## Cách chạy

### Bước 1: Cấu hình biến môi trường

```bash
# Từ thư mục root của project
cp .env.ai.example .env.ai
# Điền DEEPSEEK_EMAIL và DEEPSEEK_PASSWORD vào .env.ai
```

### Bước 2: Khởi động service

```bash
# Chạy ds2api + ai-service bằng docker compose
docker compose -f docker-compose.ai.yml --env-file .env.ai up -d

# Xem log
docker compose -f docker-compose.ai.yml logs -f
```

### Bước 3: Test thử

```bash
# Kiểm tra health
curl http://localhost:8000/health

# Hỏi AI về project
curl -X POST http://localhost:8000/chat \
  -H "Content-Type: application/json" \
  -d '{"question": "Project này có những chức năng gì?", "context_type": "summary"}'

# Hỏi về một domain cụ thể
curl -X POST http://localhost:8000/chat \
  -H "Content-Type: application/json" \
  -d '{"question": "Controller đặt phòng hoạt động như thế nào?", "context_type": "domain", "domain": "booking"}'
```

## API Endpoints

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/health` | Kiểm tra service |
| POST | `/chat` | Gửi câu hỏi, nhận câu trả lời tiếng Việt |
| POST | `/refresh-context` | Làm mới cache (khi code thay đổi) |
| GET | `/context-info` | Xem thông tin context hiện tại |

### POST `/chat` - Request body

```json
{
  "question": "Câu hỏi của bạn",
  "context_type": "full",     // "full" | "summary" | "domain"
  "domain": "booking"         // Chỉ khi context_type = "domain"
}
```

**context_type:**
- `full` — Đọc toàn bộ project (chậm hơn, đầy đủ hơn)
- `summary` — Chỉ đọc file quan trọng (nhanh hơn, khuyến nghị)
- `domain` — Chỉ đọc file liên quan domain: `room`, `booking`, `customer`, `payment`, `auth`

## Tích hợp vào Next.js Frontend

1. Copy `examples/AIChatBot.tsx` vào `app/components/`
2. Copy `examples/route.ts` vào `app/api/ai-chat/`
3. Thêm vào `app/.env.local`:
   ```
   AI_SERVICE_URL=http://localhost:8000
   ```
4. Dùng component:
   ```tsx
   import AIChatBot from "@/components/AIChatBot";
   // ...
   <AIChatBot />
   ```

## Lưu ý

- **DeepSeek account**: Dùng tài khoản DeepSeek Web (chat.deepseek.com), không cần API key trả phí.
- **Context limit**: Project lớn nên dùng `context_type: "summary"` hoặc `"domain"` để tránh vượt token limit.
- **Cache**: Context được cache khi service khởi động. Gọi `/refresh-context` khi cần làm mới.
