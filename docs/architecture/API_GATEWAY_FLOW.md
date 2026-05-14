# Luồng API Gateway

## 1. Vai trò API Gateway

API Gateway sử dụng Ocelot để định tuyến request từ MVC Web hoặc Postman đến các service tương ứng.

## 2. Luồng tổng quát

```txt
Client / MVC Web
   |
   v
http://localhost:6000
   |
   v
Ocelot Gateway
   |
   v
Service tương ứng