/**
 * app/api/ai-chat/route.ts
 * 
 * Next.js API Route: proxy câu hỏi từ frontend đến ai-service.
 * Đặt file này vào: app/api/ai-chat/route.ts
 */

import { NextRequest, NextResponse } from "next/server";

const AI_SERVICE_URL = process.env.AI_SERVICE_URL || "http://localhost:8000";

export async function POST(req: NextRequest) {
  try {
    const body = await req.json();

    const res = await fetch(`${AI_SERVICE_URL}/chat`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    });

    if (!res.ok) {
      const error = await res.text();
      return NextResponse.json(
        { error: `AI service lỗi: ${error}` },
        { status: res.status }
      );
    }

    const data = await res.json();
    return NextResponse.json(data);
  } catch (err) {
    console.error("AI chat error:", err);
    return NextResponse.json(
      { error: "Không thể kết nối đến AI service" },
      { status: 500 }
    );
  }
}
