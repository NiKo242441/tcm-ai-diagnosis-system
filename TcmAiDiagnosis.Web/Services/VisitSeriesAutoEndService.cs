using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Web.Services
{
    /// <summary>
    /// 就诊系列自动结束后台服务
    /// 定期检查并自动结束长时间无活动的就诊系列
    /// </summary>
    public class VisitSeriesAutoEndService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<VisitSeriesAutoEndService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // 每24小时检查一次
        private readonly int _autoEndDays = 90; // 90天无活动自动结束

        public VisitSeriesAutoEndService(IServiceProvider serviceProvider, ILogger<VisitSeriesAutoEndService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("就诊系列自动结束服务已启动");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessAutoEndVisitSeries();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "执行就诊系列自动结束任务时发生错误");
                }

                // 等待下次执行
                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("就诊系列自动结束服务已停止");
        }

        /// <summary>
        /// 处理就诊系列自动结束逻辑
        /// </summary>
        private async Task ProcessAutoEndVisitSeries()
        {
            using var scope = _serviceProvider.CreateScope();
            var visitDomain = scope.ServiceProvider.GetRequiredService<VisitDomain>();

            _logger.LogInformation("开始执行就诊系列自动结束检查");

            try
            {
                // 获取所有进行中的就诊系列
                var activeSeries = await visitDomain.QueryVisitSeriesAsync(0, 0, 0); // doctorId=0表示查询所有医生，tenantId=0表示查询所有租户
                
                var cutoffDate = DateTime.Now.AddDays(-_autoEndDays);
                var endedCount = 0;

                foreach (var series in activeSeries)
                {
                    // 检查系列是否需要自动结束
                    if (ShouldAutoEndSeries(series, cutoffDate))
                    {
                        try
                        {
                            await visitDomain.EndSeriesAsync(series);
                            endedCount++;
                            
                            _logger.LogInformation(
                                "自动结束就诊系列: SeriesId={SeriesId}, PatientId={PatientId}, LastActivity={LastActivity}",
                                series.SeriesId, series.PatientUserId, GetLastActivityDate(series));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, 
                                "自动结束就诊系列失败: SeriesId={SeriesId}, PatientId={PatientId}",
                                series.SeriesId, series.PatientUserId);
                        }
                    }
                }

                _logger.LogInformation("就诊系列自动结束检查完成，共结束 {EndedCount} 个系列", endedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行就诊系列自动结束检查时发生错误");
            }
        }

        /// <summary>
        /// 判断就诊系列是否应该自动结束
        /// </summary>
        /// <param name="series">就诊系列</param>
        /// <param name="cutoffDate">截止日期</param>
        /// <returns>是否应该自动结束</returns>
        private bool ShouldAutoEndSeries(VisitSeries series, DateTime cutoffDate)
        {
            // 如果系列已经结束，跳过
            if (series.Status != 0)
                return false;

            // 获取最后活动时间
            var lastActivityDate = GetLastActivityDate(series);

            // 如果最后活动时间早于截止日期，则需要自动结束
            return lastActivityDate < cutoffDate;
        }

        /// <summary>
        /// 获取就诊系列的最后活动时间
        /// </summary>
        /// <param name="series">就诊系列</param>
        /// <returns>最后活动时间</returns>
        private DateTime GetLastActivityDate(VisitSeries series)
        {
            // 优先使用最后一次就诊时间
            if (series.Visits != null && series.Visits.Any())
            {
                var lastVisitDate = series.Visits.Max(v => v.VisitDate);
                return lastVisitDate;
            }

            // 如果没有就诊记录，使用系列开始时间
            return series.SeriesStartDate;
        }
    }
}