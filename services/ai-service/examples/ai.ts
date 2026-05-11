/**
 * lib/ai.ts
 * 
 * Utility gọi AI service từ Next.js frontend.
 * Đặt file này vào: app/lib/ai.ts (hoặc app/utils/ai.ts)
 */

const AI_SERVICE_URL = process.env.AI_SERVICE_URL || "http://localhost:8000";

export type ContextType = "full" | "summary" | "domain";

export interface ChatOptions {
  question: string;
  contextType?: ContextType;
  domain?: string; // "room" | "booking" | "customer" | "payment" | "auth"
}

export interface ChatResult {
  answer: string;
  contextLength: number;
  model: string;
}

/**
 * Gửi câu hỏi đến AI service và nhận câu trả lời tiếng Việt.
 */
export async function askAI(options: ChatOptions): Promise<ChatResult> {
  const res = await fetch(`${AI_SERVICE_URL}/chat`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      question: options.question,
      context_type: options.contextType ?? "summary",
      domain: options.domain,
    }),
  });

  if (!res.ok) {
    const err = await res.text();
    throw new Error(`AI Service lỗi: ${err}`);
  }

  const data = await res.json();
  return {
    answer: data.answer,
    contextLength: data.context_length,
    model: data.model,
  };
}

// ─── Ví dụ sử dụng trong component ───────────────────────────────────────────
//
// import { askAI } from "@/lib/ai";
//
// const answer = await askAI({
//   question: "Danh sách các phòng trống hôm nay",
//   contextType: "domain",
//   domain: "room",
// });
//
// console.log(answer.answer); // Câu trả lời tiếng Việt
