using TcmAiDiagnosis.EFContext;
using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Web.Services
{
    public interface IInventoryAlertService
    {
        Task<List<InventoryAlertResult>> CheckAndProcessAlertsAsync(int? tenantId = null);
        Task<AlertStatistics> GetAlertStatisticsAsync(int? tenantId = null);
    }

    public class InventoryAlertService : IInventoryAlertService
    {
        private readonly TcmAiDiagnosisContext _context;
        private readonly InventoryAlertRuleDomain _alertRuleDomain;

        public InventoryAlertService(
            TcmAiDiagnosisContext context,
            InventoryAlertRuleDomain alertRuleDomain)
        {
            _context = context;
            _alertRuleDomain = alertRuleDomain;
        }

        public async Task<List<InventoryAlertResult>> CheckAndProcessAlertsAsync(int? tenantId = null)
        {
            var alerts = await _alertRuleDomain.CheckInventoryAlertsAsync(tenantId);

            // 这里可以添加预警处理逻辑，比如发送通知等
            if (alerts.Any())
            {
                // 记录预警日志或发送通知
                await ProcessAlertNotifications(alerts);
            }

            return alerts;
        }

        public async Task<AlertStatistics> GetAlertStatisticsAsync(int? tenantId = null)
        {
            var alerts = await _alertRuleDomain.CheckInventoryAlertsAsync(tenantId);

            return new AlertStatistics
            {
                TotalAlerts = alerts.Count,
                CriticalAlerts = alerts.Count(a => a.Priority == 3),
                HighAlerts = alerts.Count(a => a.Priority == 2),
                MediumAlerts = alerts.Count(a => a.Priority == 1),
                ResolvedAlerts = 0 // 可以根据需要实现已处理预警的统计
            };
        }

        private async Task ProcessAlertNotifications(List<InventoryAlertResult> alerts)
        {
            // 实现预警通知逻辑
            // 可以发送系统通知、邮件、短信等
            foreach (var alert in alerts)
            {
                // 记录到日志或发送通知
                Console.WriteLine($"预警: {alert.AlertMessage}");
            }

            await Task.CompletedTask;
        }
    }

    public class AlertStatistics
    {
        public int TotalAlerts { get; set; }
        public int CriticalAlerts { get; set; }
        public int HighAlerts { get; set; }
        public int MediumAlerts { get; set; }
        public int ResolvedAlerts { get; set; }
    }
}