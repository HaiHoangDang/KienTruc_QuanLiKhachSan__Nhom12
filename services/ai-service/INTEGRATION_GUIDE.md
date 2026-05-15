# HƯỚNG DẪN TÍCH HỢP AI SERVICE v4

## Những thay đổi so với v3

### Vấn đề cũ (v3)
- Mỗi lần chat → ds2api tạo conversation mới → AI không nhớ gì
- Lịch sử trong DB có nhưng chỉ dùng làm `history[]` chưa đủ

### Giải pháp mới (v4)
```
[Mỗi lần chat]
    ↓
Load lịch sử SQLite → inject vào system prompt (AI "nhớ" ngay từ đầu)
    +
Load history[] → gửi kèm messages (AI hiểu turn-taking)
    ↓
ds2api tạo conversation → AI trả lời → ds2api xóa conversation
    ↓
Lưu Q&A mới vào SQLite
```

AI không bao giờ "mất trí" vì ngữ cảnh được build lại từ DB mỗi request.

---

## API Changes — POST /chat

### Request body (thêm 2 field mới)
```json
{
  "question":      "Tôi muốn đặt phòng tại Vũng Tàu",
  "customer_id":   "KH001",
  "customer_name": "Nguyễn Văn A",      // MỚI: tên khách từ session
  "is_logged_in":  true,                // MỚI: trạng thái đăng nhập
  "db_context":    "• P402 — 1.000.000₫/đêm — Luxury Vung Tau View\n..."
}
```

### Response (không đổi)
```json
{
  "answer":      "Dạ, hiện còn phòng P402 tại Luxury Vung Tau View...",
  "customer_id": "KH001"
}
```

---

## Tích hợp từ Next.js (TypeScript)

### File: app/api/ai-chat/route.ts
```typescript
import { NextRequest, NextResponse } from "next/server";

const AI_SERVICE_URL = process.env.AI_SERVICE_URL || "http://localhost:8000";

export async function POST(req: NextRequest) {
  const body = await req.json();

  // Lấy thông tin từ session/cookie
  const session = await getServerSession(); // hoặc cách bạn đang dùng
  
  const payload = {
    question:      body.question,
    customer_id:   session?.user?.customerId || "guest",
    customer_name: session?.user?.name || null,
    is_logged_in:  !!session?.user,
    db_context:    body.db_context || null,
  };

  const res = await fetch(`${AI_SERVICE_URL}/chat`, {
    method:  "POST",
    headers: { "Content-Type": "application/json" },
    body:    JSON.stringify(payload),
  });

  const data = await res.json();
  return NextResponse.json(data);
}
```

### Load lịch sử khi đăng nhập (quan trọng!)
```typescript
// Gọi ngay sau khi user đăng nhập thành công
async function loadChatHistory(customerId: string) {
  const res = await fetch(`${AI_SERVICE_URL}/history/${customerId}`);
  const data = await res.json();
  // data.messages = [{role, content, created_at}, ...]
  setChatMessages(data.messages); // hiển thị lên UI
}
```

---

## Tích hợp từ C# (ASP.NET)

### ChatController.cs
```csharp
[HttpPost("api/ai-chat")]
public async Task<IActionResult> AiChat([FromBody] ChatRequest req)
{
    var customerId = User.Identity?.IsAuthenticated == true
        ? User.FindFirst("CustomerId")?.Value ?? "guest"
        : "guest";
    
    var customerName = User.Identity?.IsAuthenticated == true
        ? User.FindFirst(ClaimTypes.Name)?.Value
        : null;

    // Build db_context từ database thật
    var dbContext = await BuildDbContext(); // phòng trống, giá...

    var payload = new {
        question      = req.Question,
        customer_id   = customerId,
        customer_name = customerName,
        is_logged_in  = User.Identity?.IsAuthenticated ?? false,
        db_context    = dbContext,
    };

    var response = await _httpClient.PostAsJsonAsync(
        $"{_aiServiceUrl}/chat", payload
    );
    
    var result = await response.Content.ReadFromJsonAsync<ChatResponse>();
    return Ok(result);
}

// Load lịch sử khi user vào trang chat
[HttpGet("api/ai-history")]
public async Task<IActionResult> GetHistory()
{
    var customerId = User.FindFirst("CustomerId")?.Value;
    if (customerId == null) return Ok(new { messages = Array.Empty<object>() });

    var response = await _httpClient.GetAsync(
        $"{_aiServiceUrl}/history/{customerId}"
    );
    var result = await response.Content.ReadAsStringAsync();
    return Content(result, "application/json");
}
```

---

## Luồng hoàn chỉnh khi user chat

```
1. User đăng nhập
   → Frontend gọi GET /history/{customer_id}
   → Hiển thị toàn bộ lịch sử cũ lên UI

2. User gõ tin nhắn mới
   → Frontend gọi POST /chat với {question, customer_id, customer_name, is_logged_in, db_context}
   → AI Service load lịch sử từ SQLite
   → Build context: login state + DB data + lịch sử text (inject vào system prompt)
   → Gửi lên ds2api: system + context + history[] + câu hỏi mới
   → ds2api tạo conversation DeepSeek → nhận trả lời → xóa conversation
   → Lưu Q&A vào SQLite
   → Trả answer về frontend
   → Frontend thêm message mới vào UI

3. Session kế tiếp (đăng nhập lại)
   → Bước 1 lặp lại → lịch sử cũ hiện ra đầy đủ
   → AI nhớ toàn bộ ngữ cảnh qua lịch sử được inject vào system prompt
```

---

## Ví dụ system prompt AI nhận được (khi có lịch sử)

```
<hotel_data>
IS_LOGGED_IN=true
CUSTOMER_NAME=Nguyễn Văn A
CUSTOMER_ID=KH001
• P402 — 1.000.000₫/đêm — Luxury Vung Tau View
• P101 — 1.200.000₫/đêm — Luxury Saigon Hotel

=== LỊCH SỬ CUỘC TRÒ CHUYỆN TRƯỚC ĐÓ ===
[2026-05-14 09:00] Khách: Tôi muốn đặt phòng tại Vũng Tàu ngày 20/5
[2026-05-14 09:00] Nhân viên AI: Dạ còn phòng P402 tại Luxury Vung Tau View...
[2026-05-14 09:01] Khách: Giá bao nhiêu?
[2026-05-14 09:01] Nhân viên AI: P402 giá 1.000.000₫/đêm, view biển đẹp...
=== KẾT THÚC LỊCH SỬ ===
</hotel_data>
```

→ AI hiểu ngay: khách này đang hỏi về P402 ở Vũng Tàu, tiếp tục tư vấn mạch lạc.
