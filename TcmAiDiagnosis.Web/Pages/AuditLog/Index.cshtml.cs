using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TcmAiDiagnosis.EFContext;
using TcmAiDiagnosis.Entities;
using TcmAiDiagnosis.Web.Models;
using System.Text;

namespace TcmAiDiagnosis.Web.Pages.AuditLog
{
    public class IndexModel : PageModel
    {
        private readonly TcmAiDiagnosisContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(TcmAiDiagnosisContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<Entities.AuditLog> AuditLogs { get; set; } = new();

        // 筛选选项 - 使用系统定义的常量
        public List<string> OperationTypes { get; set; } = new()
        {
            "Create", "Update", "Delete", "View", "Search", "Export", "Login", "Logout"
        };

        public List<string> OperationModules { get; set; } = new()
        {
            "Appointment", "Herb", "Inventory", "User", "System", "AuditLog", "Treatment", "Prescription"
        };

        public List<string> OperationStatuses { get; set; } = new()
        {
            "Success", "Failed"
        };

        public List<string> VerificationLevels { get; set; } = new()
        {
            "Level1", "Level2", "Level3", "Level4"
        };

        public List<string> ReviewStatuses { get; set; } = new()
        {
            "Pending", "Approved", "Rejected"
        };

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SelectedOperationType { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SelectedOperationModule { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SelectedStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? RiskLevel { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? VerificationLevel { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ReviewStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public Pagination Pagination { get; set; } = new() { PageSize = 20 };

        public async Task OnGetAsync(int currentPage = 1)
        {
            try
            {
                Pagination.CurrentPage = currentPage;

                var query = _context.AuditLogs
                    .Include(a => a.User)
                    .Include(a => a.Reviewer)
                    .AsQueryable();

                // 应用筛选条件
                query = ApplyFilters(query);

                // 分页查询
                await LoadPagedDataAsync(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载审计日志列表失败");
                TempData["ErrorMessage"] = "加载日志数据失败，请重试";
            }
        }

        private IQueryable<Entities.AuditLog> ApplyFilters(IQueryable<Entities.AuditLog> query)
        {
            // 关键词搜索
            if (!string.IsNullOrEmpty(SearchString))
            {
                query = query.Where(a =>
                    a.OperationDetails.Contains(SearchString) ||
                    a.OperationModule.Contains(SearchString) ||
                    a.OperationType.Contains(SearchString) ||
                    (a.User != null && a.User.FullName.Contains(SearchString)) ||
                    (a.Reviewer != null && a.Reviewer.FullName.Contains(SearchString)) ||
                    a.IpAddress.Contains(SearchString) ||
                    a.RequestPath.Contains(SearchString) ||
                    a.ClinicalReason.Contains(SearchString) ||
                    a.ReviewComments.Contains(SearchString));
            }

            // 操作类型筛选
            if (!string.IsNullOrEmpty(SelectedOperationType))
            {
                query = query.Where(a => a.OperationType == SelectedOperationType);
            }

            // 操作模块筛选
            if (!string.IsNullOrEmpty(SelectedOperationModule))
            {
                query = query.Where(a => a.OperationModule == SelectedOperationModule);
            }

            // 状态筛选
            if (!string.IsNullOrEmpty(SelectedStatus))
            {
                query = query.Where(a => a.OperationStatus == SelectedStatus);
            }

            // 风险等级筛选
            if (!string.IsNullOrEmpty(RiskLevel))
            {
                query = RiskLevel switch
                {
                    "Low" => query.Where(a => a.RiskScore <= 4),
                    "Medium" => query.Where(a => a.RiskScore > 4 && a.RiskScore <= 6),
                    "High" => query.Where(a => a.RiskScore > 6 && a.RiskScore <= 9),
                    "Critical" => query.Where(a => a.RiskScore > 9),
                    _ => query
                };
            }

            // 验证级别筛选
            if (!string.IsNullOrEmpty(VerificationLevel))
            {
                query = query.Where(a => a.VerificationLevel == VerificationLevel);
            }

            // 审核状态筛选
            if (!string.IsNullOrEmpty(ReviewStatus))
            {
                query = query.Where(a => a.ReviewStatus == ReviewStatus);
            }

            // 时间范围筛选
            if (StartDate.HasValue)
            {
                query = query.Where(a => a.OperationTime >= StartDate.Value.Date);
            }

            if (EndDate.HasValue)
            {
                var endDate = EndDate.Value.Date.AddDays(1);
                query = query.Where(a => a.OperationTime < endDate);
            }

            return query;
        }

        private async Task LoadPagedDataAsync(IQueryable<Entities.AuditLog> query)
        {
            Pagination.TotalCount = await query.CountAsync();

            AuditLogs = await query
                .OrderByDescending(a => a.OperationTime)
                .Skip((Pagination.CurrentPage - 1) * Pagination.PageSize)
                .Take(Pagination.PageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        // 生成分页URL的方法
        public string GetPaginationUrl(int page)
        {
            var queryParams = new List<string>
            {
                $"currentPage={page}"
            };

            if (!string.IsNullOrEmpty(SearchString))
                queryParams.Add($"searchString={Uri.EscapeDataString(SearchString)}");

            if (!string.IsNullOrEmpty(SelectedOperationType))
                queryParams.Add($"selectedOperationType={Uri.EscapeDataString(SelectedOperationType)}");

            if (!string.IsNullOrEmpty(SelectedOperationModule))
                queryParams.Add($"selectedOperationModule={Uri.EscapeDataString(SelectedOperationModule)}");

            if (!string.IsNullOrEmpty(SelectedStatus))
                queryParams.Add($"selectedStatus={Uri.EscapeDataString(SelectedStatus)}");

            if (!string.IsNullOrEmpty(RiskLevel))
                queryParams.Add($"riskLevel={Uri.EscapeDataString(RiskLevel)}");

            if (!string.IsNullOrEmpty(VerificationLevel))
                queryParams.Add($"verificationLevel={Uri.EscapeDataString(VerificationLevel)}");

            if (!string.IsNullOrEmpty(ReviewStatus))
                queryParams.Add($"reviewStatus={Uri.EscapeDataString(ReviewStatus)}");

            if (StartDate.HasValue)
                queryParams.Add($"startDate={StartDate.Value:yyyy-MM-dd}");

            if (EndDate.HasValue)
                queryParams.Add($"endDate={EndDate.Value:yyyy-MM-dd}");

            return $"?{string.Join("&", queryParams)}";
        }

        public async Task<IActionResult> OnPostExportAsync()
        {
            try
            {
                var query = _context.AuditLogs
                    .Include(a => a.User)
                    .Include(a => a.Reviewer)
                    .AsQueryable();

                query = ApplyFilters(query);

                var logs = await query
                    .OrderByDescending(a => a.OperationTime)
                    .AsNoTracking()
                    .ToListAsync();

                // 生成CSV内容
                var csvContent = GenerateCsvContent(logs);

                // 返回CSV文件
                var fileName = $"audit_logs_export_{DateTime.Now:yyyyMMddHHmmss}.csv";
                return File(Encoding.UTF8.GetBytes(csvContent), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "导出审计日志失败");
                TempData["ErrorMessage"] = "导出失败，请重试";
                return RedirectToPage();
            }
        }

        private string GenerateCsvContent(List<Entities.AuditLog> logs)
        {
            var sb = new StringBuilder();

            // CSV头部 - 包含新字段
            sb.AppendLine("操作ID,操作时间,用户姓名,用户ID,IP地址,操作模块,操作类型,操作状态,风险评分,验证级别,审核状态,审核人,执行时长(ms),操作详情,临床理由,审核意见,请求路径,错误信息");

            // 数据行
            foreach (var log in logs)
            {
                var userFullName = log.User?.FullName ?? "系统用户";
                var reviewerName = log.Reviewer?.FullName ?? "";
                var executionDuration = log.ExecutionDuration?.ToString() ?? "";
                var riskScore = log.RiskScore?.ToString() ?? "";
                var operationDetails = EscapeCsvField(log.OperationDetails ?? "");
                var clinicalReason = EscapeCsvField(log.ClinicalReason ?? "");
                var reviewComment = EscapeCsvField(log.ReviewComments ?? "");
                var requestPath = EscapeCsvField(log.RequestPath ?? "");
                var errorMessage = EscapeCsvField(log.ErrorMessage ?? "");
                var verificationLevel = GetVerificationLevelDisplayName(log.VerificationLevel ?? "");
                var reviewStatus = GetReviewStatusDisplayName(log.ReviewStatus ?? "");

                sb.AppendLine($"\"{log.LogId}\",\"{log.OperationTime:yyyy-MM-dd HH:mm:ss}\",\"{userFullName}\",\"{log.UserId}\",\"{log.IpAddress}\",\"{log.OperationModule}\",\"{log.OperationType}\",\"{log.OperationStatus}\",\"{riskScore}\",\"{verificationLevel}\",\"{reviewStatus}\",\"{reviewerName}\",\"{executionDuration}\",\"{operationDetails}\",\"{clinicalReason}\",\"{reviewComment}\",\"{requestPath}\",\"{errorMessage}\"");
            }

            return sb.ToString();
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";

            // 转义CSV中的特殊字符
            field = field.Replace("\"", "\"\"");
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                field = $"\"{field}\"";
            }
            return field;
        }

        // 辅助方法
        private string GetVerificationLevelDisplayName(string level)
        {
            return level switch
            {
                "Level1" => "一级验证",
                "Level2" => "二级验证",
                "Level3" => "三级验证",
                "Level4" => "四级验证",
                _ => level
            };
        }

        private string GetReviewStatusDisplayName(string status)
        {
            return status switch
            {
                "Pending" => "待审核",
                "Approved" => "已批准",
                "Rejected" => "已拒绝",
                _ => status
            };
        }
    }
}