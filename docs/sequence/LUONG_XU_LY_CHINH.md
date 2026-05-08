# LUỒNG XỬ LÝ CHÍNH

## 1. Luồng đăng nhập
1. Người dùng nhập tài khoản và mật khẩu
2. Hệ thống gửi request đến Auth Service
3. Auth Service kiểm tra thông tin
4. Nếu hợp lệ, hệ thống trả về JWT
5. Client dùng JWT cho các request tiếp theo

## 2. Luồng đặt phòng
1. Người dùng chọn phòng và ngày đặt
2. Hệ thống gọi Room Service để kiểm tra phòng trống
3. Nếu hợp lệ, Booking Service tạo booking
4. Payment Service xử lý thanh toán
5. Notification Service gửi email xác nhận

## 3. Luồng chatbot
1. Người dùng nhập câu hỏi
2. Frontend hoặc MVC gửi request đến Chatbot Service
3. Chatbot Service xử lý intent
4. Nếu cần, service gọi mô hình AI
5. Kết quả được trả về cho người dùng