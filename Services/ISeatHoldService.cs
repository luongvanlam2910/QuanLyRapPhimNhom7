namespace DatVeXemPhim.Services;

public interface ISeatHoldService
{
    void Hold(int lichChieuId, int gheId, string userKey);
    Task<bool> IsHeldByOtherAsync(int lichChieuId, int gheId, string userKeyCurrent);
    void Release(int lichChieuId, string userKey);
    /// <summary>Nhả riêng một số ghế mà người dùng này đang giữ (bỏ chọn ghế).</summary>
    void ReleaseSeats(int lichChieuId, string userKey, IEnumerable<int> gheIds);
}
