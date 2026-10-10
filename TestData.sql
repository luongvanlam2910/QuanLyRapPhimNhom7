/* =====================================================================
   TestData.sql — Dữ liệu mẫu để test/demo (chạy SAU khi đã có cấu trúc bảng).
   - Tương thích SQL Server 2008 R2.
   - Chỉ chạy khi bảng PHIM đang trống (an toàn khi chạy lại).
   - Chạy:  sqlcmd -S <server> -U <user> -P <pwd> -d datvexemphim -f 65001 -I -i TestData.sql
   Gồm: 10 thể loại, 24 phim, 3 rạp, 12 phòng (576 ghế), 3 phòng ban, 6 nhân viên,
        40 khách hàng, 5 khuyến mãi, ~670 suất chiếu (7 ngày trước → 7 ngày sau),
        ~200 đơn đặt vé kèm vé và thanh toán.
   ===================================================================== */
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF EXISTS (SELECT 1 FROM PHIM) BEGIN PRINT N'Đã có dữ liệu phim - bỏ qua.'; RETURN; END

BEGIN TRAN;

-- Bảng số 1..1000
DECLARE @N TABLE (n INT PRIMARY KEY);
INSERT @N SELECT TOP 1000 ROW_NUMBER() OVER (ORDER BY a.object_id) FROM sys.all_objects a CROSS JOIN sys.all_objects b;

-- ===== Thể loại =====
INSERT THE_LOAI (MaTheLoai, TenTheLoai) VALUES
 ('TL001',N'Hành động'),('TL002',N'Tình cảm'),('TL003',N'Hài hước'),('TL004',N'Kinh dị'),('TL005',N'Hoạt hình'),
 ('TL006',N'Khoa học viễn tưởng'),('TL007',N'Phiêu lưu'),('TL008',N'Tâm lý'),('TL009',N'Hình sự'),('TL010',N'Gia đình');

-- ===== Phim (24) =====
INSERT PHIM (MaPhim, TenPhim, ThoiLuong, DaoDien, DoTuoi, NgayKhoiChieu, TrangThai, MoTa)
SELECT 'PH' + RIGHT('000'+CAST(n AS VARCHAR(3)),3),
       N'Phim mẫu ' + CAST(n AS NVARCHAR(3)) + N': ' + CHOOSE_NAME,
       90 + (n*7) % 61,
       N'Đạo diễn ' + CHAR(64 + (n % 20) + 1),
       CASE n % 5 WHEN 0 THEN 'P' WHEN 1 THEN 'K' WHEN 2 THEN 'T13' WHEN 3 THEN 'T16' ELSE 'T18' END,
       CASE WHEN n <= 16 THEN DATEADD(day, -(n*9), CAST(GETDATE() AS DATE)) WHEN n <= 22 THEN DATEADD(day, n*5, CAST(GETDATE() AS DATE)) ELSE DATEADD(day, -200, CAST(GETDATE() AS DATE)) END,
       CASE WHEN n <= 16 THEN N'Đang chiếu' WHEN n <= 22 THEN N'Sắp chiếu' ELSE N'Ngừng chiếu' END,
       N'Nội dung mô tả của phim mẫu số ' + CAST(n AS NVARCHAR(3)) + N' dùng để kiểm thử hệ thống đặt vé.'
FROM (SELECT n, CASE n % 8 WHEN 0 THEN N'Bình minh' WHEN 1 THEN N'Hoàng hôn' WHEN 2 THEN N'Cuộc săn' WHEN 3 THEN N'Ký ức' WHEN 4 THEN N'Lời hứa' WHEN 5 THEN N'Bí mật' WHEN 6 THEN N'Hành trình' ELSE N'Giấc mơ' END AS CHOOSE_NAME FROM @N WHERE n <= 24) x;

INSERT PHIM_THE_LOAI (PhimsId, TheLoaisId)
SELECT p.Id, t.Id FROM PHIM p
JOIN (SELECT Id, ROW_NUMBER() OVER (ORDER BY Id) rn FROM THE_LOAI) t
  ON t.rn = (p.Id % 10) + 1 OR t.rn = ((p.Id * 3 + 1) % 10) + 1
GROUP BY p.Id, t.Id;

-- ===== Rạp / phòng / ghế =====
INSERT RAP_CHIEU (MaRap, TenRap, DiaChi, Sdt) VALUES
 ('R001',N'UTC Cinema Cầu Giấy',N'Số 3 Cầu Giấy, Đống Đa, Hà Nội','0241111001'),
 ('R002',N'UTC Cinema Long Biên',N'Số 12 Nguyễn Văn Cừ, Long Biên, Hà Nội','0241111002'),
 ('R003',N'UTC Cinema Hà Đông',N'Số 25 Quang Trung, Hà Đông, Hà Nội','0241111003');

INSERT PHONG_CHIEU (MaPhong, RapChieuId, TenPhong, LoaiPhong, SucChua)
SELECT 'P' + RIGHT('0000'+CAST(ROW_NUMBER() OVER (ORDER BY r.Id, k.n) AS VARCHAR(4)),4), r.Id,
       N'Phòng ' + CAST(k.n AS NVARCHAR(2)),
       CASE k.n WHEN 1 THEN '2D' WHEN 2 THEN '3D' WHEN 3 THEN 'IMAX' ELSE '2D' END, 48
FROM RAP_CHIEU r CROSS JOIN (SELECT n FROM @N WHERE n <= 4) k;

INSERT GHE (MaGhe, PhongChieuId, HangGhe, SoGhe, LoaiGhe, TrangThai)
SELECT 'G' + RIGHT('0000'+CAST(ROW_NUMBER() OVER (ORDER BY p.Id, h.n, s.n) AS VARCHAR(4)),4), p.Id,
       CHAR(64 + h.n), s.n,
       CASE WHEN h.n = 6 THEN N'Đôi' WHEN h.n IN (3,4) THEN N'VIP' ELSE N'Thường' END, N'Hoạt động'
FROM PHONG_CHIEU p CROSS JOIN (SELECT n FROM @N WHERE n <= 6) h CROSS JOIN (SELECT n FROM @N WHERE n <= 8) s;

-- ===== Phòng ban / nhân viên / khách hàng / khuyến mãi =====
INSERT PHONG_BAN (MaPb, TenPb, MoTa, TrangThai) VALUES
 ('PB001',N'Bán vé',N'Nhân viên bán vé tại quầy',N'Hoạt động'),
 ('PB002',N'Soát vé',N'Nhân viên soát vé tại cửa phòng chiếu',N'Hoạt động'),
 ('PB003',N'Quản lý',N'Ban quản lý rạp',N'Hoạt động');

INSERT NHAN_VIEN (MaNv, RapChieuId, PhongBanId, NgaySinh, Cccd, HoTen, ChucVu, Sdt, Email, TrangThai)
SELECT 'NV' + RIGHT('000'+CAST(n AS VARCHAR(3)),3),
       (SELECT Id FROM (SELECT Id, ROW_NUMBER() OVER (ORDER BY Id) rn FROM RAP_CHIEU) q WHERE q.rn = ((n-1) % 3) + 1),
       (SELECT Id FROM (SELECT Id, ROW_NUMBER() OVER (ORDER BY Id) rn FROM PHONG_BAN) q WHERE q.rn = CASE WHEN n <= 3 THEN 3 WHEN n % 2 = 0 THEN 2 ELSE 1 END),
       DATEADD(year, -(24 + n), '2000-06-15'), '0010990000' + CAST(n AS VARCHAR(2)),
       N'Nhân viên số ' + CAST(n AS NVARCHAR(2)), CASE WHEN n <= 3 THEN 'QuanLy' ELSE 'NhanVien' END,
       '09120000' + RIGHT('00'+CAST(n AS VARCHAR(2)),2), 'nv' + CAST(n AS VARCHAR(2)) + '@utccinema.vn', N'Hoạt động'
FROM @N WHERE n <= 6;

INSERT KHACH_HANG (MaKh, HoTen, Sdt, Email, NgaySinh, NgayDangKy, TrangThai)
SELECT 'KH' + RIGHT('000'+CAST(n AS VARCHAR(3)),3), N'Khách hàng ' + CAST(n AS NVARCHAR(3)),
       '09300000' + RIGHT('00'+CAST(n AS VARCHAR(3)),2) + CAST(n % 10 AS VARCHAR(1)) ,
       'kh' + CAST(n AS VARCHAR(3)) + '@mail.test',
       DATEADD(day, n*137, '1985-01-01'), DATEADD(day, -n*3, CAST(GETDATE() AS DATE)),
       CASE WHEN n % 20 = 0 THEN N'Đã khóa' ELSE N'Hoạt động' END
FROM @N WHERE n <= 40;

INSERT KHUYEN_MAI (MaKm, TenKm, LoaiGiam, GiaTri, NgayBatDau, NgayKetThuc) VALUES
 ('KMT01',N'Giảm 10% thành viên','PERCENT',10,DATEADD(day,-30,GETDATE()),DATEADD(day,60,GETDATE())),
 ('KMT02',N'Giảm 20% cuối tuần','PERCENT',20,DATEADD(day,-10,GETDATE()),DATEADD(day,30,GETDATE())),
 ('KMT03',N'Giảm 15.000đ mỗi đơn','AMOUNT',15000,DATEADD(day,-30,GETDATE()),DATEADD(day,90,GETDATE())),
 ('KMT04',N'Sinh viên giảm 25%','PERCENT',25,DATEADD(day,-60,GETDATE()),DATEADD(day,120,GETDATE())),
 ('KMT05',N'Khuyến mãi đã hết hạn','PERCENT',30,DATEADD(day,-90,GETDATE()),DATEADD(day,-30,GETDATE()));

-- ===== Lịch chiếu: 4 suất/phòng/ngày, 7 ngày trước → 7 ngày sau =====
DECLARE @today DATETIME; SET @today = CAST(CAST(GETDATE() AS DATE) AS DATETIME);
DECLARE @nPhim INT; SELECT @nPhim = COUNT(*) FROM PHIM WHERE TrangThai = N'Đang chiếu';

INSERT LICH_CHIEU (PhimId, PhongChieuId, ThoiGianBatDau, ThoiGianKetThuc, GiaVe, TrangThai)
SELECT ph.Id, pc.Id, s.start, DATEADD(minute, ph.ThoiLuong, s.start),
       70000 + (pc.rn % 3) * 15000 + CASE WHEN s.slot >= 3 THEN 20000 ELSE 0 END,
       CASE WHEN s.start < GETDATE() THEN N'Đã kết thúc' ELSE N'Mở bán' END
FROM (SELECT Id, ROW_NUMBER() OVER (ORDER BY Id) rn FROM PHONG_CHIEU) pc
CROSS JOIN (SELECT n - 8 AS d FROM @N WHERE n <= 15) dd
CROSS JOIN (SELECT n AS slot FROM @N WHERE n <= 4) sl
CROSS APPLY (SELECT DATEADD(minute, CASE sl.slot WHEN 1 THEN 600 WHEN 2 THEN 810 WHEN 3 THEN 1020 ELSE 1230 END,
                            DATEADD(day, dd.d, @today)) AS start, sl.slot AS slot) s
JOIN (SELECT Id, ThoiLuong, ROW_NUMBER() OVER (ORDER BY Id) rn FROM PHIM WHERE TrangThai = N'Đang chiếu') ph
  ON ph.rn = ((pc.rn + dd.d + 100 + sl.slot * 3) % @nPhim) + 1;

UPDATE LICH_CHIEU SET MaLich = 'LC' + RIGHT('00000'+CAST(Id AS VARCHAR(5)),5);

-- ===== Đơn đặt vé =====
IF OBJECT_ID('tempdb..#L') IS NOT NULL DROP TABLE #L;
SELECT ROW_NUMBER() OVER (ORDER BY Id) rn, Id, PhongChieuId, GiaVe, ThoiGianBatDau INTO #L FROM LICH_CHIEU;
DECLARE @nL INT; SELECT @nL = COUNT(*) FROM #L;

IF OBJECT_ID('tempdb..#O') IS NOT NULL DROP TABLE #O;
SELECT n, l.Id AS LichId, l.PhongChieuId, l.GiaVe, l.ThoiGianBatDau, (n % 3) + 1 AS nSeats,
       CASE WHEN n % 10 <= 6 THEN N'Đã thanh toán' WHEN n % 10 = 7 THEN N'Chờ thanh toán' ELSE N'Đã hủy' END AS TT,
       CASE WHEN n % 4 = 0 THEN NULL ELSE ((n % 40) + 1) END AS khIdx,
       CASE WHEN n % 9 = 0 THEN 1 ELSE 0 END AS coKm
INTO #O
FROM (SELECT n FROM @N WHERE n <= 200) o JOIN #L l ON l.rn = ((o.n * 3) % @nL) + 1;
-- đơn chờ thanh toán chỉ hợp lý với suất sắp chiếu
UPDATE #O SET TT = N'Đã thanh toán' WHERE TT = N'Chờ thanh toán' AND ThoiGianBatDau < GETDATE();

DECLARE @km1 INT; SELECT @km1 = Id FROM KHUYEN_MAI WHERE MaKm = 'KMT01';

INSERT DAT_VE (MaDat, KhachHangId, TenKhachVangLai, SdtVangLai, NhanVienId, KhuyenMaiId, NgayDat, TongTien, TrangThai, HanThanhToan)
SELECT 'DV' + RIGHT('000000'+CAST(o.n AS VARCHAR(6)),6),
       kh.Id,
       CASE WHEN o.khIdx IS NULL THEN N'Khách vãng lai ' + CAST(o.n AS NVARCHAR(3)) END,
       CASE WHEN o.khIdx IS NULL THEN '08800000' + RIGHT('00'+CAST(o.n AS VARCHAR(3)),2) END,
       CASE WHEN o.khIdx IS NULL THEN (SELECT TOP 1 Id FROM NHAN_VIEN WHERE MaNv = 'NV004') END,
       CASE WHEN o.coKm = 1 THEN @km1 END,
       DATEADD(hour, -(2 + o.n % 40), o.ThoiGianBatDau),
       CASE WHEN o.coKm = 1 THEN ROUND(o.nSeats * o.GiaVe * 0.9, 0) ELSE o.nSeats * o.GiaVe END,
       o.TT,
       CASE WHEN o.TT = N'Chờ thanh toán' THEN DATEADD(minute, 10, GETDATE()) END
FROM #O o
LEFT JOIN (SELECT Id, ROW_NUMBER() OVER (ORDER BY Id) rn FROM KHACH_HANG) kh ON kh.rn = o.khIdx;

IF OBJECT_ID('tempdb..#G') IS NOT NULL DROP TABLE #G;
SELECT Id, PhongChieuId, ROW_NUMBER() OVER (PARTITION BY PhongChieuId ORDER BY Id) AS gr INTO #G FROM GHE;

INSERT VE (MaVe, DatVeId, LichChieuId, GheId, GiaVe, TrangThai, ThoiGianSoatVe, NhanVienSoatVeId)
SELECT 'VE' + RIGHT('00000000'+CAST(ROW_NUMBER() OVER (ORDER BY o.n, k.n) AS VARCHAR(8)),8),
       d.Id, o.LichId, g.Id, o.GiaVe,
       CASE WHEN o.TT = N'Đã hủy' THEN N'Đã hủy'
            WHEN o.TT = N'Đã thanh toán' AND o.ThoiGianBatDau < GETDATE() THEN N'Đã sử dụng' ELSE N'Đã đặt' END,
       CASE WHEN o.TT = N'Đã thanh toán' AND o.ThoiGianBatDau < GETDATE() THEN DATEADD(minute, -10, o.ThoiGianBatDau) END,
       CASE WHEN o.TT = N'Đã thanh toán' AND o.ThoiGianBatDau < GETDATE() THEN (SELECT TOP 1 Id FROM NHAN_VIEN WHERE MaNv = 'NV005') END
FROM #O o
JOIN (SELECT n FROM @N WHERE n <= 3) k ON k.n <= o.nSeats
JOIN DAT_VE d ON d.MaDat = 'DV' + RIGHT('000000'+CAST(o.n AS VARCHAR(6)),6)
JOIN #G g ON g.PhongChieuId = o.PhongChieuId AND g.gr = ((o.n * 5 + k.n) % 48) + 1;

INSERT THANH_TOAN (MaTt, DatVeId, SoTien, NgayTt, PhuongThuc, TrangThai)
SELECT 'TT' + RIGHT('000000'+CAST(ROW_NUMBER() OVER (ORDER BY d.Id) AS VARCHAR(6)),6), d.Id, d.TongTien,
       DATEADD(minute, 3, d.NgayDat),
       CASE d.Id % 4 WHEN 0 THEN N'Tiền mặt' WHEN 1 THEN N'Chuyển khoản' WHEN 2 THEN N'Thẻ' ELSE N'Momo' END,
       CASE d.TrangThai WHEN N'Đã thanh toán' THEN N'Thành công' ELSE N'Hoàn tiền' END
FROM DAT_VE d WHERE d.TrangThai IN (N'Đã thanh toán', N'Đã hủy');

COMMIT;

SELECT 'THE_LOAI' t, COUNT(*) c FROM THE_LOAI UNION ALL SELECT 'PHIM', COUNT(*) FROM PHIM UNION ALL SELECT 'RAP_CHIEU', COUNT(*) FROM RAP_CHIEU
UNION ALL SELECT 'PHONG_CHIEU', COUNT(*) FROM PHONG_CHIEU UNION ALL SELECT 'GHE', COUNT(*) FROM GHE UNION ALL SELECT 'NHAN_VIEN', COUNT(*) FROM NHAN_VIEN
UNION ALL SELECT 'KHACH_HANG', COUNT(*) FROM KHACH_HANG UNION ALL SELECT 'KHUYEN_MAI', COUNT(*) FROM KHUYEN_MAI UNION ALL SELECT 'LICH_CHIEU', COUNT(*) FROM LICH_CHIEU
UNION ALL SELECT 'DAT_VE', COUNT(*) FROM DAT_VE UNION ALL SELECT 'VE', COUNT(*) FROM VE UNION ALL SELECT 'THANH_TOAN', COUNT(*) FROM THANH_TOAN;
GO
/* ===== Đặt tên thật cho dữ liệu mẫu (cập nhật theo mã, chạy lại an toàn) ===== */
SET NOCOUNT ON;
-- Nhân viên
DECLARE @E TABLE (ma VARCHAR(10), ten NVARCHAR(100));
INSERT @E VALUES ('NV001',N'Nguyễn Thị Hương'),('NV002',N'Trần Văn Minh'),('NV003',N'Lê Thị Thu Hà'),
                 ('NV004',N'Phạm Quốc Bảo'),('NV005',N'Hoàng Thị Mai'),('NV006',N'Vũ Đình Khoa');
UPDATE v SET HoTen = e.ten FROM NHAN_VIEN v JOIN @E e ON e.ma = v.MaNv;

-- Khách hàng: ghép họ + đệm + tên theo giới (lẻ = nam, chẵn = nữ)
DECLARE @Ho TABLE (i INT, v NVARCHAR(20));
INSERT @Ho VALUES (0,N'Nguyễn'),(1,N'Trần'),(2,N'Lê'),(3,N'Phạm'),(4,N'Hoàng'),(5,N'Vũ'),(6,N'Đặng'),(7,N'Bùi'),(8,N'Đỗ'),(9,N'Ngô');
DECLARE @DemN TABLE (i INT, v NVARCHAR(20)); INSERT @DemN VALUES (0,N'Văn'),(1,N'Minh'),(2,N'Quốc'),(3,N'Đức'),(4,N'Hoàng');
DECLARE @TenN TABLE (i INT, v NVARCHAR(20)); INSERT @TenN VALUES (0,N'Nam'),(1,N'Hùng'),(2,N'Dũng'),(3,N'Long'),(4,N'Khoa'),(5,N'Tuấn'),(6,N'Phong'),(7,N'Bảo'),(8,N'Huy'),(9,N'Sơn');
DECLARE @DemF TABLE (i INT, v NVARCHAR(20)); INSERT @DemF VALUES (0,N'Thị'),(1,N'Ngọc'),(2,N'Thu'),(3,N'Mai'),(4,N'Thanh');
DECLARE @TenF TABLE (i INT, v NVARCHAR(20)); INSERT @TenF VALUES (0,N'Lan'),(1,N'Hoa'),(2,N'Linh'),(3,N'Trang'),(4,N'Hương'),(5,N'Nga'),(6,N'Phương'),(7,N'Thảo'),(8,N'Yến'),(9,N'Vy');

UPDATE k SET HoTen =
  (SELECT v FROM @Ho WHERE i = (c.n * 3) % 10) + N' ' +
  CASE WHEN c.n % 2 = 1 THEN (SELECT v FROM @DemN WHERE i = (c.n * 2) % 5) + N' ' + (SELECT v FROM @TenN WHERE i = (c.n * 7) % 10)
       ELSE (SELECT v FROM @DemF WHERE i = (c.n * 2) % 5) + N' ' + (SELECT v FROM @TenF WHERE i = (c.n * 7) % 10) END
FROM KHACH_HANG k
JOIN (SELECT Id, CAST(SUBSTRING(MaKh, 3, 10) AS INT) n FROM KHACH_HANG WHERE MaKh LIKE 'KH[0-9][0-9][0-9]') c ON c.Id = k.Id;

-- Khách vãng lai ở đơn bán tại quầy
UPDATE DAT_VE SET TenKhachVangLai =
  (SELECT v FROM @Ho WHERE i = (Id * 3) % 10) + N' ' + (SELECT v FROM @DemN WHERE i = Id % 5) + N' ' + (SELECT v FROM @TenN WHERE i = (Id * 7) % 10)
WHERE TenKhachVangLai LIKE N'Khách vãng lai%';

SELECT TOP 5 MaPhim, TenPhim, DaoDien FROM PHIM ORDER BY Id;
SELECT TOP 5 MaKh, HoTen FROM KHACH_HANG ORDER BY Id;
SELECT MaNv, HoTen, ChucVu FROM NHAN_VIEN ORDER BY Id;
GO
/* ===== Phim THẬT + poster thật (ảnh nằm ở wwwroot/images/posters, lấy từ Wikipedia) ===== */
SET NOCOUNT ON;
DECLARE @R TABLE (n INT, ten NVARCHAR(100), dd NVARCHAR(100), phut INT, tuoi VARCHAR(5), g1 INT, g2 INT, mt NVARCHAR(600), anh VARCHAR(40));
INSERT @R VALUES
(1,N'Mai',N'Trấn Thành',131,'T18',1,7,N'Mai là người phụ nữ làm nghề mát-xa, sống lặng lẽ với quá khứ nhiều tổn thương. Mối tình bất ngờ với chàng trai kém tuổi mở ra cơ hội đổi thay nhưng cũng kéo theo những ràng buộc của gia đình và định kiến.','poster-ph001.jpg'),
(2,N'Bố Già',N'Trấn Thành, Vũ Ngọc Đãng',128,'T13',9,2,N'Câu chuyện về ông Ba Sang, người cha lao động nghèo, và cậu con trai Quắn cùng những xung đột, tình cảm gia đình đầy nước mắt xen lẫn tiếng cười.','poster-ph002.jpg'),
(3,N'Hai Phượng',N'Lê Văn Kiệt',98,'T18',0,8,N'Hai Phượng, người mẹ từng là tay đấm cự phách, dấn thân vào hành trình giải cứu con gái bị bắt cóc bởi đường dây buôn người.','poster-ph003.jpg'),
(4,N'Inception',N'Christopher Nolan',148,'T13',5,0,N'Dom Cobb, tên trộm chuyên đánh cắp bí mật trong giấc mơ, nhận nhiệm vụ cấy một ý tưởng vào tâm trí mục tiêu.','poster-ph004.jpg'),
(5,N'The Dark Knight',N'Christopher Nolan',152,'T16',0,8,N'Batman cùng Harvey Dent và Jim Gordon đối đầu Joker, tên tội phạm hỗn loạn đẩy thành phố Gotham vào bờ vực.','poster-ph005.jpg'),
(6,N'The Lion King',N'Roger Allers, Rob Minkoff',88,'P',4,9,N'Chú sư tử con Simba rời bỏ vương quốc sau cái chết của vua cha Mufasa, rồi phải trở về giành lại ngôi vương.','poster-ph006.jpg'),
(7,N'Frozen',N'Chris Buck, Jennifer Lee',102,'P',4,9,N'Công chúa Anna lên đường tìm chị gái Elsa, nữ hoàng có phép thuật băng giá, để cứu vương quốc khỏi mùa đông vĩnh cửu.','poster-ph007.jpg'),
(8,N'Toy Story',N'John Lasseter',81,'P',4,2,N'Cao bồi Woody và phi hành gia Buzz Lightyear từ đối thủ trở thành bạn bè trong cuộc phiêu lưu của những món đồ chơi.','poster-ph008.jpg'),
(9,N'Spirited Away',N'Hayao Miyazaki',125,'K',4,6,N'Cô bé Chihiro lạc vào thế giới linh hồn và phải làm việc trong nhà tắm của phù thủy Yubaba để cứu cha mẹ.','poster-ph009.png'),
(10,N'Coco',N'Lee Unkrich',105,'P',4,9,N'Cậu bé Miguel đam mê âm nhạc bước vào Xứ sở của người chết để khám phá bí mật của gia đình mình.','poster-ph010.jpg'),
(11,N'Parasite',N'Bong Joon-ho',132,'T18',7,8,N'Gia đình nghèo của Ki-taek từng người một len vào làm việc cho gia đình giàu có nhà Park, kéo theo những biến cố khó lường.','poster-ph011.png'),
(12,N'Joker',N'Todd Phillips',122,'T18',7,8,N'Arthur Fleck, nghệ sĩ hài thất bại bị xã hội ruồng bỏ, dần biến thành tên tội phạm Joker.','poster-ph012.jpg'),
(13,N'The Conjuring',N'James Wan',112,'T18',3,7,N'Vợ chồng điều tra siêu nhiên Ed và Lorraine Warren giúp một gia đình đối mặt với thế lực ma quái trong ngôi nhà ở Rhode Island.','poster-ph013.jpg'),
(14,N'Jurassic Park',N'Steven Spielberg',127,'T13',6,5,N'Công viên giải trí khủng long được tạo ra bằng công nghệ nhân bản gặp sự cố, du khách phải tìm cách sống sót.','poster-ph014.jpg'),
(15,N'La La Land',N'Damien Chazelle',128,'T13',1,7,N'Nhạc công jazz Sebastian và cô diễn viên trẻ Mia theo đuổi ước mơ ở Los Angeles, và tình yêu của họ phải đối mặt với những lựa chọn.','poster-ph015.png'),
(16,N'Titanic',N'James Cameron',195,'T13',1,7,N'Chuyện tình giữa Jack và Rose trên con tàu Titanic định mệnh trong chuyến hải trình đầu tiên.','poster-ph016.png'),
(17,N'Interstellar',N'Christopher Nolan',169,'T13',5,7,N'Nhóm phi hành gia du hành qua lỗ sâu để tìm ngôi nhà mới cho nhân loại khi Trái Đất cạn kiệt tài nguyên.','poster-ph017.jpg'),
(18,N'Avatar',N'James Cameron',162,'T13',5,6,N'Cựu lính thủy Jake Sully nhập vào cơ thể người Na''vi trên hành tinh Pandora và đứng giữa hai thế giới.','poster-ph018.jpg'),
(19,N'Avengers: Endgame',N'Anthony Russo, Joe Russo',181,'T13',0,5,N'Các siêu anh hùng còn lại của Avengers hợp sức đảo ngược hậu quả cú búng tay của Thanos.','poster-ph019.jpg'),
(20,N'Dune',N'Denis Villeneuve',155,'T13',5,6,N'Paul Atreides cùng gia tộc đến hành tinh sa mạc Arrakis, nơi sản sinh hương liệu quý nhất vũ trụ, và bị cuốn vào âm mưu quyền lực.','poster-ph020.jpg'),
(21,N'Top Gun: Maverick',N'Joseph Kosinski',130,'T13',0,7,N'Phi công Maverick trở lại huấn luyện thế hệ phi công mới cho một nhiệm vụ cực kỳ nguy hiểm.','poster-ph021.jpg'),
(22,N'The Shawshank Redemption',N'Frank Darabont',142,'T16',7,8,N'Chủ ngân hàng Andy Dufresne bị kết án oan, tìm thấy tình bạn và hy vọng trong nhà tù Shawshank.','poster-ph022.jpg'),
(23,N'The Godfather',N'Francis Ford Coppola',175,'T18',8,7,N'Gia tộc mafia Corleone và quá trình Michael từ người ngoài cuộc trở thành người kế vị.','poster-ph023.jpg'),
(24,N'Forrest Gump',N'Robert Zemeckis',142,'T13',7,1,N'Cuộc đời kỳ lạ của chàng trai chậm hiểu nhưng tốt bụng Forrest Gump đi qua nhiều sự kiện lớn của nước Mỹ.','poster-ph024.jpg');
UPDATE p SET TenPhim = r.ten, DaoDien = r.dd, ThoiLuong = r.phut, DoTuoi = r.tuoi, MoTa = r.mt, AnhPoster = r.anh
FROM PHIM p JOIN @R r ON p.MaPhim = 'PH' + RIGHT('000' + CAST(r.n AS VARCHAR(3)), 3);

DELETE FROM PHIM_THE_LOAI WHERE PhimsId IN (SELECT Id FROM PHIM WHERE MaPhim LIKE 'PH[0-9][0-9][0-9]');
INSERT PHIM_THE_LOAI (PhimsId, TheLoaisId)
SELECT p.Id, t.Id FROM @R r
JOIN PHIM p ON p.MaPhim = 'PH' + RIGHT('000' + CAST(r.n AS VARCHAR(3)), 3)
JOIN THE_LOAI t ON t.MaTheLoai = 'TL' + RIGHT('000' + CAST(r.g1 + 1 AS VARCHAR(3)), 3)
UNION
SELECT p.Id, t.Id FROM @R r
JOIN PHIM p ON p.MaPhim = 'PH' + RIGHT('000' + CAST(r.n AS VARCHAR(3)), 3)
JOIN THE_LOAI t ON t.MaTheLoai = 'TL' + RIGHT('000' + CAST(r.g2 + 1 AS VARCHAR(3)), 3);
SELECT COUNT(*) AS phim_co_poster FROM PHIM WHERE AnhPoster IS NOT NULL;
SELECT COUNT(*) AS lien_ket_the_loai FROM PHIM_THE_LOAI;
GO
/* ===== Tài khoản đăng nhập cho nhân viên mẫu (để demo bán vé tại quầy / phân quyền) =====
   Mật khẩu = mật khẩu của tài khoản Admin hiện có (dùng lại hash), hãy đổi sau khi triển khai thật. NV001-003 = QuanLy, NV004-006 = NhanVien.
   Đăng nhập: nv1@utccinema.vn ... nv6@utccinema.vn */
SET NOCOUNT ON;
DECLARE @hash NVARCHAR(MAX); SELECT TOP 1 @hash = u.PasswordHash FROM AspNetUsers u
  JOIN AspNetUserRoles ur ON ur.UserId = u.Id JOIN AspNetRoles r ON r.Id = ur.RoleId WHERE r.Name = 'Admin';
IF @hash IS NOT NULL
BEGIN
  INSERT AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
                      PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, HoTen)
  SELECT CAST(NEWID() AS NVARCHAR(36)), nv.Email, UPPER(nv.Email), nv.Email, UPPER(nv.Email), 1, @hash,
         CAST(NEWID() AS NVARCHAR(36)), CAST(NEWID() AS NVARCHAR(36)), 0, 0, 1, 0, nv.HoTen
  FROM NHAN_VIEN nv WHERE nv.UserId IS NULL AND nv.Email IS NOT NULL
    AND NOT EXISTS (SELECT 1 FROM AspNetUsers x WHERE x.NormalizedEmail = UPPER(nv.Email));

  UPDATE nv SET UserId = u.Id FROM NHAN_VIEN nv JOIN AspNetUsers u ON u.NormalizedEmail = UPPER(nv.Email) WHERE nv.UserId IS NULL;

  INSERT AspNetUserRoles (UserId, RoleId)
  SELECT nv.UserId, r.Id FROM NHAN_VIEN nv JOIN AspNetRoles r ON r.Name = nv.ChucVu
  WHERE nv.UserId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM AspNetUserRoles x WHERE x.UserId = nv.UserId AND x.RoleId = r.Id);
END
SELECT nv.MaNv, nv.HoTen, nv.Email, nv.ChucVu, (SELECT COUNT(*) FROM AspNetUserRoles x WHERE x.UserId = nv.UserId) AS so_vai_tro FROM NHAN_VIEN nv ORDER BY nv.Id;
