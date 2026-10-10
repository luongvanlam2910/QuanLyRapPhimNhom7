using DatVeXemPhim.Data;
using DatVeXemPhim.Exceptions;
using DatVeXemPhim.Models;
using DatVeXemPhim.Models.Entities;
using DatVeXemPhim.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DatVeXemPhim.Services;

public class DatVeService : IDatVeService
{
    private const int SoVeToiDaMoiDon = 8;

    private readonly ApplicationDbContext _db;
    private readonly ISeatHoldService _seatHoldService;
    private readonly IConfiguration _configuration;
    private readonly IKiemTraDoTuoiService _kiemTraDoTuoiService;

    public DatVeService(
        ApplicationDbContext db,
        ISeatHoldService seatHoldService,
        IConfiguration configuration,
        IKiemTraDoTuoiService kiemTraDoTuoiService)
    {
        _db = db;
        _seatHoldService = seatHoldService;
        _configuration = configuration;
        _kiemTraDoTuoiService = kiemTraDoTuoiService;
    }

    private int BookingCutoffMinutes => _configuration.GetValue<int?>("app:BookingCutoffMinutes") ?? 0;
    private int ThanhToanTimeoutMinutes => _configuration.GetValue<int?>("app:SeatHoldMinutes") ?? 10;

    public async Task<List<DatVe>> FindByKhachHangAsync(int khachHangId)
        => await _db.DatVes
            .Include(d => d.Ves)!.ThenInclude(v => v.LichChieu)
            .Include(d => d.Ves)!.ThenInclude(v => v.Ghe)
            .Where(d => d.KhachHangId == khachHangId)
            .OrderByDescending(d => d.NgayDat)
            .ToListAsync();

    public async Task<DatVe?> FindByIdAsync(int id)
        => await _db.DatVes
            .Include(d => d.Ves)!.ThenInclude(v => v.LichChieu)
            .Include(d => d.Ves)!.ThenInclude(v => v.Ghe)
            .Include(d => d.ThanhToans)
            .FirstOrDefaultAsync(d => d.Id == id);

    /// <summary>
    /// Giao dich dat ve: tao DatVe + Ve cho tung ghe, kiem tra trung ghe, tinh khuyen mai.
    /// </summary>
    /// <param name="userKey">Khóa giữ ghế tạm (UserId hoặc ConnectionId) — dùng để giải phóng đúng ghế đã giữ sau khi đặt xong.</param>
    /// <param name="xacNhanThanhToanNgay">
    /// true (bán tại quầy): thu tiền ngay, đơn chuyển "Đã thanh toán" luôn.
    /// false (đặt online): đơn giữ ở "Chờ thanh toán" kèm hạn chót, chuyển sang trang cổng thanh toán mô phỏng
    /// (xem DatVeController.ThanhToanGateway / XacNhanThanhToanAsync) trước khi chốt đơn.
    /// </param>
    public async Task<DatVeResponse> DatVeAsync(DatVeRequest request, int? khachHangId, int? nhanVienId, string? userKey = null, bool xacNhanThanhToanNgay = true)
    {
        if (request.GheIds == null || request.GheIds.Count == 0)
        {
            throw new BusinessException("Vui lòng chọn ít nhất 1 ghế.");
        }

        var gheIds = request.GheIds.Distinct().ToList();
        if (gheIds.Count != request.GheIds.Count)
        {
            throw new BusinessException("Danh sách ghế bị trùng lặp.");
        }

        if (gheIds.Count > SoVeToiDaMoiDon)
        {
            throw new BusinessException($"Mỗi đơn chỉ được đặt tối đa {SoVeToiDaMoiDon} ghế.");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var lichChieu = await _db.LichChieus
                .Include(l => l.Phim)
                .Include(l => l.PhongChieu)!.ThenInclude(p => p!.RapChieu)
                .FirstOrDefaultAsync(l => l.Id == request.LichChieuId)
                ?? throw new ResourceNotFoundException("Lịch chiếu", "Id", request.LichChieuId);

            if (lichChieu.TrangThai != TrangThaiLichChieu.MoBan)
            {
                throw new BusinessException("Suất chiếu này hiện không còn mở bán");
            }

            if (lichChieu.ThoiGianBatDau <= DateTime.Now.AddMinutes(BookingCutoffMinutes))
            {
                throw new BusinessException("Suất chiếu đã bắt đầu hoặc sắp bắt đầu, không thể đặt vé.");
            }

            KhachHang? khachHang = null;
            NhanVien? nhanVien = null;

            if (khachHangId.HasValue)
            {
                khachHang = await _db.KhachHangs.FindAsync(khachHangId.Value)
                    ?? throw new ResourceNotFoundException("Khách hàng", "Id", khachHangId.Value);

                if (khachHang.TrangThai == TrangThaiKhachHang.DaKhoa)
                {
                    throw new BusinessException("Tài khoản của bạn đã bị khóa, không thể đặt vé.");
                }

                _kiemTraDoTuoiService.KiemTra(lichChieu.Phim!, lichChieu, khachHang.NgaySinh);
            }
            else if (nhanVienId.HasValue)
            {
                // Nhân viên bán tại quầy cho khách vãng lai: không ép khách phải có tài khoản/KhachHang.
                nhanVien = await _db.NhanViens.FindAsync(nhanVienId.Value)
                    ?? throw new ResourceNotFoundException("Nhân viên", "Id", nhanVienId.Value);
            }
            else
            {
                throw new BusinessException("Không xác định được khách hàng đặt vé");
            }

            KhuyenMai? khuyenMai = null;
            if (request.KhuyenMaiId.HasValue)
            {
                khuyenMai = await _db.KhuyenMais.FindAsync(request.KhuyenMaiId.Value);
                if (khuyenMai != null)
                {
                    var today = DateOnly.FromDateTime(DateTime.Now);
                    bool hieuLuc = (khuyenMai.NgayBatDau == null || khuyenMai.NgayBatDau <= today)
                        && (khuyenMai.NgayKetThuc == null || khuyenMai.NgayKetThuc >= today);
                    if (!hieuLuc)
                    {
                        khuyenMai = null;
                    }
                }
            }

            var datVe = new DatVe
            {
                KhachHangId = khachHang?.Id,
                TenKhachVangLai = khachHang == null ? request.TenKhachVangLai : null,
                SdtVangLai = khachHang == null ? request.SdtVangLai : null,
                NhanVienId = nhanVien?.Id,
                KhuyenMaiId = khuyenMai?.Id,
                NgayDat = DateTime.Now,
                TongTien = 0,
                TrangThai = TrangThaiDatVe.ChoThanhToan,
                HanThanhToan = xacNhanThanhToanNgay ? null : DateTime.Now.AddMinutes(ThanhToanTimeoutMinutes)
            };
            _db.DatVes.Add(datVe);
            await _db.SaveChangesAsync();
            datVe.MaDat = $"DV{datVe.Id:D6}";

            decimal tongTienGoc = 0;
            var danhSachVe = new List<Ve>();

            foreach (var gheId in gheIds)
            {
                var ghe = await _db.Ghes.FindAsync(gheId)
                    ?? throw new ResourceNotFoundException("Ghế", "Id", gheId);

                if (ghe.PhongChieuId != lichChieu.PhongChieuId)
                {
                    throw new BusinessException($"Ghế {ghe.HangGhe}{ghe.SoGhe} không thuộc phòng chiếu của suất này.");
                }

                if (ghe.TrangThai != TrangThaiGhe.HoatDong)
                {
                    throw new BusinessException($"Ghế {ghe.HangGhe}{ghe.SoGhe} đang bảo trì, không thể đặt.");
                }

                var veTonTai = await _db.Ves.AnyAsync(v =>
                    v.LichChieuId == request.LichChieuId && v.GheId == gheId && v.TrangThai != TrangThaiVe.DaHuy);
                if (veTonTai)
                {
                    throw new GheDaDatException($"Ghế {ghe.HangGhe}{ghe.SoGhe} đã được đặt. Vui lòng chọn ghế khác.");
                }

                // Ghế đang được người KHÁC giữ tạm thì không cho đặt, kể cả khi chưa có Ve trong DB.
                if (!string.IsNullOrWhiteSpace(userKey) &&
                    await _seatHoldService.IsHeldByOtherAsync(lichChieu.Id, gheId, userKey))
                {
                    throw new GheDaDatException($"Ghế {ghe.HangGhe}{ghe.SoGhe} đang được người khác giữ tạm. Vui lòng chọn ghế khác.");
                }

                var giaVeGhe = TinhGiaVeTheoLoaiGhe(lichChieu.GiaVe, ghe.LoaiGhe);

                var ve = new Ve
                {
                    DatVeId = datVe.Id,
                    LichChieuId = lichChieu.Id,
                    GheId = ghe.Id,
                    Ghe = ghe,
                    GiaVe = giaVeGhe,
                    TrangThai = TrangThaiVe.DaDat
                };
                _db.Ves.Add(ve);
                danhSachVe.Add(ve);

                tongTienGoc += giaVeGhe;
            }

            // Lưu trước để từng Ve có Id, rồi mới gán MaVe ổn định không trùng lặp khi đặt đồng thời.
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Vi phạm unique index (LichChieuId, GheId) do có người khác vừa đặt xong trong lúc ta đang xử lý.
                throw new GheDaDatException("Một hoặc nhiều ghế vừa được người khác đặt. Vui lòng chọn lại ghế.");
            }

            foreach (var ve in danhSachVe)
            {
                ve.MaVe = $"VE{ve.Id:D6}";
            }

            var soTienGiam = khuyenMai != null ? TinhGiamGia(tongTienGoc, khuyenMai) : 0m;
            var tongTienSauGiam = Math.Max(tongTienGoc - soTienGiam, 0m);

            datVe.TongTien = tongTienSauGiam;

            ThanhToan? thanhToan = null;
            if (xacNhanThanhToanNgay)
            {
                thanhToan = new ThanhToan
                {
                    DatVeId = datVe.Id,
                    SoTien = tongTienSauGiam,
                    NgayTt = DateTime.Now,
                    PhuongThuc = request.PhuongThucThanhToan,
                    TrangThai = TrangThaiThanhToan.ThanhCong
                };
                _db.ThanhToans.Add(thanhToan);
                datVe.TrangThai = TrangThaiDatVe.DaThanhToan;
            }

            await _db.SaveChangesAsync();
            if (thanhToan != null)
            {
                thanhToan.MaTt = $"TT{thanhToan.Id:D6}";
                await _db.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            // Giải phóng ghế giữ tạm đúng theo key đã dùng để giữ (sửa lỗi dùng nhầm khachHangId làm key release).
            // Từ đây ghế đã có bản ghi Ve chính thức (được unique index bảo vệ) nên có thể giải phóng
            // hold tạm dù đơn chưa thanh toán xong — HuyDonQuaHanService sẽ dọn nếu khách bỏ dở.
            if (!string.IsNullOrWhiteSpace(userKey))
            {
                _seatHoldService.Release(lichChieu.Id, userKey);
            }

            return new DatVeResponse
            {
                DatVeId = datVe.Id,
                MaDat = datVe.MaDat,
                TenPhim = lichChieu.Phim!.TenPhim,
                TenPhong = lichChieu.PhongChieu!.TenPhong,
                TenRap = lichChieu.PhongChieu.RapChieu!.TenRap,
                ThoiGianChieu = lichChieu.ThoiGianBatDau,
                GheDaDat = danhSachVe.Select(v => new GheInfoDto
                {
                    MaVe = v.MaVe,
                    HangGhe = v.Ghe!.HangGhe,
                    SoGhe = v.Ghe.SoGhe,
                    LoaiGhe = v.Ghe.LoaiGhe,
                    GiaVe = v.GiaVe
                }).ToList(),
                TongTienGoc = tongTienGoc,
                SoTienGiam = soTienGiam,
                TongTienSauGiam = tongTienSauGiam,
                TrangThai = datVe.TrangThai ?? TrangThaiDatVe.ChoThanhToan,
                MaTt = thanhToan?.MaTt
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<DatVeResponse> XacNhanThanhToanAsync(string maDat, bool thanhCong, string? phuongThucThanhToan)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var datVe = await _db.DatVes
                .Include(d => d.Ves)!.ThenInclude(v => v.Ghe)
                .Include(d => d.Ves)!.ThenInclude(v => v.LichChieu)!.ThenInclude(lc => lc!.Phim)
                .Include(d => d.Ves)!.ThenInclude(v => v.LichChieu)!.ThenInclude(lc => lc!.PhongChieu)!.ThenInclude(pc => pc!.RapChieu)
                .FirstOrDefaultAsync(d => d.MaDat == maDat)
                ?? throw new ResourceNotFoundException("Đặt vé", "Mã đặt", maDat);

            if (datVe.TrangThai != TrangThaiDatVe.ChoThanhToan)
            {
                throw new BusinessException($"Đơn đặt vé này không ở trạng thái chờ thanh toán (hiện tại: {datVe.TrangThai}).");
            }

            if (datVe.HanThanhToan.HasValue && datVe.HanThanhToan.Value < DateTime.Now)
            {
                throw new BusinessException("Đơn đặt vé đã quá hạn thanh toán. Vui lòng đặt vé lại.");
            }

            var ve0 = datVe.Ves.First();
            var lichChieu = ve0.LichChieu!;

            var thanhToan = new ThanhToan
            {
                DatVeId = datVe.Id,
                SoTien = datVe.TongTien,
                NgayTt = DateTime.Now,
                PhuongThuc = phuongThucThanhToan,
                TrangThai = thanhCong ? TrangThaiThanhToan.ThanhCong : TrangThaiThanhToan.ThatBai
            };
            _db.ThanhToans.Add(thanhToan);
            await _db.SaveChangesAsync();
            thanhToan.MaTt = $"TT{thanhToan.Id:D6}";

            if (thanhCong)
            {
                datVe.TrangThai = TrangThaiDatVe.DaThanhToan;
            }
            else
            {
                // Thanh toán thất bại: hủy đơn và trả lại ghế ngay để người khác có thể đặt.
                datVe.TrangThai = TrangThaiDatVe.DaHuy;
                foreach (var ve in datVe.Ves)
                {
                    ve.TrangThai = TrangThaiVe.DaHuy;
                }
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new DatVeResponse
            {
                DatVeId = datVe.Id,
                MaDat = datVe.MaDat,
                TenPhim = lichChieu.Phim!.TenPhim,
                TenPhong = lichChieu.PhongChieu!.TenPhong,
                TenRap = lichChieu.PhongChieu.RapChieu!.TenRap,
                ThoiGianChieu = lichChieu.ThoiGianBatDau,
                GheDaDat = datVe.Ves.Select(v => new GheInfoDto
                {
                    MaVe = v.MaVe,
                    HangGhe = v.Ghe!.HangGhe,
                    SoGhe = v.Ghe.SoGhe,
                    LoaiGhe = v.Ghe.LoaiGhe,
                    GiaVe = v.GiaVe
                }).ToList(),
                TongTienGoc = datVe.Ves.Sum(v => v.GiaVe),
                SoTienGiam = 0,
                TongTienSauGiam = datVe.TongTien,
                TrangThai = datVe.TrangThai ?? "",
                MaTt = thanhToan.MaTt
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Quét và tự hủy các đơn "Chờ thanh toán" đã quá hạn, trả ghế lại cho hệ thống.
    /// Gọi định kỳ bởi HuyDonQuaHanService (BackgroundService).
    /// </summary>
    public async Task<int> HuyCacDonQuaHanAsync()
    {
        var now = DateTime.Now;
        var donQuaHan = await _db.DatVes
            .Include(d => d.Ves)
            .Where(d => d.TrangThai == TrangThaiDatVe.ChoThanhToan && d.HanThanhToan != null && d.HanThanhToan < now)
            .ToListAsync();

        foreach (var datVe in donQuaHan)
        {
            datVe.TrangThai = TrangThaiDatVe.DaHuy;
            foreach (var ve in datVe.Ves)
            {
                ve.TrangThai = TrangThaiVe.DaHuy;
            }
        }

        if (donQuaHan.Count > 0)
        {
            await _db.SaveChangesAsync();
        }

        return donQuaHan.Count;
    }

    public async Task HuyDatVeAsync(int datVeId, int khachHangId)
        => await HuyDatVeInternalAsync(datVeId, khachHangId, isStaff: false);

    /// <summary>
    /// Nhân viên/quản lý hủy đơn (ví dụ hủy cả suất chiếu, hoặc khách yêu cầu qua quầy).
    /// Khác với khách tự hủy: không kiểm tra quyền sở hữu, và đồng bộ trạng thái Vé/Thanh toán.
    /// </summary>
    public async Task HuyDatVeByStaffAsync(int datVeId)
        => await HuyDatVeInternalAsync(datVeId, khachHangId: null, isStaff: true);

    /// <summary>
    /// Nhân viên/quản lý đổi trạng thái đơn — chỉ cho phép các bước chuyển hợp lệ:
    ///   Chờ thanh toán → Đã thanh toán (ghi nhận thanh toán) | Đã hủy (trả ghế)
    ///   Đã thanh toán  → Đã hủy (hoàn tiền, trả ghế)
    ///   Đã hủy         → (không được mở lại: ghế có thể đã có người khác đặt)
    /// Đơn đã thanh toán KHÔNG được quay về "Chờ thanh toán".
    /// </summary>
    public async Task DoiTrangThaiByStaffAsync(int datVeId, string trangThaiMoi)
    {
        if (trangThaiMoi != TrangThaiDatVe.ChoThanhToan && trangThaiMoi != TrangThaiDatVe.DaThanhToan && trangThaiMoi != TrangThaiDatVe.DaHuy)
        {
            throw new BusinessException("Trạng thái không hợp lệ.");
        }

        var datVe = await _db.DatVes
            .Include(d => d.Ves)
            .Include(d => d.ThanhToans)
            .FirstOrDefaultAsync(d => d.Id == datVeId)
            ?? throw new ResourceNotFoundException("Đặt vé", "Id", datVeId);

        var cu = datVe.TrangThai;
        if (cu == trangThaiMoi)
        {
            return;
        }

        if (cu == TrangThaiDatVe.DaHuy)
        {
            throw new BusinessException("Đơn đã hủy không thể mở lại vì ghế có thể đã được người khác đặt. Hãy tạo đơn mới.");
        }

        if (trangThaiMoi == TrangThaiDatVe.DaHuy)
        {
            await HuyDatVeInternalAsync(datVeId, khachHangId: null, isStaff: true);
            return;
        }

        if (cu == TrangThaiDatVe.DaThanhToan)
        {
            // trangThaiMoi lúc này chỉ có thể là "Chờ thanh toán"
            throw new BusinessException("Đơn đã thanh toán không thể chuyển về \"Chờ thanh toán\". Nếu cần hoàn tiền, hãy hủy đơn.");
        }

        // Chờ thanh toán → Đã thanh toán
        if (datVe.HanThanhToan.HasValue && datVe.HanThanhToan.Value < DateTime.Now)
        {
            throw new BusinessException("Đơn đã quá hạn thanh toán, ghế đã được trả lại. Hãy tạo đơn mới.");
        }
        if (datVe.Ves.Any(v => v.TrangThai == TrangThaiVe.DaHuy))
        {
            throw new BusinessException("Đơn có vé đã bị hủy, không thể xác nhận thanh toán.");
        }

        var thanhToan = new ThanhToan
        {
            DatVeId = datVe.Id,
            SoTien = datVe.TongTien,
            NgayTt = DateTime.Now,
            PhuongThuc = "Nhân viên xác nhận",
            TrangThai = TrangThaiThanhToan.ThanhCong
        };
        _db.ThanhToans.Add(thanhToan);
        await _db.SaveChangesAsync();
        thanhToan.MaTt = $"TT{thanhToan.Id:D6}";

        datVe.TrangThai = TrangThaiDatVe.DaThanhToan;
        datVe.HanThanhToan = null;
        await _db.SaveChangesAsync();
    }

    private async Task HuyDatVeInternalAsync(int datVeId, int? khachHangId, bool isStaff)
    {
        var datVe = await _db.DatVes
            .Include(d => d.Ves)
            .Include(d => d.ThanhToans)
            .FirstOrDefaultAsync(d => d.Id == datVeId)
            ?? throw new ResourceNotFoundException("Đặt vé", "Id", datVeId);

        if (!isStaff && datVe.KhachHangId != khachHangId)
        {
            throw new BusinessException("Bạn không có quyền hủy đơn này");
        }

        if (datVe.TrangThai == TrangThaiDatVe.DaHuy)
        {
            throw new BusinessException("Đơn đặt vé đã được hủy trước đó.");
        }

        if (!isStaff && datVe.TrangThai == TrangThaiDatVe.DaThanhToan)
        {
            throw new BusinessException("Đơn đã thanh toán không thể hủy qua hệ thống. Vui lòng liên hệ rạp.");
        }

        foreach (var ve in datVe.Ves)
        {
            ve.TrangThai = TrangThaiVe.DaHuy;
        }
        datVe.TrangThai = TrangThaiDatVe.DaHuy;

        // Hủy bởi nhân viên khi đơn đã thanh toán → đánh dấu hoàn tiền để đối soát (fix #5: trước đây
        // Ve vẫn giữ "Đã đặt" khi admin đổi trạng thái DatVe, khiến ghế bị khóa vĩnh viễn).
        if (isStaff)
        {
            foreach (var tt in datVe.ThanhToans.Where(t => t.TrangThai == TrangThaiThanhToan.ThanhCong))
            {
                tt.TrangThai = TrangThaiThanhToan.HoanTien;
            }
        }

        await _db.SaveChangesAsync();
    }

    private static decimal TinhGiaVeTheoLoaiGhe(decimal giaGoc, string? loaiGhe)
    {
        return loaiGhe switch
        {
            "VIP" => Math.Round(giaGoc * 1.3m, 0, MidpointRounding.AwayFromZero),
            "Đôi" => Math.Round(giaGoc * 1.5m, 0, MidpointRounding.AwayFromZero),
            _ => giaGoc
        };
    }

    private static decimal TinhGiamGia(decimal tongTien, KhuyenMai km)
    {
        if (km.LoaiGiam == "PERCENT")
        {
            return Math.Round(tongTien * km.GiaTri / 100m, 0, MidpointRounding.AwayFromZero);
        }
        if (km.LoaiGiam == "AMOUNT")
        {
            return Math.Min(km.GiaTri, tongTien);
        }
        return 0m;
    }
}
