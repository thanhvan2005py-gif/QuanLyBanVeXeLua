CREATE DATABASE QLXeBusPhuongTrang_65134257
GO
USE QLXeBusPhuongTrang_65134257
GO
CREATE TABLE BenXe (
    MaBen INT IDENTITY(1,1) PRIMARY KEY,
    TenBen NVARCHAR(100) NOT NULL,
    DiaChi NVARCHAR(200)
);
CREATE TABLE Tuyen (
    MaTuyen INT IDENTITY(1,1) PRIMARY KEY,
    TenTuyen NVARCHAR(200) NOT NULL,
    BenDau INT NOT NULL,
    BenCuoi INT NOT NULL,
    QuangDuongKm INT,
	FOREIGN KEY (BenDau) REFERENCES BenXe(MaBen),
    FOREIGN KEY (BenCuoi) REFERENCES BenXe(MaBen)
);
CREATE TABLE Xe (
    MaXe INT IDENTITY(1,1) PRIMARY KEY,
    BienSo NVARCHAR(20) NOT NULL,
    SoCho INT NOT NULL,
    LoaiXe NVARCHAR(50),
    TrangThai NVARCHAR(50)
);
CREATE TABLE TaiXe (
    MaTaiXe INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    DienThoai VARCHAR(20),
    BangLai NVARCHAR(20)
);
CREATE TABLE LichChay (
    MaLichChay INT IDENTITY(1,1) PRIMARY KEY,
    MaTuyen INT NOT NULL,
    MaXe INT NOT NULL,
    MaTaiXe INT NOT NULL,
    GioKhoiHanh TIME NOT NULL,
    GioDen TIME NOT NULL,
    TrangThai NVARCHAR(50),
    FOREIGN KEY (MaTuyen) REFERENCES Tuyen(MaTuyen),
    FOREIGN KEY (MaXe) REFERENCES Xe(MaXe),
    FOREIGN KEY (MaTaiXe) REFERENCES TaiXe(MaTaiXe)
);
CREATE TABLE Chuyen (
    MaChuyen INT IDENTITY(1,1) PRIMARY KEY,
    MaLichChay INT NOT NULL,
    Ngay DATE NOT NULL,
    SoGheTrong INT NOT NULL,
	GiaVe DECIMAL(18, 0) NOT NULL, 
    FOREIGN KEY (MaLichChay) REFERENCES LichChay(MaLichChay)
);

CREATE TABLE KhachHang (
    MaKH INT IDENTITY(1,1) PRIMARY KEY,
    TenKH NVARCHAR(100) NOT NULL,        
    DienThoai VARCHAR(20) NOT NULL UNIQUE, 
    Email VARCHAR(100),           
    MatKhau VARCHAR(255) NOT NULL
);

CREATE TABLE Ve (
    MaVe INT IDENTITY(1,1) PRIMARY KEY,
    MaChuyen INT NOT NULL,
    SoGhe NVARCHAR(10) NOT NULL,
    MaKH INT NOT NULL,
    Gia DECIMAL(18,2) NOT NULL,
    ThoiGianDat DATETIME NOT NULL DEFAULT GETDATE(),
    TrangThai NVARCHAR(30),
    FOREIGN KEY (MaChuyen) REFERENCES Chuyen(MaChuyen),
    FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH)
);
CREATE TABLE ThanhToan (
    MaTT INT IDENTITY(1,1) PRIMARY KEY,
    MaVe INT NOT NULL,
    SoTien DECIMAL(18,2) NOT NULL,
    PhuongThuc NVARCHAR(50) NOT NULL,
    ThoiGianThanhToan DATETIME NOT NULL DEFAULT GETDATE(),
    TrangThai NVARCHAR(20) NOT NULL,
    FOREIGN KEY (MaVe) REFERENCES Ve(MaVe)
);
GO
CREATE TABLE TaiKhoan (
    MaTK INT IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap VARCHAR(50) NOT NULL UNIQUE,
    MatKhau VARCHAR(255) NOT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    DienThoai VARCHAR(20),
    TrangThai BIT DEFAULT 1,
    NgayTao DATETIME DEFAULT GETDATE()
);
GO
INSERT INTO BenXe VALUES (N'Bến Xe Nha Trang', N'Nha Trang, Khánh Hòa')
INSERT INTO BenXe VALUES (N'Bến Xe Miền Đông', N'Bình Thạnh, TP.HCM')
INSERT INTO BenXe VALUES (N'Bến Xe Miền Tây', N'Bình Tân, TP.HCM')
INSERT INTO BenXe VALUES (N'Bến Xe Phan Rang', N'Phan Rang, Ninh Thuận');
GO
INSERT INTO Tuyen VALUES (N'Nha Trang - TP.HCM', 1, 2, 1600)
INSERT INTO Tuyen VALUES (N'TP.HCM - Phan Rang', 2, 4, 350)
INSERT INTO Tuyen VALUES (N'Phan Rang - Nha Trang', 1, 2, 1600);
GO
INSERT INTO Xe VALUES (N'50A-12345', 45, N'Limousine', N'Hoạt động')
INSERT INTO Xe VALUES (N'50B-54321', 34, N'Giường nằm', N'Hoạt động')
INSERT INTO Xe VALUES (N'50C-67890', 16, N'Ghế ngồi', N'Hoạt động')
INSERT INTO Xe VALUES (N'50D-98765', 45, N'Limousine', N'Bảo trì');
GO
INSERT INTO TaiXe VALUES (N'Nguyễn Thành Văn', '0393412153', N'B2')
INSERT INTO TaiXe VALUES (N'Trần Trung Hưng', '0987654321', N'C1')
INSERT INTO TaiXe VALUES (N'Huỳnh Tấn Phát', '0123456789', N'C');
GO
INSERT INTO LichChay VALUES (1, 1, 1, '08:00:00', '20:00:00', N'Hoạt động')
INSERT INTO LichChay VALUES (1, 2, 2, '10:00:00', '22:00:00', N'Hoạt động')
INSERT INTO LichChay VALUES (2, 3, 1, '06:00:00', '14:00:00', N'Hoạt động');
GO
INSERT INTO Chuyen VALUES (1, '2025-11-20', 45,200000)
INSERT INTO Chuyen VALUES (1, '2025-11-21', 45,200000)
INSERT INTO Chuyen VALUES (3, '2025-11-20', 16,200000);
GO
INSERT INTO KhachHang VALUES (N'Nguyễn Đức Duy', '0900111222','duy.nguyen@gmail.com','123456')
INSERT INTO KhachHang VALUES (N'Trần Ngọc Vũ','0900333444','vu.tran@gmail.com','123456');
GO
INSERT INTO Ve VALUES (1, N'1A', 1, 400000, '2025-11-15 14:00:00', N'Đã thanh toán')
INSERT INTO Ve VALUES (1, N'1B', 2, 400000, '2025-11-16 09:00:00', N'Đã đặt')
INSERT INTO Ve VALUES (3, N'1C', 1, 200000, '2025-11-10 12:30:00', N'Đã thanh toán');
GO
INSERT INTO ThanhToan VALUES (1, 400000,N'Chuyển khoản', '2025-11-15 14:05:00', N'Hoàn tất')
INSERT INTO ThanhToan VALUES (3,200000, N'Tiền mặt', '2025-11-10 12:35:00', N'Hoàn tất');
GO

INSERT INTO TaiKhoan (TenDangNhap, MatKhau, HoTen, DienThoai, TrangThai) 
VALUES ('admin1', '123456', N'Nguyễn Văn Admin1', '0909123456', 1);
INSERT INTO TaiKhoan (TenDangNhap, MatKhau, HoTen, DienThoai, TrangThai) 
VALUES ('admin2', '123456', N'Nguyễn Văn Admin2', '0909654321', 1);
GO
