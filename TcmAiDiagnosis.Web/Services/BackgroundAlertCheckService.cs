using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TcmAiDiagnosis.Domain;

namespace TcmAiDiagnosis.Web.Services
{
    public class BackgroundAlertCheckService : BackgroundService
    {
        private readonly ILogger<BackgroundAlertCheckService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public BackgroundAlertCheckService(
            ILogger<BackgroundAlertCheckService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("库存预警后台检查服务已启动");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var alertDomain = scope.ServiceProvider.GetRequiredService<InventoryAlertRuleDomain>();

                    // 每10分钟检查一次预警
                    var alerts = await alertDomain.CheckInventoryAlertsAsync();

                    if (alerts.Any())
                    {
                        _logger.LogWarning("发现 {Count} 个库存预警", alerts.Count);
                        // 这里可以添加预警通知逻辑
                    }

                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "库存预警检查失败");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }
    }
}