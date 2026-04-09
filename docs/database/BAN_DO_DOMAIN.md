# BẢN ĐỒ DOMAIN

## 1. Auth Domain
### Controller liên quan
- AuthenticationController
- AccountController

### Bảng dữ liệu liên quan
- KHACHHANG
- NHANVIEN

### Chức năng
- Đăng nhập
- Đăng ký
- Phân quyền

---

## 2. Room Domain
### Controller liên quan
- RoomController
- HotelController

### Bảng dữ liệu liên quan
- KHACHSAN
- PHONG
- LOAIPHONG
- TIENICH
- PHONG_TIENICH
- TRANGTHAI_PHONG

### Chức năng
- Quản lý khách sạn
- Quản lý phòng
- Quản lý loại phòng
- Kiểm tra phòng trống

---

## 3. Booking Domain
### Controller liên quan
- BookingController

### Bảng dữ liệu liên quan
- THUEPHONG
- CTTHUEPHONG

### Chức năng
- Tạo đặt phòng
- Hủy đặt phòng
- Cập nhật trạng thái đặt phòng

---

## 4. Payment Domain
### Bảng dữ liệu liên quan
- THANHTOAN
- SDDICHVU

### Chức năng
- Xử lý thanh toán
- Lưu lịch sử giao dịch

---

## 5. Notification Domain
### Chức năng
- Gửi email xác nhận
- Gửi OTP
- Thông báo thay đổi trạng thái

---

## 6. Chatbot Domain
### Thành phần liên quan
- ChatBotController
- thư mục `app/`

### Chức năng
- Chat với khách hàng
- Tư vấn thông tin khách sạn
- Giải đáp câu hỏi tự động