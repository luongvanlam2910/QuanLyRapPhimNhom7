using DatVeXemPhim.Data;
using DatVeXemPhim.Exceptions;
using DatVeXemPhim.Models;
using DatVeXemPhim.Models.Entities;
using DatVeXemPhim.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DatVeXemPhim.Services;

public class GheService : IGheService
{
    private readonly ApplicationDbContext _db;
    private readonly ISeatHoldService _seatHoldService;
    private readonly IConfiguration _configuration;

    public GheService(ApplicationDbContext db, ISeatHoldService seatHoldService, IConfiguration configuration)
    {
        _db = db;
        _seatHoldService = seatHoldService;
        _configuration = configuration;
    }

    private int BookingCutoffMinutes => _configuration.GetValue<int?>("app:BookingCutoffMinutes") ?? 0;

    public async Task<List<Ghe>> FindByPhongAsync(int phongChieuId)
        => await _db.Ghes
            .Where(g => g.PhongChieuId == phongChieuId)
            .OrderBy(g => g.HangGhe).ThenBy(g => g.SoGhe)
            .ToListAsync();

    public async Task<Ghe> FindByIdAsync(int gheId)
        => await _db.Ghes.FindAsync(gheId)
            ?? throw new ResourceNotFoundException("Ghế", "Id", gheId);

    public async Task<List<SeatStatusDto>> GetTrangThaiGheForLichAsync(int lichChieuId, string? sessionOrUserKey)
    {
        var lichChieu = await _db.LichChieus.FindAsync(lichChieuId)
            ?? throw new ResourceNotFoundException("Lịch chiếu", "Id", lichChieuId);

        var tatCaGhe = await FindByPhongAsync(lichChieu.PhongChieuId);

        var maGheDaDat = await _db.Ves
            .Where(v => v.LichChieuId == lichChieuId && v.TrangThai != "Đã hủy")
            .Select(v => v.GheId)
            .ToListAsync();

        var key = string.IsNullOrWhiteSpace(sessionOrUserKey) ? "__GUEST__" : sessionOrUserKey;

        var result = new List<SeatStatusDto>();
        foreach (var ghe in tatCaGhe)
        {
            string trangThai;
            if (ghe.TrangThai != "Hoạt động")
            {
                trangThai = "BAO_TRI";
            }
            else if (maGheDaDat.Contains(ghe.Id))
            {
                trangThai = "DA_DAT";
            }
            else if (await _seatHoldService.IsHeldByOtherAsync(lichChieuId, ghe.Id, key))
            {
                trangThai = "GIU_TAM";
            }
            else
            {
                trangThai = "TRONG";
            }

            result.Add(new SeatStatusDto
            {
                GheId = ghe.Id,
                MaGhe = ghe.MaGhe,
                HangGhe = ghe.HangGhe,
                SoGhe = ghe.SoGhe,
                LoaiGhe = ghe.LoaiGhe,
                TrangThai = trangThai
            });
        }
        return result;
    }

    public async Task GiuGheTamAsync(int lichChieuId, List<int> gheIds, string sessionOrUserKey)
    {
        var lichChieu = await _db.LichChieus.FindAsync(lichChieuId)
            ?? throw new ResourceNotFoundException("Lịch chiếu", "Id", lichChieuId);

        if (lichChieu.TrangThai != TrangThaiLichChieu.MoBan)
        {
            throw new BusinessException("Suất chiếu này hiện không còn mở bán.");
        }

        if (lichChieu.ThoiGianBatDau <= DateTime.Now.AddMinutes(BookingCutoffMinutes))
        {
            throw new BusinessException("Suất chiếu đã bắt đầu hoặc sắp bắt đầu, không thể giữ ghế.");
        }

        foreach (var gheId in gheIds)
        {
            var ghe = await _db.Ghes.FindAsync(gheId)
                ?? throw new ResourceNotFoundException("Ghế", "Id", gheId);

            var veDaDat = await _db.Ves.AnyAsync(v =>
                v.LichChieuId == lichChieuId && v.GheId == gheId && v.TrangThai != "Đã hủy");
            if (veDaDat)
            {
                throw new GheDaDatException(
                    $"Ghế {ghe.HangGhe}{ghe.SoGhe} đã được đặt bởi người khác. Vui lòng chọn ghế khác.");
            }

            if (await _seatHoldService.IsHeldByOtherAsync(lichChieuId, gheId, sessionOrUserKey))
            {
                throw new GheDaDatException(
                    $"Ghế {ghe.HangGhe}{ghe.SoGhe} đang được người khác giữ tạm. Vui lòng chọn ghế khác.");
            }

            _seatHoldService.Hold(lichChieuId, gheId, sessionOrUserKey);
        }
    }

    public Task GiaiPhongGiuTamAsync(int lichChieuId, string sessionOrUserKey)
    {
        _seatHoldService.Release(lichChieuId, sessionOrUserKey);
        return Task.CompletedTask;
    }

    public Task GiaiPhongGheAsync(int lichChieuId, IEnumerable<int> gheIds, string sessionOrUserKey)
    {
        _seatHoldService.ReleaseSeats(lichChieuId, sessionOrUserKey, gheIds);
        return Task.CompletedTask;
    }

    public async Task<Ghe> SaveAsync(Ghe ghe)
    {
        if (ghe.Id == 0)
        {
            _db.Ghes.Add(ghe);
        }
        else
        {
            _db.Ghes.Update(ghe);
        }
        await _db.SaveChangesAsync();
        return ghe;
    }

    public async Task<int> CountByPhongAsync(int phongChieuId)
        => await _db.Ghes.CountAsync(g => g.PhongChieuId == phongChieuId);

    /// <summary>
    /// Tự động tạo ghế cho phòng chiếu mới theo đúng sức chứa (SucChua) đã khai báo,
    /// mỗi hàng 10 ghế, tỉ lệ loại ghế ~50% Thường / 37.5% VIP / phần còn lại ghế Đôi
    /// (số ghế Đôi luôn chẵn vì bán theo cặp).
    /// Trước đây hàm này luôn sinh cố định 48 ghế bất kể SucChua nhập vào (fix #8).
    /// </summary>
    public async Task GenerateGheForPhongAsync(PhongChieu phongChieu)
    {
        if (await CountByPhongAsync(phongChieu.Id) > 0)
        {
            return;
        }

        var tongGhe = Math.Max(phongChieu.SucChua, 1);
        const int soGheMoiHang = 10;
        var soHang = (int)Math.Ceiling(tongGhe / (double)soGheMoiHang);

        var soGheDoi = tongGhe / 10 * 2; // ~20% tổng ghế dành cho ghế đôi, luôn chẵn
        if (soGheDoi % 2 != 0) soGheDoi--;
        var soGheVip = (tongGhe - soGheDoi) * 3 / 8; // phần còn lại: ~37.5% VIP, còn lại Thường

        // Dùng Id lớn nhất (không dùng Count): sau khi xóa phòng/ghế, Count giảm xuống và mã ghế mới sẽ trùng mã cũ.
        var maxGheId = (await _db.Ghes.MaxAsync(g => (int?)g.Id)) ?? 0;
        var ghes = new List<Ghe>();
        var daTao = 0;

        for (int hang = 0; hang < soHang && daTao < tongGhe; hang++)
        {
            var tenHang = ((char)('A' + hang)).ToString();
            var soGheHangNay = Math.Min(soGheMoiHang, tongGhe - daTao);

            for (int soGhe = 1; soGhe <= soGheHangNay; soGhe++)
            {
                string loaiGhe;
                if (daTao < soGheVip) loaiGhe = "VIP";
                else if (daTao < soGheVip + soGheDoi) loaiGhe = "Đôi";
                else loaiGhe = "Thường";

                maxGheId++;
                daTao++;
                ghes.Add(new Ghe
                {
                    MaGhe = $"G{maxGheId:D3}",
                    PhongChieuId = phongChieu.Id,
                    HangGhe = tenHang,
                    SoGhe = soGhe,
                    LoaiGhe = loaiGhe,
                    TrangThai = "Hoạt động"
                });
            }
        }

        _db.Ghes.AddRange(ghes);
        await _db.SaveChangesAsync();
    }
}
