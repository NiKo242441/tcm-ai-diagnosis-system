using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TcmAiDiagnosis.EFContext;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Domain
{
    /// <summary>
    /// 就诊记录领域服务类，用于管理就诊记录及就诊系列的业务逻辑
    /// </summary>
    public class VisitDomain
    {
        private readonly TcmAiDiagnosisContext _context;
        private readonly PatientDomain _patientDomain;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VisitDomain(TcmAiDiagnosisContext context, PatientDomain patientDomain, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _patientDomain = patientDomain;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 更新患者所有未结束的就诊系列为已结束状态
        /// </summary>
        /// <param name="patientId">患者ID</param>
        /// <param name="tenantId">租户ID</param>
        /// <param name="endDate">结束日期</param>
        public async Task UpdatePatientActiveSeriesAsync(int patientId, int tenantId, DateTime endDate)
        {
            var activeSeries = await _context.VisitSeries
                .Where(vs => vs.PatientUserId == patientId && vs.TenantId == tenantId && vs.Status == 0)
                .ToListAsync();

            foreach (var series in activeSeries)
            {
                series.Status = 1; // 标记为已结束
                series.SeriesEndDate = endDate;
                series.UpdatedAt = endDate;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 添加单条就诊记录（自动处理序列创建或关联）
        /// </summary>
        /// <param name="newVisit">新就诊记录</param>
        /// <param name="currentDoctorId">当前操作医生ID</param>
        /// <param name="currentTenantId">当前医生所属租户ID</param>
        public async Task AddVisitAsync(Visit newVisit, int currentDoctorId, int currentTenantId)
        {
            // 获取当前登录用户（通过ASP.NET Core Identity）
            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            if (currentUser == null)
                throw new System.Exception("未获取到当前用户信息");
            //// 验证当前用户ID与传入的医生ID是否一致
            //if (currentUser.Id != currentDoctorId)
            //    throw new System.Exception("当前用户无权限操作该医生的就诊记录");
            //// 验证当前用户租户与传入的租户ID是否一致
            //if (currentUser.TenantId != currentTenantId)
            //    throw new System.Exception("当前用户无权限操作该租户的就诊记录");
            VisitSeries? targetSeries;
            DateTime dateTime = DateTime.Now;
            if (newVisit.SeriesId.HasValue && newVisit.SeriesId.Value > 0)
            {
                targetSeries = await GetVisitSeriesByIdAsync(newVisit.SeriesId.Value);
                if (targetSeries == null)
                {
                    throw new ArgumentException("未找到指定的就诊系列");
                }
                if (targetSeries.PatientUserId != newVisit.PatientUserId)
                {
                    throw new ArgumentException("复诊信息与患者不匹配");
                }
                if (targetSeries.Status != 0)
                {
                    throw new System.Exception("就诊系列已结束，无法添加新记录");
                }
                targetSeries.UpdatedAt = dateTime;
            }
            else
            {
                // 创建新就诊系列
                targetSeries = new VisitSeries
                {
                    PatientUserId = newVisit.PatientUserId,
                    DoctorUserId = newVisit.DoctorUserId,
                    TenantId = newVisit.TenantId,
                    ChiefComplaint = newVisit.ChiefComplaint,
                    PatientNotes = await _patientDomain.GetPatientNotes(newVisit.PatientUserId),
                    SeriesStartDate = dateTime,
                    Status = 0,
                    CreatedAt = dateTime,
                    UpdatedAt = dateTime
                };
                _context.VisitSeries.Add(targetSeries);
                //await _context.SaveChangesAsync();
            }

            // 设置就诊顺序
            newVisit.Sequence = targetSeries.Visits?.Count + 1 ?? 1;
            newVisit.UpdatedAt = dateTime;

            // 添加到系列集合
            if (targetSeries.Visits == null)
            {
                targetSeries.Visits = new List<Visit>();
            }
            targetSeries.Visits.Add(newVisit);
            newVisit.Series = targetSeries;

            // 同步更新系列随访/复诊时间
            if (newVisit.NextFollowUpDate.HasValue)
                targetSeries.NextFollowUpDate = newVisit.NextFollowUpDate;
            if (newVisit.NextFollowUpVisitDate.HasValue)
                targetSeries.NextFollowUpVisitDate = newVisit.NextFollowUpVisitDate;

            newVisit.VisitNotes = GenerateVisitNotes(targetSeries);

            await _context.SaveChangesAsync(); // 使用异步方法保存更改
        }

        public async Task UpdateVisitNotes()
        {
            var series = await _context.VisitSeries.Include(x => x.Visits).Include(x => x.PatientUser).ThenInclude(x => x.Detail).ToListAsync();
            foreach (var seriesItem in series)
            {
                seriesItem.PatientNotes = _patientDomain.GetPatientNotes(seriesItem.PatientUser);
                GenerateVisitNotes(seriesItem);
            }
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 结束就诊系列（标记为已结束）
        /// </summary>
        /// <param name="series">目标就诊系列</param>
        public async Task EndSeriesAsync(VisitSeries series)
        {
            // 获取当前登录用户
            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            if (currentUser == null)
                throw new System.Exception("未获取到当前用户信息");
            // 验证就诊系列所属租户是否与当前用户租户一致
            if (series.TenantId != currentUser.TenantId)
                throw new System.Exception("当前用户无权限结束其他租户的就诊系列");
            series.Status = 1;
            series.SeriesEndDate = System.DateTime.Now;
            series.UpdatedAt = System.DateTime.Now;

            await _context.SaveChangesAsync(); // 使用异步方法保存更改
        }

        /// <summary>
        /// 根据ID结束就诊系列（标记为已结束）
        /// </summary>
        /// <param name="seriesId">就诊系列ID</param>
        /// <param name="endReason">结束原因</param>
        /// <param name="operatorUserId">操作用户ID</param>
        public async Task EndSeriesAsync(int seriesId, string endReason, int operatorUserId)
        {
            var series = await GetVisitSeriesByIdAsync(seriesId);
            if (series == null)
                throw new ArgumentException("未找到指定的就诊系列");

            // 获取当前登录用户
            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            if (currentUser == null)
                throw new System.Exception("未获取到当前用户信息");
            
            // 验证就诊系列所属租户是否与当前用户租户一致
            if (series.TenantId != currentUser.TenantId)
                throw new System.Exception("当前用户无权限结束其他租户的就诊系列");

            series.Status = 1;
            series.SeriesEndDate = System.DateTime.Now;
            series.UpdatedAt = System.DateTime.Now;
            // 可以考虑添加结束原因字段到VisitSeries实体中
            // series.EndReason = endReason;

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 根据ID获取就诊系列（包含关联的就诊记录）
        /// </summary>
        public async Task<VisitSeries?> GetVisitSeriesByIdAsync(int seriesId)
        {
            return await _context.VisitSeries
                .Include(vs => vs.Visits)
                .FirstOrDefaultAsync(vs => vs.SeriesId == seriesId);
        }

        /// <summary>
        /// 查询就诊系列（支持医生/租户维度过滤）
        /// </summary>
        /// <param name="doctorId">医生ID（传0则按租户查询）</param>
        /// <param name="tenantId">租户ID</param>
        /// <param name="status">就诊系列状态（默认查询未结束状态）</param>
        /// <returns>就诊系列集合</returns>
        public async Task<IEnumerable<VisitSeries>> QueryVisitSeriesAsync(int doctorId, int tenantId, int status = 0)
        {
            // 使用EF Core查询就诊系列（支持状态过滤）
            return await _context.VisitSeries
                .Include(vs => vs.Visits)
                .Where(vs => (doctorId > 0 ? vs.DoctorUserId == doctorId : vs.TenantId == tenantId) && vs.Status == status)
                .ToListAsync(); // 异步查询就诊系列
        }
        /// <summary>
        /// 查询患者就诊系列
        /// </summary>
        /// <param name="doctorId">医生ID（传0则按租户查询）</param>
        /// <param name="tenantId">租户ID</param>
        /// <param name="status">就诊系列状态（默认查询未结束状态）</param>
        /// <returns>就诊系列集合</returns>
        public async Task<VisitSeries?> QueryVisitSeriesByPatientAsync(int patientId, int tenantId, int status = 0)
        {
            // 使用EF Core查询就诊系列（支持状态过滤）
            return await _context.VisitSeries
                .Include(vs => vs.Visits)
                .Include(vs => vs.DoctorUser)
                .Where(vs => vs.PatientUserId == patientId && vs.TenantId == tenantId && vs.Status == status)
                .OrderByDescending(x => x.SeriesStartDate)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 查询患者所有就诊系列（包括进行中和已完成的）
        /// </summary>
        /// <param name="patientId">患者ID</param>
        /// <param name="tenantId">租户ID</param>
        /// <returns>患者所有就诊系列集合</returns>
        public async Task<IEnumerable<VisitSeries>> QueryAllVisitSeriesByPatientAsync(int patientId, int tenantId)
        {
            // 使用EF Core查询患者所有就诊系列
            return await _context.VisitSeries
                .Include(vs => vs.Visits)
                .Include(vs => vs.DoctorUser)
                .Where(vs => vs.PatientUserId == patientId && vs.TenantId == tenantId)
                .OrderByDescending(x => x.SeriesStartDate)
                .ToListAsync();
        }

        /// <summary>
        /// 根据就诊系列ID获取所有就诊记录
        /// </summary>
        /// <param name="seriesId">就诊系列ID</param>
        /// <returns>就诊记录集合</returns>
        public async Task<List<Visit>> GetVisitsBySeriesIdAsync(int seriesId)
        {
            return await _context.Visits
                .Where(v => v.SeriesId == seriesId)
                .OrderBy(v => v.Sequence)
                .ToListAsync();
        }

        /// <summary>
        /// 根据ID获取就诊记录
        /// </summary>
        /// <param name="visitId">就诊记录ID</param>
        /// <returns>就诊记录</returns>
        public async Task<Visit?> GetVisitByIdAsync(int visitId)
        {
            return await _context.Visits
                .Include(v => v.Series)
                .ThenInclude(s => s.PatientUser)
                .ThenInclude(u => u.Detail)
                .FirstOrDefaultAsync(v => v.VisitId == visitId);
        }

        /// <summary>
        /// 查询全部就诊信息
        /// </summary>
        /// <returns>所有就诊记录集合</returns>
        public async Task<IEnumerable<Visit>> QueryAllVisitsAsync()
        {
            // 使用EF Core查询所有就诊记录
            return await _context.Visits
                .Include(v => v.Series)
                .ToListAsync();
        }



        /// <summary>
        /// 生成问诊信息的文本描述
        /// </summary>
        /// <param name="series">就诊系列</param>
        /// <returns>格式化的问诊信息文本</returns>
        public string GenerateVisitNotes(VisitSeries series)
        {
            if (series == null || series.Visits == null || !series.Visits.Any())
                return string.Empty;

            // 按就诊时间排序
            var sortedVisits = series.Visits.OrderBy(v => v.Sequence).ToList();
            var notes = new System.Text.StringBuilder();
            bool hasMultipleVisits = sortedVisits.Count > 1;

            for (int i = 0; i < sortedVisits.Count; i++)
            {
                var visit = sortedVisits[i];
                bool isCurrentVisit = i == sortedVisits.Count - 1;

                // 添加就诊标题
                if (hasMultipleVisits)
                {
                    if (isCurrentVisit)
                    {
                        notes.AppendLine($"当此就诊/回访（第{visit.Sequence}次）");
                    }
                    else
                    {
                        notes.AppendLine($"第{visit.Sequence}次就诊/随访");
                    }
                }
                var visitType = string.Empty;
                switch (visit.VisitType)
                {
                    case VisitType.Initial:
                        visitType = "初诊";
                        break;
                    case VisitType.FollowUp:
                        visitType = "复诊";
                        break;
                    case VisitType.FollowUpCall:
                        visitType = "随访";
                        break;
                }
                notes.AppendLine($"就诊日期: {visit.VisitDate:yyyy-MM-dd HH:mm}");
                notes.AppendLine($"就诊类型: {visitType}");
                notes.AppendLine($"主诉: {visit.ChiefComplaint}");
                notes.AppendLine($"伴随症状: {visit.AccompanyingSymptoms}");
                notes.AppendLine($"发病天数: {visit.OnsetDays}天");
                notes.AppendLine($"加重时间: {visit.AggravationTime}");
                notes.AppendLine($"气候关系: {visit.ClimateRelationship}");
                notes.AppendLine($"舌苔: {visit.TongueCoating}");
                notes.AppendLine($"舌形: {visit.TongueShape}");
                notes.AppendLine($"舌咽症状: {visit.TongueSymptoms}");
                notes.AppendLine($"脉象类型: {visit.PulseType}");
                notes.AppendLine($"脉象特征: {visit.PulseFeatures}");
                notes.AppendLine($"面色: {visit.FaceColor}");
                notes.AppendLine($"精神状态: {visit.MentalState}");
                notes.AppendLine($"睡眠: {visit.Sleep}");
                notes.AppendLine($"大小便: {visit.Excretion}");
                notes.AppendLine($"情绪状态: {visit.EmotionState}");
                notes.AppendLine($"汗出情况: {visit.Sweating}");
                notes.AppendLine($"体质评估: {visit.ConstitutionAssessment}");
                notes.AppendLine($"下次随访时间: {visit.NextFollowUpDate:yyyy-MM-dd}");
                notes.AppendLine($"下次复诊时间: {visit.NextFollowUpVisitDate:yyyy-MM-dd}");
                notes.AppendLine(); // 空行分隔
                visit.VisitNotes = notes.ToString();
            }

            return notes.ToString();
        }
    }
}