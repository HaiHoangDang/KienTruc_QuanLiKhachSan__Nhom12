# Kiến trúc hiện tại của hệ thống DKS Hotel Manager

## 1. Tổng quan

Hệ thống DKS Hotel Manager ban đầu được xây dựng theo mô hình ASP.NET MVC truyền thống. Project chính `DKS_HotelManager` đảm nhiệm giao diện người dùng, controller xử lý nghiệp vụ, model Entity Framework và truy cập cơ sở dữ liệu SQL Server.

Trong quá trình kiến trúc hóa, hệ thống được tách dần thành các service độc lập để mô phỏng kiến trúc microservices. Các service được đặt trong thư mục `services` và được truy cập thông qua API Gateway sử dụng Ocelot.

## 2. Thành phần hiện tại

- `DKS_HotelManager`: ứng dụng web MVC chính, đóng vai trò giao diện người dùng.
- `DKS.Gateway`: API Gateway sử dụng Ocelot để định tuyến request.
- `auth-service`: xử lý đăng nhập và sinh JWT.
- `room-service`: cung cấp API tra cứu phòng, loại phòng, trạng thái phòng.
- `booking-service`: xử lý đặt phòng và ghi dữ liệu thuê phòng.
- `payment-service`: xử lý ghi nhận thanh toán.
- `notification-service`: xử lý thông báo.
- `ai-service`: chatbot AI hỗ trợ tư vấn khách sạn.

## 3. Cơ sở dữ liệu

Các service hiện dùng chung cơ sở dữ liệu SQL Server `DKS_HotelManager`. Đây là cách tiếp cận shared database để phù hợp với phạm vi đồ án và dễ tích hợp với hệ thống MVC cũ.

## 4. Đánh giá

Kiến trúc hiện tại là kiến trúc lai giữa monolithic MVC và microservices. Một số nghiệp vụ cũ vẫn còn nằm trong MVC, trong khi các nghiệp vụ chính đang được tách dần sang service độc lập.