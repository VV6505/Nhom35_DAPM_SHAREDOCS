DROP DATABASE IF EXISTS HeThongChiaSeTaiLieu_V1;
CREATE DATABASE HeThongChiaSeTaiLieu_V1;
GO
USE HeThongChiaSeTaiLieu_V1;
GO

-----------------------------------------------------------
-- 1. TẠO CẤU TRÚC BẢNG (Giữ nguyên cấu trúc của bạn)
-----------------------------------------------------------
CREATE TABLE Khoa (
    MaKhoa CHAR(5) PRIMARY KEY,
    TenKhoa NVARCHAR(100) NOT NULL
);
go
CREATE TABLE Nganh (
    MaNganh CHAR(5) PRIMARY KEY,
    TenNganh NVARCHAR(100) NOT NULL,
    MaKhoa CHAR(5) FOREIGN KEY REFERENCES Khoa(MaKhoa)
);
go
CREATE TABLE Lop (
    MaLop CHAR(5) PRIMARY KEY,
    TenLop NVARCHAR(100) NOT NULL,
    MaKhoa CHAR(5) FOREIGN KEY REFERENCES Khoa(MaKhoa)
);
go
CREATE TABLE MonHoc (
    MaMonHoc CHAR(5) PRIMARY KEY,
    TenMonHoc NVARCHAR(100) NOT NULL,
    MaNganh CHAR(5) FOREIGN KEY REFERENCES Nganh(MaNganh)
);
go
CREATE TABLE VaiTro (
    MaVaiTro CHAR(5) PRIMARY KEY,
    TenVaiTro NVARCHAR(50) NOT NULL
);
go
CREATE TABLE TaiKhoan (
    MaTK CHAR(5) PRIMARY KEY,
    TenTK NVARCHAR(100) NOT NULL,
    MatKhau VARCHAR(255) NOT NULL,
    MaVaiTro CHAR(5) FOREIGN KEY REFERENCES VaiTro(MaVaiTro),
    TrangThai NVARCHAR(100)
);
go
CREATE TABLE GiangVien (
    MaGV CHAR(5) PRIMARY KEY,
    TenGV NVARCHAR(100) NOT NULL,
    GioiTinh NVARCHAR(10),
    NgaySinh DATE,
    Email VARCHAR(100),
    SDT VARCHAR(10),
    HocVi NVARCHAR(100),
    MaKhoa CHAR(5) FOREIGN KEY REFERENCES Khoa(MaKhoa),
    MaTK CHAR(5) FOREIGN KEY REFERENCES TaiKhoan(MaTK)
);
go
CREATE TABLE SinhVien (
    MaSV CHAR(5) PRIMARY KEY,
    TenSV NVARCHAR(100) NOT NULL,
    Email VARCHAR(100),
    NgaySinh DATE,
    GioiTinh NVARCHAR(10),
    DiemTichLuy INT DEFAULT 0,
    MaLop CHAR(5) FOREIGN KEY REFERENCES Lop(MaLop),
    TrangThaiSV NVARCHAR(100),
    MaTK CHAR(5) FOREIGN KEY REFERENCES TaiKhoan(MaTK)
);
go
CREATE TABLE HocKy (
    MaHK CHAR(5) PRIMARY KEY,
    TenHK NVARCHAR(50),
    NamHoc VARCHAR(20),
    NgayBD DATE,
    NgayKT DATE
);
go
CREATE TABLE LichSuDiem (
    MaLS CHAR(5) PRIMARY KEY,
    MaSV CHAR(5) FOREIGN KEY REFERENCES SinhVien(MaSV),
    SoDiemThayDoi INT,
    LyDo NVARCHAR(100),
    NgayThayDoi DATE,
    MaHK CHAR(5) FOREIGN KEY REFERENCES HocKy(MaHK)
);
go
CREATE TABLE DoQuy (
    MaDQ CHAR(5) PRIMARY KEY,
    MucDoQuy INT,
    DiemTL INT
);
go
CREATE TABLE LoaiTaiLieu (
    MaLTL CHAR(5) PRIMARY KEY,
    TenLTL NVARCHAR(100),
	MaDQ CHAR(5) FOREIGN KEY REFERENCES DoQuy(MaDQ)
);
go
CREATE TABLE TaiLieu (
    MaTaiLieu CHAR(5) PRIMARY KEY,
    TieuDe NVARCHAR(255) NOT NULL,
    MoTa NVARCHAR(MAX),
    DuongDanFile VARCHAR(500),
    LoaiFile VARCHAR(10),
    KichThuoc FLOAT,
    LuotTai INT DEFAULT 0,
    CheDoHienThi BIT,
    trangThaiDuyet NVARCHAR(20),
    LyDoTuChoi NVARCHAR(255),
    maMonHoc CHAR(5) FOREIGN KEY REFERENCES MonHoc(MaMonHoc),
    maNguoiDang CHAR(5), 
    maBaoCao CHAR(5),
    ngayDang DATETIME DEFAULT GETDATE(),
    lanTaiBan INT DEFAULT 1,
    NXB NVARCHAR(100),
    namXB DATE,
    maloaiTL CHAR(5) FOREIGN KEY REFERENCES LoaiTaiLieu(MaLTL),
    diemYeuCau INT,
    maNguoiDuyetKhoa CHAR(5)
);
go
CREATE TABLE ThongBao (
    MaTB CHAR(5) PRIMARY KEY,
    MaNguoiNhan CHAR(5),
    TieuDe NVARCHAR(100),
    NoiDung NVARCHAR(500),
    LoaiThongBao NVARCHAR(100),
    TrangThai NVARCHAR(50),
    NgayTao DATETIME DEFAULT GETDATE(),
    MaTL CHAR(5) FOREIGN KEY REFERENCES TaiLieu(MaTaiLieu)
);
go
CREATE TABLE DanhGia (
    MaDG CHAR(5) PRIMARY KEY,
    MaTL CHAR(5) FOREIGN KEY REFERENCES TaiLieu(MaTaiLieu),
    MaND CHAR(5),
    SoSaoDG INT CHECK (SoSaoDG BETWEEN 1 AND 5),
    ThoiGian DATETIME DEFAULT GETDATE()
);
go
CREATE TABLE BinhLuan (
    MaBL CHAR(5) PRIMARY KEY,
    MaTL CHAR(5) FOREIGN KEY REFERENCES TaiLieu(MaTaiLieu),
    MaND CHAR(5),
    NoiDung NVARCHAR(200),
    ThoiGian DATETIME DEFAULT GETDATE()
);
go
CREATE TABLE TLYeuThich (
    MaTLL CHAR(5) PRIMARY KEY,
    MaTL CHAR(5) FOREIGN KEY REFERENCES TaiLieu(MaTaiLieu),
    MaND CHAR(5),
    ThoiGian DATETIME DEFAULT GETDATE()
);
go
CREATE TABLE LichSuTaiXuong (
    MaDownTL INT PRIMARY KEY IDENTITY(1,1),
    MaTaiLieu CHAR(5) FOREIGN KEY REFERENCES TaiLieu(MaTaiLieu),
    NgayTai DATETIME DEFAULT GETDATE(),
    MaND CHAR(5)
);
go
CREATE TABLE BaoCaoViPham (
    MaBaoCao CHAR(5) PRIMARY KEY,
    MaTaiLieu CHAR(5) FOREIGN KEY REFERENCES TaiLieu(MaTaiLieu),
    NguoiBaoCao CHAR(5),
    LyDo NVARCHAR(255),
    MoTaChiTiet NVARCHAR(MAX),
    TrangThaiXuLy NVARCHAR(20),
    NgayBaoCao DATETIME DEFAULT GETDATE(),
    NgayDuyet DATETIME
);
GO

-----------------------------------------------------------
-- TRIGGER TỰ ĐỘNG GỬI THÔNG BÁO KHI DUYỆT/TỪ CHỐI TÀI LIỆU
-----------------------------------------------------------
DROP TRIGGER IF EXISTS trg_ThongBao_TaiLieu;
GO

CREATE TRIGGER trg_ThongBao_TaiLieu
ON TaiLieu
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @MaxMaTB INT;
    
    -- Lấy mã thông báo lớn nhất hiện tại
    SELECT @MaxMaTB = ISNULL(MAX(CAST(SUBSTRING(MaTB, 3, 3) AS INT)), 0) FROM ThongBao;
    
    -- Xử lý khi tài liệu được DUYỆT (chuyển sang trạng thái "Đã duyệt")
    INSERT INTO ThongBao (MaTB, MaNguoiNhan, TieuDe, NoiDung, LoaiThongBao, TrangThai, NgayTao, MaTL)
    SELECT 
        'TB' + RIGHT('000' + CAST(@MaxMaTB + ROW_NUMBER() OVER (ORDER BY i.MaTaiLieu) AS VARCHAR(3)), 3),
        i.MaNguoiDang,
        N'Tài liệu đã được duyệt',
        N'Chúc mừng! Tài liệu "' + i.TieuDe + N'" của bạn đã được duyệt và hiển thị công khai trên hệ thống.',
        N'Duyệt tài liệu',
        N'Chưa đọc',
        GETDATE(),
        i.MaTaiLieu
    FROM inserted i
    LEFT JOIN deleted d ON i.MaTaiLieu = d.MaTaiLieu
    WHERE i.TrangThaiDuyet = N'Đã duyệt' 
        AND (d.MaTaiLieu IS NULL OR d.TrangThaiDuyet != N'Đã duyệt');
    
    -- Cập nhật lại @MaxMaTB sau khi insert thông báo duyệt
    SELECT @MaxMaTB = ISNULL(MAX(CAST(SUBSTRING(MaTB, 3, 3) AS INT)), 0) FROM ThongBao;
    
    -- Xử lý khi tài liệu bị TỪ CHỐI
    INSERT INTO ThongBao (MaTB, MaNguoiNhan, TieuDe, NoiDung, LoaiThongBao, TrangThai, NgayTao, MaTL)
    SELECT 
        'TB' + RIGHT('000' + CAST(@MaxMaTB + ROW_NUMBER() OVER (ORDER BY i.MaTaiLieu) AS VARCHAR(3)), 3),
        i.MaNguoiDang,
        N'Tài liệu đã bị từ chối',
        N'Tài liệu "' + i.TieuDe + N'" của bạn đã bị từ chối. Lý do: ' + ISNULL(i.LyDoTuChoi, N'Không có lý do cụ thể.'),
        N'Từ chối tài liệu',
        N'Chưa đọc',
        GETDATE(),
        i.MaTaiLieu
    FROM inserted i
    LEFT JOIN deleted d ON i.MaTaiLieu = d.MaTaiLieu
    WHERE i.TrangThaiDuyet = N'Từ chối' 
        AND (d.MaTaiLieu IS NULL OR d.TrangThaiDuyet != N'Từ chối');
    
    -- Cập nhật lại @MaxMaTB sau khi insert thông báo từ chối
    SELECT @MaxMaTB = ISNULL(MAX(CAST(SUBSTRING(MaTB, 3, 3) AS INT)), 0) FROM ThongBao;
    
    -- Xử lý khi tài liệu được ĐĂNG TẢI THÀNH CÔNG (INSERT mới)
    INSERT INTO ThongBao (MaTB, MaNguoiNhan, TieuDe, NoiDung, LoaiThongBao, TrangThai, NgayTao, MaTL)
    SELECT 
        'TB' + RIGHT('000' + CAST(@MaxMaTB + ROW_NUMBER() OVER (ORDER BY i.MaTaiLieu) AS VARCHAR(3)), 3),
        i.MaNguoiDang,
        N'Đăng tải tài liệu thành công',
        N'Tài liệu "' + i.TieuDe + N'" đã được đăng tải thành công. Tài liệu đang chờ duyệt từ cán bộ khoa.',
        N'Đăng tải tài liệu',
        N'Chưa đọc',
        GETDATE(),
        i.MaTaiLieu
    FROM inserted i
    LEFT JOIN deleted d ON i.MaTaiLieu = d.MaTaiLieu
    WHERE d.MaTaiLieu IS NULL; -- Chỉ khi INSERT (không có bản ghi cũ)
END;
GO

-----------------------------------------------------------
-- TRIGGER TỰ ĐỘNG CỘNG ĐIỂM KHI TÀI LIỆU ĐƯỢC DUYỆT
-----------------------------------------------------------
DROP TRIGGER IF EXISTS trg_CongDiem_TaiLieuDuyet;
GO

CREATE TRIGGER trg_CongDiem_TaiLieuDuyet
ON TaiLieu
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Chỉ xử lý khi tài liệu chuyển sang trạng thái "Đã duyệt"
    IF EXISTS (
        SELECT 1 
        FROM inserted i
        INNER JOIN deleted d ON i.MaTaiLieu = d.MaTaiLieu
        WHERE i.TrangThaiDuyet = N'Đã duyệt' 
            AND d.TrangThaiDuyet != N'Đã duyệt'
    )
    BEGIN
        -- Cộng điểm cho sinh viên (tính tổng điểm nếu có nhiều tài liệu)
        UPDATE sv
        SET sv.DiemTichLuy = sv.DiemTichLuy + ISNULL(TongDiem.TongDiemCong, 0)
        FROM SinhVien sv
        INNER JOIN (
            SELECT 
                tk.MaSV,
                SUM(dq.DiemTL) AS TongDiemCong
            FROM inserted i
            INNER JOIN deleted d ON i.MaTaiLieu = d.MaTaiLieu
            INNER JOIN TaiKhoan tk ON i.MaNguoiDang = tk.MaTK
            INNER JOIN SinhVien sv2 ON tk.MaSV = sv2.MaSV
            INNER JOIN LoaiTaiLieu ltl ON i.MaLoaiTL = ltl.MaLTL
            INNER JOIN DoQuy dq ON ltl.MaDQ = dq.MaDQ
            WHERE i.TrangThaiDuyet = N'Đã duyệt' 
                AND d.TrangThaiDuyet != N'Đã duyệt'
            GROUP BY tk.MaSV
        ) AS TongDiem ON sv.MaSV = TongDiem.MaSV;
        
        -- Ghi lại lịch sử thay đổi điểm (từng tài liệu một dòng)
        INSERT INTO LichSuDiem (MaSV, SoDiemThayDoi, LyDo, NgayThayDoi, MaHK)
        SELECT 
            tk.MaSV,
            dq.DiemTL,
            N'Đăng tải tài liệu "' + i.TieuDe + N'" được duyệt',
            GETDATE(),
            (SELECT TOP 1 MaHK FROM HocKy WHERE GETDATE() BETWEEN NgayBD AND NgayKT ORDER BY NgayBD DESC)
        FROM inserted i
        INNER JOIN deleted d ON i.MaTaiLieu = d.MaTaiLieu
        INNER JOIN TaiKhoan tk ON i.MaNguoiDang = tk.MaTK
        INNER JOIN SinhVien sv ON tk.MaSV = sv.MaSV
        INNER JOIN LoaiTaiLieu ltl ON i.MaLoaiTL = ltl.MaLTL
        INNER JOIN DoQuy dq ON ltl.MaDQ = dq.MaDQ
        WHERE i.TrangThaiDuyet = N'Đã duyệt' 
            AND d.TrangThaiDuyet != N'Đã duyệt';
    END;
END;
GO

-----------------------------------------------------------
-- 2. CHÈN DỮ LIỆU DANH MỤC
-----------------------------------------------------------
INSERT INTO Khoa (MaKhoa, TenKhoa) VALUES  
('CNS', N'Khoa Công nghệ số'),
('SPCN', N'Khoa Sư phạm Công nghiệp'),
('DDT', N'Khoa Điện - Điện tử'),
('KTXD', N'Khoa Kỹ thuật Xây dựng'),
('CK', N'Khoa Cơ khí'),
('CHHMT', N'Khoa Công nghệ Hóa học - Môi trường');
GO

-- SỬA LỖI: Đổi 'KKT' thành mã khoa thực tế đã chèn ở trên (CK, DDT, KTXD...)
INSERT INTO Nganh (MaNganh, TenNganh, MaKhoa) VALUES  
('NG001', N'Công nghệ Kỹ thuật Cơ khí', 'CK'),
('NG002', N'Công nghệ Kỹ thuật Ô tô', 'CK'),
('NG003', N'Công nghệ Kỹ thuật Nhiệt', 'CK'),
('NG004', N'Công nghệ Kỹ thuật Cơ Điện tử', 'CK'),
('NG005', N'Công nghệ Kỹ thuật Điện - Điện tử', 'DDT'),
('NG006', N'Công nghệ Kỹ thuật Điện tử - Viễn thông', 'DDT'),
('NG007', N'Công nghệ Kỹ thuật Điều khiển và Tự động hóa', 'DDT'),
('NG008', N'Công nghệ Thông tin', 'CNS'),
('NG009', N'Công nghệ Kỹ thuật Xây dựng', 'KTXD'),
('NG010', N'Công nghệ Kỹ thuật Giao thông', 'KTXD'),
('NG011', N'Kỹ thuật Cơ sở Hạ tầng', 'KTXD'),
('NG012', N'Công nghệ Kỹ thuật Môi trường', 'CHHMT'),
('NG013', N'Kỹ thuật Sinh học Thực phẩm', 'CHHMT'),
('NG014', N'Kỹ thuật Thực phẩm', 'CHHMT'),
('NG015', N'Sư phạm Kỹ thuật Công nghiệp', 'SPCN'),
('NG016', N'Công nghệ Kỹ thuật Kiến trúc', 'KTXD'),
('NG017', N'Công nghệ Vật liệu', 'CK'),
('NG018', N'Kỹ thuật Chất hóa học', 'CHHMT');
GO

INSERT INTO Lop (MaLop, TenLop, MaKhoa) VALUES  
('23T1', N'23T1', 'CNS'), ('23T2', N'23T2', 'CNS'), ('23T3', N'23T3', 'CNS'),
('23OT1', N'23OT1', 'CK'), ('23OT2', N'23OT2', 'CK'), ('23OT3', N'23OT3', 'CK'),
('23CK1', N'23CK1', 'CK'), ('23CK2', N'23CK2', 'CK'), ('23CK3', N'23CK3', 'CK'),
('23DT1', N'23DT1', 'DDT'), ('23DT2', N'23DT2', 'DDT'), ('23DT3', N'23DT3', 'DDT'),
('23DD1', N'23DD1', 'DDT'), ('23DD2', N'23DD2', 'DDT'), ('23DD3', N'23DD3', 'DDT'),
('23XD1', N'23XD1', 'KTXD'), ('23XD2', N'23XD2', 'KTXD'), ('23XD3', N'23XD3', 'KTXD'),
('23KT1', N'23KT1', 'KTXD'), ('23KT2', N'23KT2', 'KTXD'),
('23TP1', N'23TP1', 'CHHMT'), ('23TP2', N'23TP2', 'CHHMT'), ('23TP3', N'23TP3', 'CHHMT'),
('23MT1', N'23MT1', 'CHHMT'), ('23MT2', N'23MT2', 'CHHMT'),
('23SP1', N'23SP1', 'SPCN'), ('23SP2', N'23SP2', 'SPCN'), ('23SP3', N'23SP3', 'SPCN');
GO

INSERT INTO MonHoc (MaMonHoc, TenMonHoc, MaNganh) VALUES  
('5121', N'Cấu trúc dữ liệu & giải thuật', 'NG008'), ('5320', N'Chuyên đề ngôn ngữ lập trình', 'NG008'),
('5128', N'Cơ sở dữ liệu II', 'NG008'), ('5321', N'Công nghệ mạng không dây', 'NG008'),
('5132', N'Công nghệ phần mềm', 'NG008'), ('5135', N'Công nghệ XML', 'NG008'),
('5322', N'Đồ án Kiểm thử phần mềm', 'NG008'), ('5344', N'Đồ án phần mềm', 'NG008'),
('5323', N'Đồ án Tốt nghiệp CNTT', 'NG008'), ('2010', N'Đổi mới sáng tạo và khởi nghiệp', 'NG008'),
('5333', N'Học kỳ doanh nghiệp CNTT', 'NG008'), ('2009', N'Kỹ năng lãnh đạo, quản lý', 'NG008'),
('5346', N'Lập trình hướng đối tượng', 'NG008'), ('5169', N'Lập trình Java nâng cao', 'NG008'),
('5172', N'Lập trình trên ĐTDĐ', 'NG008'), ('5173', N'Lập trình trực quan', 'NG008'),
('5175', N'Lập trình web nâng cao', 'NG008'), ('5183', N'Ngoại ngữ chuyên ngành CNTT', 'NG008'),
('5188', N'Phân tích thiết kế hướng đối tượng', 'NG008'), ('5325', N'Quản trị dự án CNTT', 'NG008'),
('5192', N'Quản trị Mạng', 'NG008'), ('5195', N'TH Cơ sở dữ liệu II', 'NG008'),
('5198', N'TH CTDL & giải thuật', 'NG008'), ('5348', N'TH Lập trình HĐT', 'NG008'),
('5203', N'TH Lập trình Java nâng cao', 'NG008'), ('5206', N'TH Lập trình trên ĐTDĐ', 'NG008'),
('5208', N'TH Lập trình web nâng cao', 'NG008'), ('5326', N'Thị giác máy tính', 'NG008'),
('5226', N'Trí tuệ nhân tạo', 'NG008'), ('5212', N'TH Quản trị Mạng', 'NG008'),
('4003', N'Cắt gọt kim loại', 'NG001'), ('4009', N'Công nghệ CAD/CAM/CNC', 'NG001'),
('4011', N'Công nghệ Chế tạo Máy I', 'NG001'), ('4012', N'Công nghệ Chế tạo Máy II', 'NG001'),
('4013', N'Công nghệ tạo phôi', 'NG001'), ('4016', N'Đồ án Chi tiết Máy', 'NG001'),
('4017', N'Đồ án Công nghệ Chế tạo Máy', 'NG001'), ('4254', N'Học kỳ doanh nghiệp Chế tạo máy', 'NG001'),
('4030', N'Kỹ thuật An toàn Cơ khí', 'NG001'), ('4034', N'Máy cắt kim loại', 'NG001'),
('4039', N'Robot công nghiệp', 'NG001'), ('4045', N'Thiết bị nâng chuyển', 'NG001'),
('4255', N'Trang bị điện trong máy cơ khí', 'NG001'), ('4256', N'Thiết kế cơ khí 3D', 'NG001'),
('4084', N'Vật liệu kỹ thuật', 'NG001'), ('4302', N'Vẽ Cơ khí', 'NG001'),
('4270', N'Anh văn chuyên ngành', 'NG001'), ('4303', N'TN Vật liệu kỹ thuật', 'NG001'),
('5002', N'Bảo vệ rơle', 'NG005'), ('5238', N'Chuyên đề ngành HT CCĐ', 'NG005'),
('5006', N'Cung cấp điện', 'NG005'), ('5007', N'ĐA chống sét và tiếp địa', 'NG005'),
('5009', N'ĐA thiết kế mạng điện khu vực', 'NG005'), ('5011', N'Điện tử công suất', 'NG005'),
('5021', N'Đồ án thiết kế cấp điện', 'NG005'), ('5330', N'Học kỳ doanh nghiệp HTCCĐ', 'NG005'),
('5034', N'KT điện cao áp & vật liệu điện', 'NG005'), ('5036', N'Kỹ thuật chiếu sáng', 'NG005'),
('7130', N'Quá trình và thiết bị truyền chất', 'NG014'), ('7131', N'Quá trình và thiết bị truyền nhiệt', 'NG014'),
('7207', N'TN QT & TB truyền nhiệt', 'NG014'), ('7208', N'TN QT & TB truyền chất', 'NG014'),
('7204', N'Vi sinh', 'NG014'), ('7205', N'TN Vi sinh', 'NG014'),
('7176', N'TN Hoá sinh', 'NG014'), ('7104', N'Hóa học thực phẩm', 'NG014'),
('7071', N'Cơ sở thiết kế nhà máy', 'NG014'), ('7209', N'Quy hoạch thực nghiệm', 'NG014'),
('7079', N'Công nghệ sấy và lạnh', 'NG014'), ('7070', N'Cơ sở kỹ thuật thực phẩm', 'NG014');
GO

-----------------------------------------------------------
-- 3. CHÈN DỮ LIỆU NGƯỜI DÙNG
-----------------------------------------------------------
INSERT INTO VaiTro (MaVaiTro, TenVaiTro) VALUES  
('VT001', N'Quản trị viên'), ('VT002', N'Giảng viên'), ('VT003', N'Sinh viên');
GO

INSERT INTO TaiKhoan (MaTK, TenTK, MatKhau, MaVaiTro, TrangThai) VALUES  
('TK001', 'admin_quang', '123456', 'VT001', N'Đang hoạt động'),
('TK002', 'gv_dung', '123456', 'VT002', N'Đang hoạt động'),
('TK003', 'gv_nam', '123456', 'VT002', N'Đang hoạt động'),
('TK004', 'sv_oanh', '123456', 'VT003', N'Đang hoạt động'),
('TK005', 'sv_thai', '123456', 'VT003', N'Đang hoạt động'),
('TK006', 'sv_vu', '123456', 'VT003', N'Đang hoạt động'),
('TK007', 'sv_lan', '123456', 'VT003', N'Đang hoạt động'),
('TK008', 'sv_hung', '123456', 'VT003', N'Tạm khóa'),
('TK009', 'sv_mai', '123456', 'VT003', N'Đang hoạt động'),
('TK010', 'sv_khoa', '123456', 'VT003', N'Đang hoạt động'),
('TK011', 'sv_tu', '123456', 'VT003', N'Đang hoạt động'),
('TK012', 'sv_chau', '123456', 'VT003', N'Đang hoạt động'),
('TK013', 'sv_nam', '123456', 'VT003', N'Đang hoạt động');
GO

<<<<<<< Updated upstream:SQLQuery1.sql
INSERT INTO GiangVien (MaGV, TenGV, GioiTinh, NgaySinh, Email, SDT, HocVi, MaKhoa, MaTK) VALUES  
('GV001', N'Trần Bửu Dung', N'Nữ', '1985-05-20', 'dungtb@ute.udn.vn', '0905123456', N'Thạc sĩ', 'CNS', 'TK002'),
('GV002', N'Lê Văn Nam', N'Nam', '1980-10-12', 'namlv@ute.udn.vn', '0905654321', N'Tiến sĩ', 'CK', 'TK003');
GO

INSERT INTO SinhVien (MaSV, TenSV, Email, NgaySinh, GioiTinh, DiemTichLuy, MaLop, TrangThaiSV, MaTK) VALUES  
('SV001', N'Nguyễn Ngọc Kiều Oanh', 'oanhnnk@gmail.com', '2005-01-15', N'Nữ', 500, '23T1', N'Đang học', 'TK004'),
('SV002', N'Trần Gia Thái', 'thaitg@gmail.com', '2005-03-22', N'Nam', 300, '23CK1', N'Đang học', 'TK005'),
('SV003', N'Văn Vũ', 'vuv@gmail.com', '2005-06-10', N'Nam', 150, '23DT1', N'Đang học', 'TK006'),
('SV004', N'Lê Thị Lan', 'lanlt@gmail.com', '2005-09-05', N'Nữ', 420, '23T1', N'Đang học', 'TK007'),
('SV005', N'Phạm Tuấn Hùng', 'hungpt@gmail.com', '2005-11-30', N'Nam', 0, '23OT1', N'Tạm nghỉ', 'TK008'),
('SV006', N'Hoàng Thanh Mai', 'maiht@gmail.com', '2005-02-28', N'Nữ', 280, '23TP1', N'Đang học', 'TK009'),
('SV007', N'Đặng Anh Khoa', 'khoada@gmail.com', '2005-08-14', N'Nam', 100, '23XD1', N'Đang học', 'TK010'),
('SV008', N'Bùi Minh Tú', 'tubm@gmail.com', '2005-12-01', N'Nam', 600, '23T2', N'Đang học', 'TK011'),
('SV009', N'Ngô Bảo Châu', 'chaunb@gmail.com', '2005-04-18', N'Nữ', 350, '23DD1', N'Đang học', 'TK012'),
('SV010', N'Lý Hải Nam', 'namlh@gmail.com', '2005-07-25', N'Nam', 210, '23T3', N'Đang học', 'TK013');
=======
INSERT INTO SinhVien (MaSV, TenSV, Email, NgaySinh, GioiTinh, DiemTichLuy, MaLop, TrangThaiSV) VALUES  
('SV001', N'Nguyễn Ngọc Kiều Oanh', 'oanhnnk@gmail.com', '2005-01-15', N'Nữ', 0, '23T2', N'Đang học'),
('SV002', N'Trần Gia Thái', 'thaitg@gmail.com', '2005-03-22', N'Nam', 0, '22SK1', N'Đang học'),
('SV003', N'Phùng Văn Vũ', 'vuv@gmail.com', '2005-06-10', N'Nam', 0, '23T1', N'Đang học'),
('SV004', N'Lê Thị Lan', 'lanlt@gmail.com', '2005-09-05', N'Nữ', 0, '23T1', N'Đang học'),
('SV005', N'Phạm Tuấn Hùng', 'hungpt@gmail.com', '2005-11-30', N'Nam', 0, '23OT1', N'Tạm nghỉ'),
('SV006', N'Hoàng Thanh Mai', 'maiht@gmail.com', '2005-02-28', N'Nữ', 0, '23TP1', N'Đang học'),
('SV007', N'Đặng Anh Khoa', 'khoada@gmail.com', '2005-08-14', N'Nam', 0, '23XD1', N'Đang học'),
('SV008', N'Bùi Minh Tú', 'tubm@gmail.com', '2005-12-01', N'Nam', 0, '23T2', N'Đang học'),
('SV009', N'Ngô Bảo Châu', 'chaunb@gmail.com', '2005-04-18', N'Nữ', 0, '23DD1', N'Đang học'),
('SV010', N'Lý Hải Nam', 'namlh@gmail.com', '2005-07-25', N'Nam', 0, '23T3', N'Đang học'),
('SV011', N'Phạm Lê Thiệu Quang', 'quangplt@gmail.com', '2005-07-05', N'Nam', 0, '23T3', N'Đang học');
>>>>>>> Stashed changes:db_SHAREDOCS.sql
GO

INSERT INTO HocKy (MaHK, TenHK, NamHoc, NgayBD, NgayKT) VALUES  
('HK231', N'Học kỳ 1', '2023-2024', '2023-09-05', '2024-01-15'),
('HK232', N'Học kỳ 2', '2023-2024', '2024-02-10', '2024-06-30'),
('HK241', N'Học kỳ 1', '2024-2025', '2024-09-05', '2025-01-15'),
('HK242', N'Học kỳ 2', '2024-2025', '2025-02-10', '2025-06-30'),
('HK251', N'Học kỳ 1', '2025-2026', '2025-09-05', '2026-01-15');
GO
INSERT INTO DoQuy (MaDQ, MucDoQuy, DiemTL) VALUES  
('DQ001', 1, 5), 
('DQ002', 2, 10), 
('DQ003', 3, 20);
GO
INSERT INTO LoaiTaiLieu (MaLTL, TenLTL, MaDQ) VALUES  
('L0001', N'Giáo trình',       'DQ003'), -- Giáo trình thường giá trị nhất (20đ)
('L0002', N'Đề cương ôn tập', 'DQ002'), -- Đề cương mức trung bình (10đ)
('L0003', N'Bài tập lớn',     'DQ002'), -- Bài tập lớn mức trung bình (10đ)
('L0004', N'Slide bài giảng', 'DQ001'); -- Slide mức cơ bản (5đ)
GO

-----------------------------------------------------------
-- 4. CHÈN DỮ LIỆU TÀI LIỆU VÀ TƯƠNG TÁC
-----------------------------------------------------------
<<<<<<< Updated upstream:SQLQuery1.sql
INSERT INTO TaiLieu (MaTaiLieu, TieuDe, MoTa, DuongDanFile, LoaiFile, trangThaiDuyet, maMonHoc, maNguoiDang, ngayDang, lanTaiBan, NXB, maloaiTL, diemYeuCau) VALUES  
('TL001', N'Slide Java Swing UTE', N'Bài giảng GUI', 'java_swing.pdf', 'PDF', N'DaDuyet', '5169', 'GV001', GETDATE(), 1, N'NXB Giáo dục', 'L0004', 0),
('TL002', N'Đề cương SQL II', N'Tổng hợp kiến thức', 'sql_on_tap.docx', 'DOCX', N'DaDuyet', '5128', 'SV001', GETDATE(), 2, N'NXB Trẻ', 'L0002', 10),
('TL003', N'Báo cáo BTL Công nghệ phần mềm', N'Mẫu nhóm 23T1', 'btl_pm.pdf', 'PDF', N'ChoDuyet', '5132', 'SV004', GETDATE(), 1, NULL, 'L0003', 5),
('TL004', N'Giáo trình Cắt gọt kim loại', N'Sách điện tử', 'giao_trinh_ck.pdf', 'PDF', N'DaDuyet', '4003', 'GV002', GETDATE(), 3, N'NXB Kỹ thuật', 'L0001', 0),
('TL005', N'Slide ReactJS', N'Tài liệu thực hành', 'react_web.pdf', 'PDF', N'DaDuyet', '5175', 'SV001', GETDATE(), 1, NULL, 'L0004', 5),
('TL006', N'Đề cương Bảo vệ rơle', N'Câu hỏi trắc nghiệm', 'role_on_tap.pdf', 'PDF', N'ChoDuyet', '5002', 'SV003', GETDATE(), 1, NULL, 'L0002', 5),
('TL007', N'Giáo trình AI', N'Tài liệu dịch', 'ai_book.pdf', 'PDF', N'DaDuyet', '5226', 'SV007', GETDATE(), 2, N'NXB Khoa học', 'L0001', 0),
('TL008', N'BTL Cơ sở dữ liệu II', N'Thiết kế database', 'btl_db2.sql', 'SQL', N'DaDuyet', '5195', 'SV004', GETDATE(), 1, NULL, 'L0003', 0),
('TL009', N'Slide Truyền nhiệt', N'Ngành thực phẩm', 'nhiet_food.ppt', 'PPT', N'DaDuyet', '7131', 'GV001', GETDATE(), 1, NULL, 'L0004', 0),
('TL010', N'Đề cương Vẽ Cơ khí', N'Bài tập hình họa', 've_ck_on_tap.pdf', 'PDF', N'TuChoi', '4302', 'SV002', GETDATE(), 1, NULL, 'L0002', 0);
=======
-- Bước 1: INSERT tài liệu ở trạng thái "Chờ duyệt" (trigger sẽ tạo thông báo đăng tải)
INSERT INTO TaiLieu 
(MaTaiLieu, TieuDe, MoTa, DuongDanFile, LoaiFile, TrangThaiDuyet, MaMonHoc, MaNguoiDang, NgayDang, LanTaiBan, NXB, MaLoaiTL, DiemYeuCau) 
VALUES  
('TL001', N'Slide Java Swing UTE', N'Bài giảng GUI', '5169_Slide_JavaSwing_GV001_2025.pdf', 'PDF', N'Chờ duyệt', '5169', 'GV001', GETDATE(), 1, N'NXB Giáo dục', 'L0004', 0),
('TL002', N'Đề cương SQL II', N'Tổng hợp kiến thức', '5128_DeCuong_SQL2_SV001_2025.docx', 'DOCX', N'Chờ duyệt', '5128', 'SV001', GETDATE(), 2, N'NXB Trẻ', 'L0002', 10),
('TL003', N'Báo cáo BTL Công nghệ phần mềm', N'Mẫu nhóm 23T1', '5132_BTL_CongNghePM_SV004_2025.pdf', 'PDF', N'Chờ duyệt', '5132', 'SV004', GETDATE(), 1, NULL, 'L0003', 5),
('TL004', N'Giáo trình Cắt gọt kim loại', N'Sách điện tử', '4003_GiaoTrinh_CatGotKimLoai_GV002_2025.pdf', 'PDF', N'Chờ duyệt', '4003', 'GV002', GETDATE(), 3, N'NXB Kỹ thuật', 'L0001', 0),
('TL005', N'Slide ReactJS', N'Tài liệu thực hành', '5175_Slide_ReactJS_SV001_2025.pdf', 'PDF', N'Chờ duyệt', '5175', 'SV001', GETDATE(), 1, NULL, 'L0004', 5),
('TL006', N'Đề cương Bảo vệ rơle', N'Câu hỏi trắc nghiệm', '5002_DeCuong_BaoVeRole_SV003_2025.pdf', 'PDF', N'Chờ duyệt', '5002', 'SV003', GETDATE(), 1, NULL, 'L0002', 5),
('TL007', N'Giáo trình AI', N'Tài liệu dịch', '5226_GiaoTrinh_AI_SV007_2025.pdf', 'PDF', N'Chờ duyệt', '5226', 'SV007', GETDATE(), 2, N'NXB Khoa học', 'L0001', 0),
('TL008', N'BTL Cơ sở dữ liệu II', N'Thiết kế database', '5195_BTL_CSDL2_SV004_2025.sql', 'SQL', N'Chờ duyệt', '5195', 'SV004', GETDATE(), 1, NULL, 'L0003', 0),
('TL009', N'Slide Truyền nhiệt', N'Ngành thực phẩm', '7131_Slide_TruyenNhiet_GV001_2025.ppt', 'PPT', N'Chờ duyệt', '7131', 'GV001', GETDATE(), 1, NULL, 'L0004', 0),
('TL010', N'Đề cương Vẽ Cơ khí', N'Bài tập hình họa', '4302_DeCuong_VeCoKhi_SV002_2025.pdf', 'PDF', N'Chờ duyệt', '4302', 'SV002', GETDATE(), 1, NULL, 'L0002', 0);
>>>>>>> Stashed changes:db_SHAREDOCS.sql
GO

-- Bước 2: UPDATE để duyệt/từ chối tài liệu (trigger sẽ tạo thông báo + cộng điểm)
UPDATE TaiLieu SET TrangThaiDuyet = N'Đã duyệt' WHERE MaTaiLieu IN ('TL001', 'TL002', 'TL004', 'TL005', 'TL007', 'TL008', 'TL009');
UPDATE TaiLieu SET TrangThaiDuyet = N'Từ chối', LyDoTuChoi = N'Bản vẽ thiếu kích thước chi tiết' WHERE MaTaiLieu = 'TL010';
GO

<<<<<<< Updated upstream:SQLQuery1.sql
-- SỬA LỖI: SV01 -> SV001 cho khớp bảng SinhVien
=======
-- Không cần INSERT thủ công vào ThongBao vì trigger đã tự động tạo

>>>>>>> Stashed changes:db_SHAREDOCS.sql
INSERT INTO DanhGia (MaDG, MaTL, MaND, SoSaoDG) VALUES  
('DG001', 'TL001', 'SV001', 5),
('DG002', 'TL002', 'SV002', 4);
GO
<<<<<<< Updated upstream:SQLQuery1.sql
=======

>>>>>>> Stashed changes:db_SHAREDOCS.sql
INSERT INTO BinhLuan (MaBL, MaTL, MaND, NoiDung) VALUES  
('BL001', 'TL001', 'SV001', N'Tài liệu rất hay và chi tiết ạ!'),
('BL002', 'TL002', 'SV003', N'Đề thi này sát với thực tế ôn tập.');

GO
-----------------------------------------------------------
-- 5. CHÈN DỮ LIỆU BẢNG BAOCAOVIPHAM
-----------------------------------------------------------
INSERT INTO BaoCaoViPham (MaBaoCao, MaTaiLieu, NguoiBaoCao, LyDo, MoTaChiTiet, TrangThaiXuLy, NgayBaoCao, NgayDuyet) VALUES  
('BC001', 'TL001', 'SV002', N'Tài liệu sai kiến thức', N'Nội dung Slide Java Swing bị nhầm lẫn ở phần xử lý sự kiện JButton.', N'Đã xử lý', '2026-04-20 08:30:00', '2026-04-21 14:00:00'),
('BC002', 'TL002', 'SV003', N'Vi phạm bản quyền', N'Tài liệu này được sao chép nguyên văn từ giáo trình của một tác giả khác chưa xin phép.', N'Chờ xử lý', '2026-04-22 09:15:00', NULL),
('BC003', 'TL003', 'SV005', N'File bị lỗi', N'Không thể mở được file PDF, thông báo lỗi định dạng file không hợp lệ.', N'Chờ xử lý', '2026-04-23 10:00:00', NULL),
('BC004', 'TL004', 'SV001', N'Nội dung không phù hợp', N'Giáo trình có chứa một số hình ảnh không liên quan đến môn học Kỹ thuật Cơ khí.', N'Đã bác bỏ', '2026-04-24 11:45:00', '2026-04-25 09:00:00'),
('BC005', 'TL005', 'SV007', N'Tài liệu rác/Spam', N'File tải lên chỉ có trang trắng, không chứa thông tin thực hành ReactJS.', N'Đã xử lý', '2026-04-25 13:20:00', '2026-04-26 10:30:00'),
('BC006', 'TL007', 'SV006', N'Sai phân loại môn học', N'Tài liệu AI nhưng lại đang được gắn vào danh mục môn học khác.', N'Chờ xử lý', '2026-04-26 15:10:00', NULL),
('BC007', 'TL008', 'SV004', N'Mô tả sai thực tế', N'Mô tả là thiết kế Database hoàn chỉnh nhưng file chỉ có 2 bảng đơn giản.', N'Đã xử lý', '2026-04-27 08:00:00', '2026-04-28 16:45:00'),
('BC008', 'TL009', 'SV010', N'Ngôn ngữ không phù hợp', N'Tài liệu sử dụng ngôn từ không chuẩn mực sư phạm trong phần ghi chú Slide.', N'Chờ xử lý', '2026-04-28 14:30:00', NULL),
('BC009', 'TL010', 'SV001', N'Tài liệu trùng lặp', N'Tài liệu này đã tồn tại trên hệ thống với mã TL002.', N'Đã bác bỏ', '2026-04-29 09:00:00', '2026-04-29 17:00:00'),
('BC010', 'TL001', 'SV009', N'Link tải file bị hỏng', N'Khi nhấn tải xuống hệ thống báo lỗi không tìm thấy tệp tin trên máy chủ.', N'Chờ xử lý', '2026-04-30 10:20:00', NULL);
GO
<<<<<<< Updated upstream:SQLQuery1.sql
=======

>>>>>>> Stashed changes:db_SHAREDOCS.sql
