# Tổng quan các microservice

## 1. auth-service

Phụ trách đăng nhập và cấp JWT token cho người dùng.

Endpoint chính:

- `POST /api/auth/login`

## 2. room-service

Phụ trách cung cấp thông tin phòng.

Endpoint chính:

- `GET /api/room`
- `GET /api/room/{id}`
- `GET /api/room/test`

## 3. booking-service

Phụ trách xử lý đặt phòng.

Endpoint chính:

- `GET /api/booking`
- `GET /api/booking/{id}`
- `POST /api/booking`
- `GET /api/booking/test`

## 4. payment-service

Phụ trách ghi nhận thanh toán.

Endpoint chính:

- `GET /api/payment`
- `GET /api/payment/{id}`
- `POST /api/payment`
- `GET /api/payment/test`

## 5. notification-service

Phụ trách thông báo.

Endpoint chính:

- `POST /api/notification/send`
- `POST /api/notification/booking-success`
- `GET /api/notification/test`

## 6. ai-service

Phụ trách chatbot AI tư vấn khách sạn.

Endpoint chính:

- `POST /chat`

Qua gateway:

- `POST /api/ai/chat`