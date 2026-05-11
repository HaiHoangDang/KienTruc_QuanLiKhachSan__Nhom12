"use client";
/**
 * components/AIChatBot.tsx
 * 
 * Component chatbot AI cho hệ thống khách sạn.
 * Đặt vào: app/components/AIChatBot.tsx
 */

import { useState } from "react";

const DOMAINS = [
  { value: "full",     label: "Toàn bộ project" },
  { value: "room",     label: "Phòng" },
  { value: "booking",  label: "Đặt phòng" },
  { value: "customer", label: "Khách hàng" },
  { value: "payment",  label: "Thanh toán" },
  { value: "auth",     label: "Đăng nhập" },
];

interface Message {
  role: "user" | "assistant";
  content: string;
}

export default function AIChatBot() {
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState("");
  const [domain, setDomain] = useState("full");
  const [loading, setLoading] = useState(false);

  async function sendMessage() {
    if (!input.trim() || loading) return;

    const userMsg: Message = { role: "user", content: input };
    setMessages((prev) => [...prev, userMsg]);
    setInput("");
    setLoading(true);

    try {
      const isFullContext = domain === "full";
      const res = await fetch("/api/ai-chat", {  // Next.js API route
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          question: input,
          context_type: isFullContext ? "summary" : "domain",
          domain: isFullContext ? undefined : domain,
        }),
      });

      const data = await res.json();
      const aiMsg: Message = { role: "assistant", content: data.answer };
      setMessages((prev) => [...prev, aiMsg]);
    } catch (err) {
      setMessages((prev) => [
        ...prev,
        { role: "assistant", content: "Xin lỗi, đã có lỗi xảy ra. Vui lòng thử lại." },
      ]);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="flex flex-col h-[600px] border rounded-xl shadow-lg bg-white">
      {/* Header */}
      <div className="bg-blue-600 text-white p-4 rounded-t-xl flex items-center gap-2">
        <span className="text-xl">🤖</span>
        <div>
          <div className="font-semibold">Trợ Lý AI - DKS Hotel</div>
          <div className="text-xs opacity-80">Powered by DeepSeek</div>
        </div>
      </div>

      {/* Domain selector */}
      <div className="px-4 py-2 border-b bg-gray-50 flex gap-2 flex-wrap">
        {DOMAINS.map((d) => (
          <button
            key={d.value}
            onClick={() => setDomain(d.value)}
            className={`px-3 py-1 rounded-full text-sm transition-colors ${
              domain === d.value
                ? "bg-blue-600 text-white"
                : "bg-white border text-gray-600 hover:bg-blue-50"
            }`}
          >
            {d.label}
          </button>
        ))}
      </div>

      {/* Messages */}
      <div className="flex-1 overflow-y-auto p-4 space-y-3">
        {messages.length === 0 && (
          <div className="text-center text-gray-400 mt-8">
            <div className="text-4xl mb-2">💬</div>
            <p>Hãy hỏi tôi về hệ thống quản lý khách sạn!</p>
            <p className="text-sm mt-1">Ví dụ: "Giải thích cách đặt phòng hoạt động"</p>
          </div>
        )}
        {messages.map((msg, i) => (
          <div
            key={i}
            className={`flex ${msg.role === "user" ? "justify-end" : "justify-start"}`}
          >
            <div
              className={`max-w-[80%] rounded-2xl px-4 py-2 text-sm whitespace-pre-wrap ${
                msg.role === "user"
                  ? "bg-blue-600 text-white rounded-br-sm"
                  : "bg-gray-100 text-gray-800 rounded-bl-sm"
              }`}
            >
              {msg.content}
            </div>
          </div>
        ))}
        {loading && (
          <div className="flex justify-start">
            <div className="bg-gray-100 rounded-2xl rounded-bl-sm px-4 py-2 text-sm text-gray-500">
              Đang suy nghĩ...
            </div>
          </div>
        )}
      </div>

      {/* Input */}
      <div className="p-4 border-t flex gap-2">
        <input
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyDown={(e) => e.key === "Enter" && !e.shiftKey && sendMessage()}
          placeholder="Nhập câu hỏi về hệ thống khách sạn..."
          className="flex-1 border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
          disabled={loading}
        />
        <button
          onClick={sendMessage}
          disabled={loading || !input.trim()}
          className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium disabled:opacity-50 hover:bg-blue-700 transition-colors"
        >
          Gửi
        </button>
      </div>
    </div>
  );
}
