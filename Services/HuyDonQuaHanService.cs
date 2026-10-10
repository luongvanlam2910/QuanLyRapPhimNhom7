namespace DatVeXemPhim.Services;

/// <summary>
/// Chạy nền định kỳ mỗi phút, tự hủy các đơn "Chờ thanh toán" đã quá hạn (khách bỏ dở ở trang
/// cổng thanh toán mô phỏng) để trả ghế lại cho hệ thống kịp thời thay vì khóa ghế vô thời hạn.
/// </summary>
public class HuyDonQuaHanService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HuyDonQuaHanService> _logger;

    public HuyDonQuaHanService(IServiceProvider serviceProvider, ILogger<HuyDonQuaHanService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var datVeService = scope.ServiceProvider.GetRequiredService<IDatVeService>();
                var soLuong = await datVeService.HuyCacDonQuaHanAsync();
                if (soLuong > 0)
                {
                    _logger.LogInformation("Đã tự động hủy {SoLuong} đơn đặt vé quá hạn thanh toán.", soLuong);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi quét đơn đặt vé quá hạn thanh toán.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Ứng dụng đang dừng — thoát vòng lặp.
            }
        }
    }
}
