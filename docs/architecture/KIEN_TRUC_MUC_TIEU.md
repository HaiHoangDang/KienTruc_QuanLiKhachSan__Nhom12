# KIẾN TRÚC MỤC TIÊU

## 1. Mục tiêu tổng quát
Chuyển hệ thống hiện tại từ monolith sang kiến trúc gần với microservices, trong đó mỗi service có cấu trúc rõ ràng:
- Controller/API
- Service
- Repository
- Database

## 2. Thành phần kiến trúc đích

### Client
- ReactJS frontend

### API Gateway
- Điều hướng request
- Kiểm tra JWT
- Giới hạn request

### Các service chính
- Auth Service
- User Service
- Room Service
- Booking Service
- Payment Service
- Notification Service
- AI Chatbot Service

### Giao tiếp bất đồng bộ
- RabbitMQ

### Triển khai
- Docker
- Docker Compose

## 3. Hướng triển khai
- Giai đoạn 1: Chuẩn hóa monolith hiện tại
- Giai đoạn 2: Tách dần từng service
- Giai đoạn 3: Bổ sung API Gateway, RabbitMQ và Docker