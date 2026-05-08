-- 1. Tạo Database
CREATE DATABASE DKS_HotelManager;
GO
USE DKS_HotelManager;
GO

-- 2. Bảng Khách sạn
CREATE TABLE KHACHSAN
(
    MaKS INT NOT NULL PRIMARY KEY,
    TenKS NVARCHAR(100) NOT NULL,
    DiaDiem NVARCHAR(200),
    MoTa NVARCHAR(500),
	HinhAnh NVARCHAR(300)
);

-- 3. Loại phòng
CREATE TABLE LOAIPHONG
(
    MaLoai INT NOT NULL PRIMARY KEY,
    TenLoai NVARCHAR(50) NOT NULL,
    GhiChu NVARCHAR(200)
);

-- 4. Phòng
CREATE TABLE PHONG
(
    MaPhong INT NOT NULL PRIMARY KEY,
    TenPhong NVARCHAR(50) NOT NULL,
    MaKS INT NOT NULL,
    MaLoai INT NOT NULL,
    SucChua INT NOT NULL,
    Tang INT NOT NULL,
    DienTich FLOAT,
    DGNgay MONEY NOT NULL,
    CONSTRAINT FK_PHONG_KS FOREIGN KEY (MaKS) REFERENCES KHACHSAN(MaKS),
    CONSTRAINT FK_PHONG_LOAIPHONG FOREIGN KEY (MaLoai) REFERENCES LOAIPHONG(MaLoai)
);

-- 5. Tiện ích
CREATE TABLE TIENICH
(
    MaTI INT NOT NULL PRIMARY KEY,
    TenTI NVARCHAR(100) NOT NULL,
    MoTa NVARCHAR(200)
);

-- 6. Liên kết phòng - tiện ích (many-to-many)
CREATE TABLE PHONG_TIENICH
(
    MaPhong INT NOT NULL,
    MaTI INT NOT NULL,
    PRIMARY KEY (MaPhong, MaTI),
    CONSTRAINT FK_PHONGTIENICH_PHONG FOREIGN KEY (MaPhong) REFERENCES PHONG(MaPhong),
    CONSTRAINT FK_PHONGTIENICH_TI FOREIGN KEY (MaTI) REFERENCES TIENICH(MaTI)
);

-- 7. Nhân viên
CREATE TABLE NHANVIEN
(
    MaNV INT NOT NULL PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    NgaySinh DATE,
    SoDT NVARCHAR(20),
    ChucVu NVARCHAR(50),
    TenDN NVARCHAR(100) UNIQUE NOT NULL,
    MatKhau NVARCHAR(200) NOT NULL,
    Email NVARCHAR(150)
);

-- 8. Khách hàng
CREATE TABLE KHACHHANG
(
    MKH INT NOT NULL PRIMARY KEY,
    TKH NVARCHAR(100) NOT NULL,
    DiaChi NVARCHAR(200),
    SDT NVARCHAR(20),
    CMND_CCCD NVARCHAR(20) UNIQUE,
    TenDN NVARCHAR(100) UNIQUE NOT NULL,
    MatKhau NVARCHAR(200) NOT NULL,
    Email NVARCHAR(150)
);

-- 9. Dịch vụ
CREATE TABLE DICHVU
(
    MaDV INT NOT NULL PRIMARY KEY,
    TenDV NVARCHAR(100) NOT NULL,
    DGDV MONEY NOT NULL
);

-- 10. Thuê phòng
CREATE TABLE THUEPHONG
(
    MaThue INT NOT NULL PRIMARY KEY,
    MaNV INT NOT NULL,
    MaPhong INT NOT NULL,
    NgayDat DATE,
    NgayVao DATE,
    NgayTra DATE,
    DatCoc MONEY,
    MaDatPhong NVARCHAR(50) UNIQUE,
    TrangThai NVARCHAR(30),
    CONSTRAINT FK_THUEPHONG_NV FOREIGN KEY (MaNV) REFERENCES NHANVIEN(MaNV),
    CONSTRAINT FK_THUEPHONG_PHONG FOREIGN KEY (MaPhong) REFERENCES PHONG(MaPhong)
);

-- 11. Chi tiết thuê phòng (nhiều khách trong một phòng)
CREATE TABLE CTTHUEPHONG
(
    MaThue INT NOT NULL,
    KHACH INT NOT NULL,
    VaiTro NVARCHAR(50),
    PRIMARY KEY (MaThue, KHACH),
    CONSTRAINT FK_CTTHUEPHONG_THUE FOREIGN KEY (MaThue) REFERENCES THUEPHONG(MaThue),
    CONSTRAINT FK_CTTHUEPHONG_KHACH FOREIGN KEY (KHACH) REFERENCES KHACHHANG(MKH)
);

-- 12. Trạng thái phòng theo ngày
CREATE TABLE TRANGTHAI_PHONG
(
    MaTrang INT NOT NULL PRIMARY KEY,
    MaPhong INT NOT NULL,
    TrangThai NVARCHAR(100) NOT NULL,
    NgayCapNhat DATE NOT NULL,
    MaNVCapNhat INT NULL,
    GhiChu NVARCHAR(300),
    CONSTRAINT FK_TRANGTHAI_PHONG FOREIGN KEY (MaPhong) REFERENCES PHONG(MaPhong),
    CONSTRAINT FK_TRANGTHAI_NV FOREIGN KEY (MaNVCapNhat) REFERENCES NHANVIEN(MaNV)
);

-- 13. Sử dụng dịch vụ
CREATE TABLE SDDICHVU
(
    MaThue INT NOT NULL,
    DV INT NOT NULL,
    SoLuot INT DEFAULT 1,
    PRIMARY KEY (MaThue, DV),
    CONSTRAINT FK_SDDV_THUE FOREIGN KEY (MaThue) REFERENCES THUEPHONG(MaThue),
    CONSTRAINT FK_SDDV_DV FOREIGN KEY (DV) REFERENCES DICHVU(MaDV)
);

-- 14. Thanh toán
CREATE TABLE THANHTOAN
(
    MaTT INT NOT NULL PRIMARY KEY,
    MaThue INT NOT NULL,
    HinhThucTT NVARCHAR(50) NOT NULL,
    ThanhTien MONEY,
    NgayTT DATE,
    CONSTRAINT FK_THANHTOAN_THUE FOREIGN KEY (MaThue) REFERENCES THUEPHONG(MaThue)
);

-- 15. Hình ảnh
CREATE TABLE HINHANH
(
    MaHinh INT NOT NULL PRIMARY KEY,
    MaPhong INT NULL,
    MKH INT NULL,
    DuongDan NVARCHAR(500) NOT NULL,
    MoTa NVARCHAR(300),
    NgayUp DATE,
    CONSTRAINT FK_HINHANH_PHONG FOREIGN KEY (MaPhong) REFERENCES PHONG(MaPhong),
    CONSTRAINT FK_HINHANH_KHACH FOREIGN KEY (MKH) REFERENCES KHACHHANG(MKH)
);

-- 16. Bình luận
CREATE TABLE BINHLUAN
(
    MaBL INT NOT NULL PRIMARY KEY,
    MaPhong INT NULL,
    MaKS INT NULL,
    MKH INT NOT NULL,
    NoiDung NVARCHAR(1000),
    NgayBL DATE,
    CONSTRAINT FK_BINHLUAN_PHONG FOREIGN KEY (MaPhong) REFERENCES PHONG(MaPhong),
    CONSTRAINT FK_BINHLUAN_KS FOREIGN KEY (MaKS) REFERENCES KHACHSAN(MaKS),
    CONSTRAINT FK_BINHLUAN_KHACH FOREIGN KEY (MKH) REFERENCES KHACHHANG(MKH)
);

INSERT INTO KHACHSAN VALUES
(1, N'Luxury Saigon Hotel', N'01 Le Loi, Quan 1, TP Ho Chi Minh', N'Khach san 5 sao trung tam Sai Gon', N'ks01.jpg'),
(2, N'Luxury Hanoi Hotel', N'45 Ly Thuong Kiet, Hoan Kiem, Ha Noi', N'Khach san sang trong tai Ha Noi', N'ks02.jpg'),
(3, N'Luxury Da Nang Resort', N'12 Vo Nguyen Giap, Da Nang', N'Khu nghi duong cao cap gan bien', N'ks03.jpg'),
(4, N'Luxury Nha Trang Beach', N'89 Tran Phu, Nha Trang', N'Khach san gan bien Nha Trang', N'ks04.jpg'),
(5, N'Luxury Hue Palace', N'25 Le Loi, TP Hue', N'Khach san gan song Huong', N'ks05.jpg'),
(6, N'Luxury Vung Tau View', N'102 Thuy Van, Vung Tau', N'Khach san huong bien', N'ks06.jpg'),
(7, N'Luxury Ha Long Bay', N'56 Halong Road, Quang Ninh', N'Khach san gan vinh Ha Long', N'ks07.jpg'),
(8, N'Luxury Sapa Mountain', N'21 Muong Hoa, Sapa', N'Khach san tren nui voi tam nhin dep', N'ks08.jpg'),
(9, N'Luxury Can Tho River', N'05 Hai Ba Trung, Can Tho', N'Khach san ven song Hau', N'ks09.jpg'),
(10, N'Luxury Phu Quoc Resort', N'33 Tran Hung Dao, Phu Quoc', N'Khu nghi duong bien sang trong', N'ks10.jpg'),
(11, N'Luxury Da Lat Hill', N'18 Phan Boi Chau, Da Lat', N'Khach san tren doi voi khong khi se lanh', N'ks11.jpg'),
(12, N'Luxury Quy Nhon Sea', N'72 Xuan Dieu, Quy Nhon', N'Khach san ven bien Quy Nhon tuyet dep', N'ks12.jpg'),
(13, N'Luxury Buon Ma Thuot Central', N'10 Nguyen Tat Thanh, Buon Ma Thuot', N'Khach san gan trung tam thanh pho Tay Nguyen', N'ks13.jpg'),
(14, N'Luxury Bien Hoa City', N'45 Pham Van Thuan, Bien Hoa', N'Khach san hien dai tai thanh pho cong nghiep', N'ks14.jpg'),
(15, N'Luxury Hai Phong Harbor', N'155 Tran Hung Dao, Hai Phong', N'Khach san gan caang Hai Phong', N'ks15.jpg'),
(16, N'Luxury My Tho Riverside', N'22 Ap Bac, My Tho', N'Khach san ben song Tien thoang mat', N'ks16.jpg'),
(17, N'Luxury Kon Tum Highland', N'30 Nguyen Hue, Kon Tum', N'Khach san vung cao nguyen yen binh', N'ks17.jpg'),
(18, N'Luxury Rach Gia Bay', N'99 Nguyen Trung Truc, Rach Gia', N'Khach san gan bien Rach Gia', N'ks18.jpg'),
(19, N'Luxury Long Xuyen Center', N'21 Tran Hung Dao, Long Xuyen', N'Khach san trung tam thanh pho', N'ks19.jpg'),
(20, N'Luxury Bac Lieu Melody', N'66 Hung Vuong, Bac Lieu', N'Khach san thanh lich tai Bac Lieu', N'ks20.jpg'),
(21, N'Luxury Pleiku Green', N'34 Le Loi, Pleiku', N'Khach san voi tam nhin nui doi', N'ks21.jpg'),
(22, N'Luxury Tay Ninh View', N'12 Quang Trung, Tay Ninh', N'Khach san gan nui Ba Den', N'ks22.jpg'),
(23, N'Luxury Cam Ranh Bay', N'88 Nguyen Tat Thanh, Cam Ranh', N'Khach san nghhi duong ven vinh', N'ks23.jpg'),
(24, N'Luxury Binh Thuan Beach', N'49 Nguyen Dinh Chieu, Phan Thiet', N'Khach san gan bien Mui Ne', N'ks24.jpg'),
(25, N'Luxury Thanh Hoa Pearl', N'23 Le Loi, Thanh Hoa', N'Khach san sang trong gan trung tam', N'ks25.jpg');


INSERT INTO LOAIPHONG VALUES
(1, N'Phong Don', N'1 Giuong don, phu hop ca nhan'),
(2, N'Phong Doi', N'2 Giuong don hoac 1 giuong doi'),
(3, N'Suite', N'Phong cao cap voi phong khach rieng'),
(4, N'Family', N'Phong cho gia dinh 4 nguoi'),
(5, N'VIP', N'Phong VIP huong bien'),
(6, N'Penthouse', N'Can ho cao cap tren tang thuong'),
(7, N'Standard', N'Phong tieu chuan gia tot'),
(8, N'Deluxe', N'Phong deluxe sang trong'),
(9, N'Executive', N'Phong danh cho khach doanh nhan'),
(10, N'Presidential Suite', N'Phong tong thong sieu sang'),
(11, N'Superior', N'Phong superior rong rai, tien nghi'),
(12, N'Connecting Room', N'Hai phong thong nhau phu hop gia dinh'),
(13, N'Junior Suite', N'Phong suite nho voi phong khach mini'),
(14, N'Honeymoon Suite', N'Phong danh rieng cho cap doi moi cuoi'),
(15, N'Luxury Ocean View', N'Phong cao cap nhin bao quat bien'),
(16, N'Garden View', N'Phong nhin khu vuon xanh mat'),
(17, N'Business Room', N'Phong toi uu cho khach cong tac, ban lam viec rieng'),
(18, N'Royal Suite', N'Phong hoang gia rong lon va sang trong'),
(19, N'Mountain View', N'Phong nhin nui tam nhin dep'),
(20, N'City View Deluxe', N'Phong deluxe nhin toan canh thanh pho'),
(21, N'Twin Deluxe', N'Phong deluxe 2 giuong don'),
(22, N'King Room', N'Phong giuong lon king size'),
(23, N'Queen Room', N'Phong giuong queen size'),
(24, N'Premier Suite', N'Phong suite cao cap trang bi tien nghi VIP'),
(25, N'Heritage Room', N'Phong thiet ke co dien dam chat van hoa dia phuong');

INSERT INTO PHONG VALUES
(1, N'P101', 1, 1, 2, 1, 25.0, 1200000),
(2, N'P102', 1, 2, 2, 1, 30.0, 1500000),
(3, N'P201', 2, 3, 4, 2, 40.0, 2000000),
(4, N'P202', 2, 5, 2, 2, 35.0, 2500000),
(5, N'P301', 3, 4, 4, 3, 50.0, 1800000),
(6, N'P302', 4, 8, 4, 3, 45.0, 2200000),
(7, N'P401', 5, 9, 2, 4, 55.0, 3000000),
(8, N'P402', 6, 7, 2, 4, 28.0, 1000000),
(9, N'P501', 7, 10, 4, 5, 60.0, 5000000),
(10, N'P502', 8, 6, 2, 5, 70.0, 6000000),
(11, N'P503', 8, 6, 4, 5, 65.0, 5500000),
(12, N'P601', 3, 4, 2, 6, 32.0, 1600000),
(13, N'P602', 3, 4, 4, 6, 38.0, 1800000),
(14, N'P701', 2, 3, 2, 7, 45.0, 2700000),
(15, N'P702', 2, 3, 4, 7, 48.0, 3000000),
(16, N'P801', 1, 2, 2, 8, 28.0, 1300000),
(17, N'P802', 1, 5, 4, 8, 36.0, 2000000),
(18, N'P901', 4, 8, 2, 9, 52.0, 3200000),
(19, N'P902', 4, 8, 4, 9, 58.0, 3500000),
(20, N'P1001', 5, 9, 2, 10, 60.0, 4000000),
(21, N'P1002', 6, 7, 4, 10, 42.0, 1900000),
(22, N'P1101', 7, 10, 4, 11, 75.0, 6500000),
(23, N'P1102', 8, 6, 2, 11, 55.0, 2800000),
(24, N'P1201', 9, 10, 4, 12, 82.0, 7200000),
(25, N'P1202', 10, 8, 2, 12, 68.0, 3500000);

INSERT INTO TIENICH VALUES
(1, N'WiFi', N'Truyen tai toc do cao mien phi'),
(2, N'TV', N'Tivi man hinh phang 50 inch'),
(3, N'May lanh', N'Dieu hoa 2 chieu'),
(4, N'Tu lanh', N'Tu lanh mini trong phong'),
(5, N'Ban lam viec', N'Co den doc sach va o cam'),
(6, N'Bep nho', N'Danh cho phong gia dinh'),
(7, N'Binh nong lanh', N'Nuoc nong 24/24'),
(8, N'Ban cong', N'Huong bien hoac thanh pho'),
(9, N'May say toc', N'May say cong suat cao'),
(10, N'Keo rem tu dong', N'Tu dong dieu chinh anh sang');

INSERT INTO PHONG_TIENICH VALUES
(1,1),(1,2),(1,3),
(2,1),(2,2),(2,4),
(3,1),(3,3),(3,5),
(4,1),(4,6),(4,8),
(5,1),(5,7),(5,8),
(6,1),(6,2),(6,3),
(7,1),(7,9),(7,10),
(8,1),(8,2),(8,3),
(9,1),(9,5),(9,10),
(10,1),(10,4),(10,6);


INSERT INTO NHANVIEN
(MaNV, HoTen, NgaySinh, SoDT, ChucVu, TenDN, MatKhau, Email)
VALUES
(1,  N'Dang Hoang Hai',     '2005-01-02', N'0374232805', N'Quan ly',          'danghoanghai',    'danghoanghai',  '2324802010209@student.tdmu.edu.vn'),
(2,  N'Tran Thi Bich',     '1992-03-22', N'0912345678', N'Quan ly',         'tranthibich2',     'nv2@123',  'bich@hotel.com'),
(3,  N'Dang Hoang Hai 2',      '1988-07-15', N'0987654321', N'Le tan',          'danghoanghai2',      'danghoanghai',  'hai@hotel.com'),
(4,  N'Pham Minh Duc',     '1995-11-05', N'0934567890', N'Le tan',         'phamminhduc4',     'nv4@123',  'duc@hotel.com'),
(5,  N'Hoang Thi Ha',      '1993-12-18', N'0923456789', N'Nhan vien',          'hoangthiha5',      'nv5@123',  'ha@hotel.com'),
(6,  N'Nguyen Khanh Nam',  '1986-09-20', N'0919876543', N'Quan ly',         'nguyenkhanhnam6',  'nv6@123',  'nam@hotel.com'),
(7,  N'Vo Quang Huy',      '1991-02-28', N'0909988776', N'Le tan',          'voquanghuy7',      'nv7@123',  'huy@hotel.com'),
(8,  N'Pham Thi Oanh',     '1994-06-16', N'0944455566', N'Le tan',         'phamthioanh8',     'nv8@123',  'oanh@hotel.com'),
(9,  N'Tran Van Hoang',    '1989-04-12', N'0933221100', N'Nhan vien',        'tranvanhoang9',    'nv9@123',  'hoang@hotel.com'),
(10, N'Le Thuy Tien',      '1996-08-25', N'0977665544', N'Nhan vien',        'lethuytien10',     'nv10@123', 'tien@hotel.com');


INSERT INTO KHACHHANG
(MKH, TKH, DiaChi, SDT, CMND_CCCD, TenDN, MatKhau, Email)
VALUES
(1,  N'Dang Hoang Hai',    N'TP Ho Chi Minh',          N'0374232805', N'074205001098', 'danghoanghai',    'danghoanghai',  '23248020102099@gmail.com'),
(2,  N'Tran Van Hung',     N'TP Ho Chi Minh',  N'0912123456', N'0234567890', 'hungphan',      '20082005',  'midorima8726@@gmail.com'),
(3,  N'Le Thi Thu',        N'Da Nang',         N'0921122334', N'0345678901', 'lethithu3',         'kh3@123',  'thu@gmail.com'),
(4,  N'Hoang Van Nam',     N'Hai Phong',       N'0939988776', N'0456789012', 'hoangvannam4',      'kh4@123',  'nam@gmail.com'),
(5,  N'Pham Thi Huong',    N'Can Tho',         N'0946655443', N'0567890123', 'phamthihuong5',     'kh5@123',  'huong@gmail.com'),
(6,  N'Vo Minh Quan',      N'Nha Trang',       N'0903344556', N'0678901234', 'vominhquan6',       'kh6@123',  'quan@gmail.com'),
(7,  N'Ngo Thanh Hien',    N'Binh Duong',      N'0914455667', N'0789012345', 'ngothanhhien7',     'kh7@123',  'hien@gmail.com'),
(8,  N'Bui Thanh Long',    N'Vung Tau',        N'0925566778', N'0890123456', 'buithanhlong8',     'kh8@123',  'long@gmail.com'),
(9,  N'Pham Minh Tuan',    N'Hue',             N'0936677889', N'0901234567', 'phamminhtuan9',     'kh9@123',  'tuan@gmail.com'),
(10, N'Le Hoai Phuong',    N'Tay Ninh',        N'0947788990', N'0912345678', 'lehoaiphuong10',    'kh10@123', 'phuong@gmail.com');

INSERT INTO DICHVU VALUES
(1, N'Giat ui', 50000),
(2, N'Dua don san bay', 200000),
(3, N'Spa', 300000),
(4, N'Massage', 250000),
(5, N'Bua sang', 150000),
(6, N'An toi', 300000),
(7, N'Thuê xe may', 200000),
(8, N'Ho boi', 100000),
(9, N'Bar', 250000),
(10, N'Dat tour', 400000);

INSERT INTO THUEPHONG VALUES
(1, 1, 1, '2025-10-01', '2025-10-02', '2025-10-05', 500000, 'DP001', N'Da tra'),
(2, 2, 2, '2025-10-03', '2025-10-04', '2025-10-06', 300000, 'DP002', N'Dang o'),
(3, 3, 3, '2025-10-05', '2025-10-06', '2025-10-09', 400000, 'DP003', N'Da tra'),
(4, 4, 4, '2025-10-07', '2025-10-08', '2025-10-10', 350000, 'DP004', N'Huy'),
(5, 5, 5, '2025-10-09', '2025-10-10', '2025-10-13', 500000, 'DP005', N'Dang o'),
(6, 6, 6, '2025-10-10', '2025-10-11', '2025-10-14', 400000, 'DP006', N'Da tra'),
(7, 7, 7, '2025-10-11', '2025-10-12', '2025-10-15', 600000, 'DP007', N'Dang o'),
(8, 8, 8, '2025-10-13', '2025-10-14', '2025-10-16', 200000, 'DP008', N'Da tra'),
(9, 9, 9, '2025-10-14', '2025-10-15', '2025-10-17', 350000, 'DP009', N'Dang o'),
(10, 10, 10, '2025-10-15', '2025-10-16', '2025-10-20', 700000, 'DP010', N'Da tra');

INSERT INTO CTTHUEPHONG VALUES
(1,1,N'Nguoi dat'),
(2,2,N'Nguoi o'),
(3,3,N'Nguoi dat'),
(4,4,N'Nguoi o'),
(5,5,N'Nguoi dat'),
(6,6,N'Nguoi o'),
(7,7,N'Nguoi dat'),
(8,8,N'Nguoi o'),
(9,9,N'Nguoi dat'),
(10,10,N'Nguoi o');


INSERT INTO TRANGTHAI_PHONG VALUES
(1,1,N'Trong','2025-10-01',1,N'Phong san sang'),
(2,2,N'Co khach','2025-10-04',2,N'Khach vua nhan phong'),
(3,3,N'Dang don dep','2025-10-06',3,N'Chuan bi don'),
(4,4,N'Hu hong','2025-10-08',4,N'Dieu hoa loi'),
(5,5,N'Trong','2025-10-09',5,N'Phong da duoc sua'),
(6,6,N'Co khach','2025-10-10',6,N'Dang o'),
(7,7,N'Trong','2025-10-12',7,N'Phong san sang'),
(8,8,N'Dang don dep','2025-10-14',8,N'Ve sinh dinh ky'),
(9,9,N'Co khach','2025-10-15',9,N'Dang o'),
(10,10,N'Trong','2025-10-16',10,N'San sang su dung');

INSERT INTO SDDICHVU VALUES
(1,1,2),(2,2,1),(3,3,1),
(4,4,2),(5,5,3),(6,6,1),
(7,7,2),(8,8,1),(9,9,3),(10,10,1);


INSERT INTO THANHTOAN VALUES
(1,1,N'Tien mat',3600000,'2025-10-05'),
(2,2,N'The tin dung',4500000,'2025-10-06'),
(3,3,N'Chuyen khoan',6000000,'2025-10-09'),
(4,4,N'Tien mat',0,'2025-10-10'),
(5,5,N'The tin dung',5400000,'2025-10-13'),
(6,6,N'Tien mat',6600000,'2025-10-14'),
(7,7,N'Chuyen khoan',9000000,'2025-10-15'),
(8,8,N'Tien mat',3300000,'2025-10-16'),
(9,9,N'The tin dung',7200000,'2025-10-17'),
(10,10,N'Tien mat',12000000,'2025-10-20');

INSERT INTO HINHANH VALUES
(1,1,1,N'p01.jpg',N'Phong don sang trong','2025-10-01'),
(2,2,2,N'p02.jpg',N'Phong doi tien nghi','2025-10-02'),
(3,3,3,N'p03.jpg',N'Phong suite cao cap','2025-10-03'),
(4,4,4,N'p04.jpg',N'Phong VIP view bien','2025-10-04'),
(5,5,5,N'p05.jpg',N'Phong family rong rai','2025-10-05'),
(6,6,6,N'p06.jpg',N'Phong deluxe sang trong','2025-10-06'),
(7,7,7,N'p07.jpg',N'Phong executive hien dai','2025-10-07'),
(8,8,8,N'p08.jpg',N'Phong standard dep','2025-10-08'),
(9,9,9,N'p09.jpg',N'Phong tong thong','2025-10-09'),
(10,10,10,N'p10.jpg',N'Can ho penthouse','2025-10-10'),
(11,11,8,N'p11.jpg',N'Phong cao cap tang 5','2025-10-11'),
(12,12,3,N'p12.jpg',N'Phong don tang 6','2025-10-12'),
(13,13,3,N'p13.jpg',N'Phong doi hien dai','2025-10-13'),
(14,14,2,N'p14.jpg',N'Phong sang trong tang 7','2025-10-14'),
(15,15,2,N'p15.jpg',N'Phong suite dep','2025-10-15'),
(16,16,1,N'p16.jpg',N'Phong mini de thuong','2025-10-16'),
(17,17,1,N'p17.jpg',N'Phong view thanh pho','2025-10-17'),
(18,18,4,N'p18.jpg',N'Phong deluxe view bien','2025-10-18'),
(19,19,4,N'p19.jpg',N'Phong premium rong rai','2025-10-19'),
(20,20,5,N'p20.jpg',N'Phong thuong gia','2025-10-20'),
(21,21,6,N'p21.jpg',N'Phong mau xanh sang','2025-10-21'),
(22,22,7,N'p22.jpg',N'Phong tong thong mini','2025-10-22');

INSERT INTO BINHLUAN VALUES
(1,1,1,1,N'Phong sach dep, nhan vien nhiet tinh','2025-10-06'),
(2,1,1,2,N'Vi tri thuan loi, an sang ngon','2025-10-07'),
(3,2,2,3,N'Phong rong, view dep','2025-10-08'),
(4,3,3,4,N'Dich vu chu dao, gia hop ly','2025-10-09'),
(5,4,4,5,N'Gia phong tot, se quay lai','2025-10-10'),
(6,5,5,6,N'Tuyet voi voi ky nghi cua toi','2025-10-11'),
(7,6,6,7,N'Rong rai, sach se','2025-10-12'),
(8,7,7,8,N'Khach san rat dep, nhan vien than thien','2025-10-13'),
(9,8,8,9,N'Dich vu tot, khong gian yen tinh','2025-10-14'),
(10,9,9,10,N'Tuyet voi, se gioi thieu ban be','2025-10-15');



-- ========== VIEWS ==========
-- Xóa view n?u ?ă t?n t?i
IF OBJECT_ID('NV_PHONG_3','V') IS NOT NULL DROP VIEW NV_PHONG_3;
GO
CREATE VIEW NV_PHONG_3 AS
SELECT DISTINCT NV.MaNV, NV.HoTen, NV.NgaySinh, NV.SoDT
FROM NHANVIEN NV
JOIN THUEPHONG TP ON NV.MaNV = TP.MaNV
WHERE TP.MaPhong = 3;
GO

IF OBJECT_ID('TEN_KH_PHONG4','V') IS NOT NULL DROP VIEW TEN_KH_PHONG4;
GO
CREATE VIEW TEN_KH_PHONG4 AS
SELECT DISTINCT KH.MKH, KH.TKH, KH.DiaChi, KH.SDT
FROM KHACHHANG KH
JOIN CTTHUEPHONG CT ON KH.MKH = CT.KHACH
JOIN THUEPHONG TP ON CT.MaThue = TP.MaThue
WHERE TP.MaPhong = 4;
GO

IF OBJECT_ID('SO_PHONG_NV','V') IS NOT NULL DROP VIEW SO_PHONG_NV;
GO
CREATE VIEW SO_PHONG_NV AS
SELECT NV.MaNV, NV.HoTen, COUNT(TP.MaPhong) AS SoPhongChoThue
FROM NHANVIEN NV
LEFT JOIN THUEPHONG TP ON NV.MaNV = TP.MaNV
GROUP BY NV.MaNV, NV.HoTen;
GO

IF OBJECT_ID('NV_THUE_VIP','V') IS NOT NULL DROP VIEW NV_THUE_VIP;
GO
CREATE VIEW NV_THUE_VIP AS
SELECT DISTINCT NV.MaNV, NV.HoTen, NV.NgaySinh, NV.SoDT
FROM NHANVIEN NV
JOIN THUEPHONG TP ON NV.MaNV = TP.MaNV
JOIN PHONG P ON TP.MaPhong = P.MaPhong
JOIN LOAIPHONG L ON P.MaLoai = L.MaLoai
WHERE L.TenLoai = N'VIP';
GO
-- ========== STORED PROCEDURES ==========
-- 1) Pḥng tr?ng theo mă lo?i
IF OBJECT_ID('PHONG_TRONG','P') IS NOT NULL DROP PROC PHONG_TRONG;
GO
CREATE PROC PHONG_TRONG @ML INT
AS
BEGIN
    SELECT MaPhong, TenPhong, MaLoai, SucChua, Tang, DGNgay
    FROM PHONG
    WHERE MaLoai = @ML
      AND MaPhong NOT IN (SELECT MaPhong FROM THUEPHONG WHERE TrangThai <> N'?ă h?y' OR TrangThai IS NULL);
END;
GO

-- 2) Danh sách d?ch v? và s? l??t theo khách hàng
IF OBJECT_ID('DS_DICHVU_SOLUOT','P') IS NOT NULL DROP PROC DS_DICHVU_SOLUOT;
GO
CREATE PROC DS_DICHVU_SOLUOT @mkh INT
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM KHACHHANG WHERE MKH = @mkh)
    BEGIN
        PRINT N'Mă khách hàng không t?n t?i';
        RETURN;
    END

    SELECT DV.MaDV, DV.TenDV, DV.DGDV, SUM(SD.SoLuot) AS SoLuot
    FROM DICHVU DV
    JOIN SDDICHVU SD ON DV.MaDV = SD.DV
    JOIN CTTHUEPHONG CT ON SD.MaThue = CT.MaThue
    WHERE CT.KHACH = @mkh
    GROUP BY DV.MaDV, DV.TenDV, DV.DGDV;
END;
GO

-- 3) Thêm/ghi nh?n thanh toán cho mă thuê (t?o m?i b?n ghi THANHTOAN)
IF OBJECT_ID('THEM_THANH_TOAN','P') IS NOT NULL DROP PROC THEM_THANH_TOAN;
GO
CREATE PROC THEM_THANH_TOAN @mt INT, @httt NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM THUEPHONG WHERE MaThue = @mt)
    BEGIN
        PRINT N'Không th? thanh toán khi mă thuê không t?n t?i';
        RETURN;
    END

    DECLARE @mtt INT = (SELECT ISNULL(MAX(MaTT),0) + 1 FROM THANHTOAN);
    DECLARE @tt MONEY = 0;

    SELECT @tt = ISNULL(P.DGNgay * DATEDIFF(DAY, T.NgayVao, T.NgayTra),0)
                + ISNULL(SUM(DV.DGDV * SD.SoLuot),0)
                - ISNULL(T.DatCoc,0)
    FROM THUEPHONG T
    JOIN PHONG P ON T.MaPhong = P.MaPhong
    LEFT JOIN SDDICHVU SD ON T.MaThue = SD.MaThue
    LEFT JOIN DICHVU DV ON SD.DV = DV.MaDV
    WHERE T.MaThue = @mt
    GROUP BY P.DGNgay, T.NgayVao, T.NgayTra, T.DatCoc;

    -- N?u ch?a có b?n ghi trong THANHTOAN cho MaThue này th́ insert, ng??c l?i update
    IF NOT EXISTS (SELECT 1 FROM THANHTOAN WHERE MaThue = @mt)
    BEGIN
        INSERT INTO THANHTOAN (MaTT, MaThue, HinhThucTT, ThanhTien, NgayTT)
        VALUES (@mtt, @mt, @httt, @tt, GETDATE());
    END
    ELSE
    BEGIN
        UPDATE THANHTOAN SET HinhThucTT = @httt, ThanhTien = @tt, NgayTT = GETDATE()
        WHERE MaThue = @mt;
    END

    PRINT N'Ghi nh?n thanh toán hoàn t?t.';
END;
GO
-- ========== FUNCTIONS ==========
-- 1) Tr? v? ??a ch? khách thuê theo mă pḥng (n?u nhi?u khách, tr? ??a ch? khách ??u tiên)
IF OBJECT_ID('DIACHI_KH_THUE','FN') IS NOT NULL DROP FUNCTION DIACHI_KH_THUE;
GO
CREATE FUNCTION DIACHI_KH_THUE (@mp INT)
RETURNS NVARCHAR(200)
AS
BEGIN
    DECLARE @dc NVARCHAR(200);
    SELECT TOP 1 @dc = KH.DiaChi
    FROM KHACHHANG KH
    JOIN CTTHUEPHONG CT ON KH.MKH = CT.KHACH
    JOIN THUEPHONG TP ON CT.MaThue = TP.MaThue
    WHERE TP.MaPhong = @mp;
    RETURN @dc;
END;
GO

-- 2) Tr? v? t?ng thành ti?n c?a khách hàng
IF OBJECT_ID('THANHTIEN','FN') IS NOT NULL DROP FUNCTION THANHTIEN;
GO
CREATE FUNCTION THANHTIEN (@mkh INT)
RETURNS MONEY
AS
BEGIN
    DECLARE @tt MONEY;
    SELECT @tt = SUM(ISNULL(TT.ThanhTien,0))
    FROM THANHTOAN TT
    JOIN CTTHUEPHONG CT ON TT.MaThue = CT.MaThue
    WHERE CT.KHACH = @mkh;
    RETURN ISNULL(@tt,0);
END;
GO

-- 3) Hàm tr? v? b?ng các b?n thanh toán ch?a thanh toán (ví d? tr? h?t các b?n ghi THANHTOAN cho khách)
IF OBJECT_ID('DS_KH_CHUA_THANH_TOAN','TF') IS NOT NULL DROP FUNCTION DS_KH_CHUA_THANH_TOAN;
GO
CREATE FUNCTION DS_KH_CHUA_THANH_TOAN (@mkh INT)
RETURNS TABLE
AS
RETURN (
    SELECT TT.MaTT, TT.MaThue, TT.HinhThucTT, TT.ThanhTien
    FROM THANHTOAN TT
    JOIN CTTHUEPHONG CT ON TT.MaThue = CT.MaThue
    WHERE CT.KHACH = @mkh
);
GO

-- 4) Hàm b?ng tr? tên khách hàng, mă thuê, thành ti?n theo mă khách
IF OBJECT_ID('BangNew','TF') IS NOT NULL DROP FUNCTION BangNew;
GO
CREATE FUNCTION BangNew(@mkh INT)
RETURNS @BangMoi TABLE (TenKH NVARCHAR(100), MaThue INT, ThanhTien MONEY)
AS
BEGIN
    INSERT INTO @BangMoi
    SELECT KH.TKH, CT.MaThue, TT.ThanhTien
    FROM KHACHHANG KH
    JOIN CTTHUEPHONG CT ON KH.MKH = CT.KHACH
    JOIN THANHTOAN TT ON CT.MaThue = TT.MaThue
    WHERE KH.MKH = @mkh;
    RETURN;
END;
GO
-- ========== TRIGGERS ==========
-- a) Trigger kiểm tra cập nhật SucChua trên PHONG
IF OBJECT_ID('CAPNHAT_SUCCHUA','TR') IS NOT NULL
    DROP TRIGGER CAPNHAT_SUCCHUA;
GO
CREATE TRIGGER CAPNHAT_SUCCHUA
ON PHONG
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1
        FROM INSERTED I
        JOIN THUEPHONG T ON I.MaPhong = T.MaPhong
        JOIN CTTHUEPHONG CT ON T.MaThue = CT.MaThue
        GROUP BY I.MaPhong, I.SucChua
        HAVING COUNT(CT.KHACH) > MAX(I.SucChua)
    )
    BEGIN
        PRINT N'Sức chứa nhỏ hơn số khách hiện tại — rollback!';
        ROLLBACK TRANSACTION;
    END
END;
GO

-- b) Trigger kiểm tra tính đúng đắn của THANHTIEN khi INSERT/UPDATE
-- Xóa trigger nếu tồn tại
IF OBJECT_ID('KT_THANHTIEN','TR') IS NOT NULL
    DROP TRIGGER KT_THANHTIEN;
GO

-- Tạo trigger
CREATE TRIGGER KT_THANHTIEN
ON THANHTOAN
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Kiểm tra từng bản ghi trong INSERTED
    IF EXISTS (
        SELECT 1
        FROM INSERTED I
        JOIN THUEPHONG T ON I.MaThue = T.MaThue
        JOIN PHONG P ON T.MaPhong = P.MaPhong
        LEFT JOIN SDDICHVU SD ON T.MaThue = SD.MaThue
        LEFT JOIN DICHVU DV ON SD.DV = DV.MaDV
        GROUP BY I.MaThue, I.ThanhTien, P.DGNgay, T.NgayVao, T.NgayTra, T.DatCoc
        HAVING I.ThanhTien <> (P.DGNgay * DATEDIFF(DAY, T.NgayVao, T.NgayTra) + ISNULL(SUM(DV.DGDV * SD.SoLuot),0) - ISNULL(T.DatCoc,0))
    )
    BEGIN
        PRINT N'Thành tiền không hợp lệ — rollback!';
        ROLLBACK TRANSACTION;
    END
END;
GO

-- c) Thủ tục thêm CTTHUEPHONG kèm kiểm tra giao dịch & sức chứa
IF OBJECT_ID('ND_THEM_CTTHUEPHONG','P') IS NOT NULL DROP PROC ND_THEM_CTTHUEPHONG;
GO

CREATE PROC ND_THEM_CTTHUEPHONG 
    @mt INT, 
    @mkh INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        
        -- Kiểm tra mã thuê tồn tại
        IF NOT EXISTS (SELECT 1 FROM THUEPHONG WHERE MaThue = @mt)
            THROW 51000, N'Mã thuê không tồn tại!', 1;

        DECLARE @mp INT;
        SELECT @mp = MaPhong FROM THUEPHONG WHERE MaThue = @mt;

        -- Kiểm tra khách đã tồn tại
        IF EXISTS (SELECT 1 FROM CTTHUEPHONG WHERE MaThue = @mt AND KHACH = @mkh)
            THROW 51002, N'Khách này đã có trong đơn thuê!', 1;

        -- Đếm khách trong cùng mã thuê (đúng logic)
        DECLARE @soKh INT;
        SELECT @soKh = COUNT(*) FROM CTTHUEPHONG WHERE MaThue = @mt;

        -- Sức chứa
        DECLARE @sucChua INT;
        SELECT @sucChua = SucChua FROM PHONG WHERE MaPhong = @mp;

        IF (@soKh + 1) > ISNULL(@sucChua,0)
            THROW 51001, N'Sức chứa phòng không đủ để thêm khách!', 1;

        -- Thêm khách
        INSERT INTO CTTHUEPHONG (MaThue, KHACH, VaiTro)
        VALUES (@mt, @mkh, N'Khách');

        COMMIT TRANSACTION;
        PRINT N'Thêm chi tiết thuê phòng thành công';

    END TRY
BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH

END;
GO
