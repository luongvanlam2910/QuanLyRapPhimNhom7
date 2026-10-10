using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace DatVeXemPhim.Services;

/// <summary>
/// Giu ghe tam thoi bang IN-MEMORY cache (thay the Java ConcurrentHashMap + @Scheduled cleanup).
/// IMemoryCache tu dong het han entry (lazy expiration), khong can timer don dep thu cong.
/// </summary>
public class SeatHoldService : ISeatHoldService
{
    private record HoldInfo(string UserKey);

    private readonly IMemoryCache _cache;
    private readonly int _seatHoldMinutes;

    // Theo doi cac ghe dang duoc giu theo tung lich chieu, de ho tro Release(lichChieuId, userKey)
    // (IMemoryCache khong ho tro enumerate theo prefix).
    private readonly ConcurrentDictionary<int, ConcurrentDictionary<int, byte>> _gheByLich = new();

    public SeatHoldService(IMemoryCache cache, IConfiguration configuration)
    {
        _cache = cache;
        _seatHoldMinutes = configuration.GetValue<int?>("app:SeatHoldMinutes") ?? 10;
    }

    private static string Key(int lichChieuId, int gheId) => $"seat-hold:{lichChieuId}:{gheId}";

    public void Hold(int lichChieuId, int gheId, string userKey)
    {
        var gheSet = _gheByLich.GetOrAdd(lichChieuId, _ => new ConcurrentDictionary<int, byte>());
        gheSet[gheId] = 0;

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_seatHoldMinutes)
        };
        options.RegisterPostEvictionCallback((key, value, reason, state) =>
        {
            gheSet.TryRemove(gheId, out _);
        });

        _cache.Set(Key(lichChieuId, gheId), new HoldInfo(userKey), options);
    }

    public Task<bool> IsHeldByOtherAsync(int lichChieuId, int gheId, string userKeyCurrent)
    {
        if (_cache.TryGetValue(Key(lichChieuId, gheId), out HoldInfo? info) && info is not null)
        {
            return Task.FromResult(info.UserKey != userKeyCurrent);
        }
        return Task.FromResult(false);
    }

    public void Release(int lichChieuId, string userKey)
    {
        if (!_gheByLich.TryGetValue(lichChieuId, out var gheSet))
        {
            return;
        }

        foreach (var gheId in gheSet.Keys.ToList())
        {
            if (_cache.TryGetValue(Key(lichChieuId, gheId), out HoldInfo? info) && info is not null && info.UserKey == userKey)
            {
                _cache.Remove(Key(lichChieuId, gheId));
                gheSet.TryRemove(gheId, out _);
            }
        }
    }

    public void ReleaseSeats(int lichChieuId, string userKey, IEnumerable<int> gheIds)
    {
        _gheByLich.TryGetValue(lichChieuId, out var gheSet);
        foreach (var gheId in gheIds.Distinct())
        {
            // Chỉ nhả ghế do chính người này giữ, không nhả nhầm ghế người khác.
            if (_cache.TryGetValue(Key(lichChieuId, gheId), out HoldInfo? info) && info is not null && info.UserKey == userKey)
            {
                _cache.Remove(Key(lichChieuId, gheId));
                gheSet?.TryRemove(gheId, out _);
            }
        }
    }
}
