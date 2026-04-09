# KIẾN TRÚC HIỆN TẠI

## 1. Công nghệ đang dùng
- ASP.NET MVC 5
- Entity Framework
- SQL Server
- FastAPI cho chatbot
- JavaScript/Tailwind ở giao diện hiện tại

## 2. Thành phần hiện có
- `DKS_HotelManager.sln`: solution chính
- `DKS_HotelManager/`: ứng dụng web chính
- `app/`: service chatbot viết bằng Python
- `Database/`: script cơ sở dữ liệu

## 3. Đặc điểm kiến trúc hiện tại
- Hệ thống chính đang là monolith theo ASP.NET MVC
- Chatbot đã là một service riêng, được gọi qua HTTP
- Chưa có API Gateway
- Chưa có RabbitMQ
- Chưa tách service theo domain hoàn chỉnh

## 4. Nhận xét
- Hệ thống đã có nền tảng để chuyển sang hướng microservices
- Tuy nhiên hiện tại business logic còn nằm nhiều trong controller
- Cần chuẩn hóa lại theo mô hình Controller -> Service -> Repository