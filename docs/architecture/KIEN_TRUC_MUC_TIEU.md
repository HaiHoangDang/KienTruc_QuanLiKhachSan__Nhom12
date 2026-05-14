# Kiến trúc mục tiêu

## 1. Mục tiêu

Mục tiêu của hệ thống là chuyển dần từ kiến trúc MVC nguyên khối sang kiến trúc hướng microservices. Ứng dụng MVC chỉ giữ vai trò giao diện, còn các nghiệp vụ chính được xử lý bởi các service độc lập thông qua API Gateway.

## 2. Mô hình mục tiêu

```txt
Người dùng
   |
   v
DKS_HotelManager MVC Web
   |
   v
Ocelot API Gateway
   |
   |-- auth-service
   |-- room-service
   |-- booking-service
   |-- payment-service
   |-- notification-service
   |-- ai-service
   |
   v
SQL Server: DKS_HotelManager