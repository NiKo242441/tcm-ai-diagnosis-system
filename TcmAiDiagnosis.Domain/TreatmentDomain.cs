using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;
using MySqlConnector;
using TcmAiDiagnosis.Entities;
using TcmAiDiagnosis.Dtos;
using TcmAiDiagnosis.EFContext;
using TcmAiDiagnosis.Entities.Enums;
using TcmAiDiagnosis.Domain.Exceptions;
using TcmAiDiagnosis.Domain.Validators;
using TcmAiDiagnosis.Domain.Services;

namespace TcmAiDiagnosis.Domain
{
    /// <summary>
    /// 治疗方案领域服务 - 实现智能治疗方案生成的核心业务逻辑
    /// 包含并发控制、异步生成、Dify API集成等功能
    /// </summary>
    public partial class TreatmentDomain
    {
        private readonly TcmAiDiagnosisContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DifyApiOptions _difyApiOptions;
        private readonly ILogger<TreatmentDomain> _logger;
        private readonly SyndromeDomain _syndromeDomain;
        private readonly VisitDomain _visitDomain;
        private readonly TreatmentDataValidator _validator;
        private readonly RetryPolicyService _retryPolicyService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// 构造函数 - 注入所有必要的依赖项
        /// </summary>
        /// <param name="context">数据库上下文</param>
        /// <param name="mapper">对象映射器</param>
        /// <param name="httpClientFactory">HTTP客户端工厂</param>
        /// <param name="difyApiOptions">Dify API配置选项</param>
        /// <param name="logger">日志记录器</param>
        /// <param name="syndromeDomain">证候领域服务</param>
        /// <param name="visitDomain">就诊领域服务</param>
        /// <param name="validator">数据验证器</param>
        /// <param name="retryPolicyService">重试策略服务</param>
        /// <param name="httpContextAccessor">HTTP上下文访问器</param>
        /// <param name="userManager">用户管理器</param>
        public TreatmentDomain(
            TcmAiDiagnosisContext context,
            IMapper mapper,
            IHttpClientFactory httpClientFactory,
            IOptions<DifyApiOptions> difyApiOptions,
            ILogger<TreatmentDomain> logger,
            SyndromeDomain syndromeDomain,
            VisitDomain visitDomain,
            TreatmentDataValidator validator,
            RetryPolicyService retryPolicyService,
            IHttpContextAccessor httpContextAccessor,
            UserManager<User> userManager)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _difyApiOptions = difyApiOptions?.Value ?? throw new ArgumentNullException(nameof(difyApiOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _syndromeDomain = syndromeDomain ?? throw new ArgumentNullException(nameof(syndromeDomain));
            _visitDomain = visitDomain ?? throw new ArgumentNullException(nameof(visitDomain));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _retryPolicyService = retryPolicyService ?? throw new ArgumentNullException(nameof(retryPolicyService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

            // 验证Dify API配置
            if (!_difyApiOptions.IsValid())
            {
                throw new InvalidConfigurationException("DifyApi", "Dify API 配置无效，请检查 appsettings.json 中的 DifyApi 配置节");
            }
        }

        /// <summary>
        /// 获取指定证候ID的最新治疗方案
        /// </summary>
        /// <param name="syndromeId">证候ID</param>
        /// <returns>治疗方案详情DTO，如果不存在则返回null</returns>
        /// <exception cref="ArgumentException">当证候ID无效时抛出</exception>
        public async Task<TreatmentDto?> GetLatestTreatmentBySyndromeIdAsync(int syndromeId)
        {
            if (syndromeId <= 0)
            {
                throw new ArgumentException("证候ID必须大于0", nameof(syndromeId));
            }

            try
            {
                _logger.LogInformation("开始获取证候 {SyndromeId} 的最新治疗方案", syndromeId);

                // 第一步：获取基础治疗方案信息
                var treatment = await _context.Treatments
                    .Where(t => t.SyndromeId == syndromeId && t.IsLatest)
                    .FirstOrDefaultAsync();

                if (treatment == null)
                {
                    _logger.LogInformation("证候 {SyndromeId} 的治疗方案不存在", syndromeId);
                    return null;
                }

                var treatmentId = treatment.Id;

                // 第二步：分步加载关联数据，避免笛卡尔积问题
                // 加载处方数据
                await _context.Prescriptions
                    .Include(p => p.PrescriptionItems)
                    .Where(p => p.TreatmentId == treatmentId)
                    .LoadAsync();

                // 加载针刺数据
                await _context.Acupunctures
                    .Where(a => a.TreatmentId == treatmentId)
                    .LoadAsync();

                // 加载艾灸数据
                await _context.Moxibustions
                    .Where(m => m.TreatmentId == treatmentId)
                    .LoadAsync();

                // 加载拔罐数据
                await _context.Cuppings
                    .Where(c => c.TreatmentId == treatmentId)
                    .LoadAsync();

                // 加载食疗数据
                await _context.DietaryTherapies
                    .Include(dt => dt.DietaryTherapyIngredients)
                    .Where(dt => dt.TreatmentId == treatmentId)
                    .LoadAsync();

                // 加载生活方式建议数据
                await _context.LifestyleAdvices
                    .Where(la => la.TreatmentId == treatmentId)
                    .LoadAsync();

                // 加载饮食建议数据
                await _context.DietaryAdvices
                    .Include(da => da.RecommendedFoods)
                    .Include(da => da.AvoidedFoods)
                    .Where(da => da.TreatmentId == treatmentId)
                    .LoadAsync();

                // 加载随访建议数据
                await _context.FollowUpAdvices
                    .Include(fa => fa.MonitoringIndicators)
                    .Where(fa => fa.TreatmentId == treatmentId)
                    .LoadAsync();

                // 加载中药警告数据
                await _context.HerbalWarnings
                    .Include(hw => hw.AffectedMedications)
                    .Where(hw => hw.TreatmentId == treatmentId)
                    .LoadAsync();

                // 第三步：映射到DTO
                var treatmentDto = _mapper.Map<TreatmentDto>(treatment);

                _logger.LogInformation("成功获取证候 {SyndromeId} 的治疗方案，状态: {Status}",
                    syndromeId, treatment.Status);

                return treatmentDto;
            }
            catch (MySqlException ex) when (ex.Number == 1205) // 锁等待超时
            {
                throw new TimeoutException($"数据库查询超时：{ex.Message}", ex);
            }
            catch (MySqlException ex) when (ex.Number == 2013) // 连接丢失
            {
                throw new InvalidOperationException($"数据库连接丢失：{ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取证候 {SyndromeId} 的治疗方案时发生错误", syndromeId);
                throw new ApplicationException($"获取治疗方案时发生错误：{ex.Message}", ex);
            }
        }

        /// <summary>
        /// 根据治疗方案ID获取完整的治疗方案详情（包含所有子项，避免笛卡尔积）
        /// </summary>
        /// <param name="treatmentId">治疗方案ID</param>
        /// <returns>治疗方案详情DTO，如果不存在则返回null</returns>
        public async Task<TreatmentDto?> GetTreatmentByIdAsync(int treatmentId)
        {
            if (treatmentId <= 0)
            {
                throw new ArgumentException("治疗方案ID必须大于0", nameof(treatmentId));
            }

            try
            {
                _logger.LogInformation("开始获取治疗方案 {TreatmentId} 的详情", treatmentId);

                var treatment = await _context.Treatments
                    .FirstOrDefaultAsync(t => t.Id == treatmentId);

                if (treatment == null)
                {
                    _logger.LogInformation("治疗方案 {TreatmentId} 不存在", treatmentId);
                    return null;
                }

                // 分步加载子表数据
                await _context.Prescriptions
                    .Include(p => p.PrescriptionItems)
                    .Where(p => p.TreatmentId == treatmentId)
                    .LoadAsync();

                await _context.Acupunctures
                    .Where(a => a.TreatmentId == treatmentId)
                    .LoadAsync();

                await _context.Moxibustions
                    .Where(m => m.TreatmentId == treatmentId)
                    .LoadAsync();

                await _context.Cuppings
                    .Where(c => c.TreatmentId == treatmentId)
                    .LoadAsync();

                await _context.DietaryTherapies
                    .Include(dt => dt.DietaryTherapyIngredients)
                    .Where(dt => dt.TreatmentId == treatmentId)
                    .LoadAsync();

                await _context.LifestyleAdvices
                    .Where(la => la.TreatmentId == treatmentId)
                    .LoadAsync();

                await _context.DietaryAdvices
                    .Include(da => da.RecommendedFoods)
                    .Include(da => da.AvoidedFoods)
                    .Where(da => da.TreatmentId == treatmentId)
                    .LoadAsync();

                await _context.FollowUpAdvices
                    .Include(fa => fa.MonitoringIndicators)
                    .Where(fa => fa.TreatmentId == treatmentId)
                    .LoadAsync();

                await _context.HerbalWarnings
                    .Include(hw => hw.AffectedMedications)
                    .Where(hw => hw.TreatmentId == treatmentId)
                    .LoadAsync();

                var dto = _mapper.Map<TreatmentDto>(treatment);
                _logger.LogInformation("成功获取治疗方案 {TreatmentId} 的详情，状态: {Status}", treatmentId, treatment.Status);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取治疗方案 {TreatmentId} 的详情时发生错误", treatmentId);
                throw new ApplicationException($"获取治疗方案详情时发生错误：{ex.Message}", ex);
            }
        }

        /// <summary>
        /// 创建新版本的治疗方案
        /// </summary>
        /// <param name="edit">编辑DTO</param>
        /// <param name="doctorUserId">医生用户ID</param>
        /// <param name="incrementType">版本递增类型（默认：Patch-保存草稿）</param>
        /// <returns>新创建的治疗方案ID</returns>
        public async Task<int> CreateNewTreatmentVersionAsync(TreatmentDto edit, int doctorUserId, VersionIncrementType incrementType = VersionIncrementType.Patch)
        {
            if (edit == null || edit.Id <= 0)
            {
                throw new ArgumentException("无效的编辑数据或治疗方案ID");
            }

            try
            {
                // 获取当前治疗方案
                var currentTreatment = await _context.Treatments
                    .FirstOrDefaultAsync(t => t.Id == edit.Id);

                if (currentTreatment == null)
                {
                    throw new InvalidOperationException($"治疗方案 {edit.Id} 不存在");
                }

                // 生成新版本号（根据递增类型）
                var newVersion = GenerateNextVersion(currentTreatment.Version, incrementType);

                // 标记所有同证候ID的最新版本为非最新（确保数据一致性）
                var latestTreatments = await _context.Treatments
                    .Where(t => t.SyndromeId == currentTreatment.SyndromeId && t.IsLatest)
                    .ToListAsync();
                
                bool hasChanges = false;
                foreach (var treatment in latestTreatments)
                {
                    treatment.IsLatest = false;
                    hasChanges = true;
                }
                
                // 如果当前治疗方案不在最新版本列表中，也标记它（防御性编程）
                if (!latestTreatments.Any(t => t.Id == currentTreatment.Id) && currentTreatment.IsLatest)
                {
                    currentTreatment.IsLatest = false;
                    hasChanges = true;
                }
                
                // 先保存旧版本的标记，确保数据一致性
                if (hasChanges)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("已标记 {Count} 个旧版本为非最新，证候ID: {SyndromeId}", 
                        latestTreatments.Count, currentTreatment.SyndromeId);
                }

                // 创建新版本的治疗方案
                var newTreatment = new Treatment
                {
                    PatientId = currentTreatment.PatientId,
                    VisitId = currentTreatment.VisitId,
                    SyndromeId = currentTreatment.SyndromeId,
                    TenantId = currentTreatment.TenantId,
                    Version = newVersion,
                    IsLatest = true,
                    IsAiOriginated = false,
                    IsArchived = false,
                    Status = Entities.Enums.TreatmentStatus.Editing,
                    TcmDiagnosis = edit.TcmDiagnosis ?? string.Empty,
                    SyndromeAnalysis = edit.SyndromeAnalysis ?? string.Empty,
                    TreatmentPrinciple = edit.TreatmentPrinciple ?? string.Empty,
                    ExpectedOutcome = edit.ExpectedOutcome ?? string.Empty,
                    Precautions = edit.Precautions ?? string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = doctorUserId,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedByUserId = doctorUserId
                };

                _context.Treatments.Add(newTreatment);
                await _context.SaveChangesAsync();

                // 加载新创建的治疗方案的所有关联数据（为空）
                await _context.Entry(newTreatment).Collection(t => t.Prescriptions).LoadAsync();
                await _context.Entry(newTreatment).Collection(t => t.Acupunctures).LoadAsync();
                await _context.Entry(newTreatment).Collection(t => t.Moxibustions).LoadAsync();
                await _context.Entry(newTreatment).Collection(t => t.Cuppings).LoadAsync();
                await _context.Entry(newTreatment).Collection(t => t.DietaryTherapies).LoadAsync();
                await _context.Entry(newTreatment).Collection(t => t.LifestyleAdvices).LoadAsync();
                await _context.Entry(newTreatment).Collection(t => t.DietaryAdvices).LoadAsync();
                await _context.Entry(newTreatment).Collection(t => t.FollowUpAdvices).LoadAsync();

                // 使用UpdateTreatmentBasicInfoAsync的逻辑来复制数据
                await UpdatePrescriptionsAsync(newTreatment, edit.Prescriptions);
                await UpdateAcupuncturesAsync(newTreatment, edit.Acupunctures);
                await UpdateMoxibustionsAsync(newTreatment, edit.Moxibustions);
                await UpdateCuppingsAsync(newTreatment, edit.Cuppings);
                await UpdateDietaryTherapiesAsync(newTreatment, edit.DietaryTherapies);
                await UpdateLifestyleAdvicesAsync(newTreatment, edit.LifestyleAdvices);
                await UpdateDietaryAdvicesAsync(newTreatment, edit.DietaryAdvices);
                await UpdateFollowUpAdvicesAsync(newTreatment, edit.FollowUpAdvices);

                await _context.SaveChangesAsync();

                _logger.LogInformation("已创建治疗方案新版本 {NewTreatmentId}，版本号: {Version}，原版本: {OldTreatmentId}", 
                    newTreatment.Id, newVersion, edit.Id);

                return newTreatment.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建治疗方案新版本时发生错误，原版本ID: {TreatmentId}", edit.Id);
                throw;
            }
        }

        /// <summary>
        /// 版本递增类型枚举
        /// </summary>
        public enum VersionIncrementType
        {
            /// <summary>
            /// 主版本递增：AI重新生成治疗方案时使用（主版本+1，次版本和修订号归0）
            /// </summary>
            Major,
            /// <summary>
            /// 次版本递增：提交审核时使用（次版本+1，修订号归0）
            /// </summary>
            Minor,
            /// <summary>
            /// 修订号递增：保存草稿时使用（修订号+1）
            /// </summary>
            Patch
        }

        /// <summary>
        /// 生成下一个版本号
        /// </summary>
        /// <param name="currentVersion">当前版本号（格式：V1.0.0）</param>
        /// <param name="incrementType">版本递增类型</param>
        /// <returns>新版本号</returns>
        private string GenerateNextVersion(string currentVersion, VersionIncrementType incrementType = VersionIncrementType.Patch)
        {
            if (string.IsNullOrWhiteSpace(currentVersion))
            {
                return "V1.0.0";
            }

            // 解析版本号：V1.0.0 -> (1, 0, 0)
            var versionStr = currentVersion.TrimStart('V', 'v');
            var parts = versionStr.Split('.');
            
            if (parts.Length != 3)
            {
                _logger.LogWarning("版本号格式不正确: {Version}，使用默认版本号 V1.0.0", currentVersion);
                return "V1.0.0";
            }

            if (!int.TryParse(parts[0], out var major) || 
                !int.TryParse(parts[1], out var minor) || 
                !int.TryParse(parts[2], out var patch))
            {
                _logger.LogWarning("版本号解析失败: {Version}，使用默认版本号 V1.0.0", currentVersion);
                return "V1.0.0";
            }

            // 根据递增类型决定如何递增版本号
            switch (incrementType)
            {
                case VersionIncrementType.Major:
                    // AI重新生成：主版本+1，次版本和修订号归0
                    major++;
                    minor = 0;
                    patch = 0;
                    break;
                case VersionIncrementType.Minor:
                    // 提交审核：次版本+1，修订号归0
                    minor++;
                    patch = 0;
                    break;
                case VersionIncrementType.Patch:
                    // 保存草稿：修订号+1
                    patch++;
                    break;
                default:
                    _logger.LogWarning("未知的版本递增类型: {IncrementType}，使用修订号递增", incrementType);
                    patch++;
                    break;
            }

            return $"V{major}.{minor}.{patch}";
        }

        /// <summary>
        /// 更新治疗方案的基础信息（顶层诊断/原则/注意事项等），并设置状态为 Editing
        /// </summary>
        /// <param name="edit">编辑DTO</param>
        /// <param name="doctorUserId">医生用户ID</param>
        public async Task UpdateTreatmentBasicInfoAsync(TreatmentDto edit, int doctorUserId)
        {
            if (edit == null || edit.Id <= 0)
            {
                throw new ArgumentException("无效的编辑数据或治疗方案ID");
            }

            try
            {
                // 第一步：获取基础治疗方案信息（避免一次性加载所有关联数据导致超时）
                var treatment = await _context.Treatments
                    .FirstOrDefaultAsync(t => t.Id == edit.Id);
                
                if (treatment == null)
                {
                    throw new InvalidOperationException($"治疗方案 {edit.Id} 不存在");
                }

                var treatmentId = treatment.Id;

                // 第二步：分步加载关联数据，避免笛卡尔积问题和查询超时
                // 加载处方数据
                await _context.Prescriptions
                    .Include(p => p.PrescriptionItems)
                    .Where(p => p.TreatmentId == treatmentId)
                    .LoadAsync();

                // 加载针刺数据
                await _context.Acupunctures
                    .Where(a => a.TreatmentId == treatmentId)
                    .LoadAsync();

                // 加载艾灸数据
                await _context.Moxibustions
                    .Where(m => m.TreatmentId == treatmentId)
                    .LoadAsync();

                // 加载拔罐数据
                await _context.Cuppings
                    .Where(c => c.TreatmentId == treatmentId)
                    .LoadAsync();

                // 加载食疗数据
                await _context.DietaryTherapies
                    .Include(dt => dt.DietaryTherapyIngredients)
                    .Where(dt => dt.TreatmentId == treatmentId)
                    .LoadAsync();

                // 加载生活方式建议数据
                await _context.LifestyleAdvices
                    .Where(la => la.TreatmentId == treatmentId)
                    .LoadAsync();

                // 加载饮食建议数据
                await _context.DietaryAdvices
                    .Include(da => da.RecommendedFoods)
                    .Include(da => da.AvoidedFoods)
                    .Where(da => da.TreatmentId == treatmentId)
                    .LoadAsync();

                // 加载随访建议数据
                await _context.FollowUpAdvices
                    .Include(fa => fa.MonitoringIndicators)
                    .Where(fa => fa.TreatmentId == treatmentId)
                    .LoadAsync();

                // 更新顶层字段
                treatment.TcmDiagnosis = edit.TcmDiagnosis ?? string.Empty;
                treatment.SyndromeAnalysis = edit.SyndromeAnalysis ?? string.Empty;
                treatment.TreatmentPrinciple = edit.TreatmentPrinciple ?? string.Empty;
                treatment.ExpectedOutcome = edit.ExpectedOutcome ?? string.Empty;
                treatment.Precautions = edit.Precautions ?? string.Empty;

                // 更新中药处方
                await UpdatePrescriptionsAsync(treatment, edit.Prescriptions);

                // 更新针灸治疗
                await UpdateAcupuncturesAsync(treatment, edit.Acupunctures);

                // 更新艾灸治疗
                await UpdateMoxibustionsAsync(treatment, edit.Moxibustions);

                // 更新拔罐治疗
                await UpdateCuppingsAsync(treatment, edit.Cuppings);

                // 更新食疗方案
                await UpdateDietaryTherapiesAsync(treatment, edit.DietaryTherapies);

                // 更新生活方式建议
                await UpdateLifestyleAdvicesAsync(treatment, edit.LifestyleAdvices);

                // 更新饮食建议
                await UpdateDietaryAdvicesAsync(treatment, edit.DietaryAdvices);

                // 更新随访建议
                await UpdateFollowUpAdvicesAsync(treatment, edit.FollowUpAdvices);

                // 状态更新为医生编辑中
                treatment.Status = Entities.Enums.TreatmentStatus.Editing;
                treatment.UpdatedAt = DateTime.UtcNow;
                treatment.UpdatedByUserId = doctorUserId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("已更新治疗方案 {TreatmentId} 的所有信息并设置为编辑中", edit.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新治疗方案 {TreatmentId} 信息时发生错误", edit.Id);
                throw;
            }
        }

        /// <summary>
        /// 更新中药处方
        /// </summary>
        private async Task UpdatePrescriptionsAsync(Treatment treatment, List<PrescriptionDto> prescriptionDtos)
        {
            // 如果DTO列表为空或null，删除所有现有处方
            if (prescriptionDtos == null || !prescriptionDtos.Any())
            {
                var allPrescriptions = treatment.Prescriptions.ToList();
                foreach (var prescription in allPrescriptions)
                {
                    // 先删除所有药材明细
                    var allItems = prescription.PrescriptionItems.ToList();
                    foreach (var item in allItems)
                    {
                        _context.PrescriptionItems.Remove(item);
                    }
                    _context.Prescriptions.Remove(prescription);
                }
                return;
            }

            foreach (var dto in prescriptionDtos)
            {
                Prescription? prescription = null;
                if (dto.Id > 0)
                {
                    prescription = treatment.Prescriptions.FirstOrDefault(p => p.Id == dto.Id);
                }

                if (prescription == null)
                {
                    // 新建处方
                    prescription = new Prescription
                    {
                        TreatmentId = treatment.Id,
                        Treatment = treatment
                    };
                    treatment.Prescriptions.Add(prescription);
                }

                // 更新处方基本信息
                prescription.Name = dto.Name ?? string.Empty;
                prescription.Category = dto.Category ?? string.Empty;
                prescription.Description = dto.Description ?? string.Empty;
                prescription.Usage = dto.Usage ?? string.Empty;
                prescription.Efficacy = dto.Efficacy ?? string.Empty;
                prescription.Indications = dto.Indications ?? string.Empty;
                prescription.Contraindications = dto.Contraindications ?? string.Empty;
                prescription.Notes = dto.Notes ?? string.Empty;

                // 更新处方药材明细
                if (dto.Items == null || !dto.Items.Any())
                {
                    // 如果DTO中Items为空，删除所有现有药材明细
                    var allItems = prescription.PrescriptionItems.ToList();
                    foreach (var item in allItems)
                    {
                        _context.PrescriptionItems.Remove(item);
                    }
                }
                else
                {
                    foreach (var itemDto in dto.Items)
                    {
                        PrescriptionItem? item = null;
                        if (itemDto.Id > 0)
                        {
                            item = prescription.PrescriptionItems.FirstOrDefault(pi => pi.Id == itemDto.Id);
                        }

                        if (item == null)
                        {
                            item = new PrescriptionItem
                            {
                                Prescription = prescription
                            };
                            prescription.PrescriptionItems.Add(item);
                        }

                        item.Name = itemDto.Name ?? string.Empty;
                        item.Dosage = itemDto.Dosage ?? string.Empty;
                        item.Unit = itemDto.Unit ?? string.Empty;
                        item.ProcessingMethod = itemDto.ProcessingMethod ?? string.Empty;
                        item.Notes = itemDto.Notes ?? string.Empty;
                    }

                    // 删除不在DTO中的药材明细
                    var dtoItemIds = dto.Items.Where(i => i.Id > 0).Select(i => i.Id).ToList();
                    var itemsToRemove = prescription.PrescriptionItems
                        .Where(pi => pi.Id > 0 && !dtoItemIds.Contains(pi.Id))
                        .ToList();
                    foreach (var item in itemsToRemove)
                    {
                        _context.PrescriptionItems.Remove(item);
                    }
                }
            }

            // 删除不在DTO中的处方
            var dtoPrescriptionIds = prescriptionDtos.Where(p => p.Id > 0).Select(p => p.Id).ToList();
            var prescriptionsToRemove = treatment.Prescriptions
                .Where(p => p.Id > 0 && !dtoPrescriptionIds.Contains(p.Id))
                .ToList();
            foreach (var prescription in prescriptionsToRemove)
            {
                // 先删除所有药材明细
                var allItems = prescription.PrescriptionItems.ToList();
                foreach (var item in allItems)
                {
                    _context.PrescriptionItems.Remove(item);
                }
                _context.Prescriptions.Remove(prescription);
            }
        }

        /// <summary>
        /// 更新针灸治疗
        /// </summary>
        private async Task UpdateAcupuncturesAsync(Treatment treatment, List<AcupunctureDto> acupunctureDtos)
        {
            // 如果DTO列表为空或null，删除所有现有记录
            if (acupunctureDtos == null || !acupunctureDtos.Any())
            {
                var allAcupunctures = treatment.Acupunctures.ToList();
                foreach (var acupuncture in allAcupunctures)
                {
                    _context.Acupunctures.Remove(acupuncture);
                }
                return;
            }

            foreach (var dto in acupunctureDtos)
            {
                Acupuncture? acupuncture = null;
                if (dto.Id > 0)
                {
                    acupuncture = treatment.Acupunctures.FirstOrDefault(a => a.Id == dto.Id);
                }

                if (acupuncture == null)
                {
                    acupuncture = new Acupuncture
                    {
                        TreatmentId = treatment.Id,
                        Treatment = treatment
                    };
                    treatment.Acupunctures.Add(acupuncture);
                }

                acupuncture.PointName = dto.PointName ?? string.Empty;
                acupuncture.Location = dto.Location ?? string.Empty;
                acupuncture.Method = dto.Method ?? string.Empty;
                acupuncture.Technique = dto.Technique ?? string.Empty;
                acupuncture.NeedleSpecification = dto.NeedleSpecification ?? string.Empty;
                acupuncture.Depth = dto.Depth ?? string.Empty;
                acupuncture.Duration = dto.Duration ?? string.Empty;
                acupuncture.Frequency = dto.Frequency ?? string.Empty;
                acupuncture.Efficacy = dto.Efficacy ?? string.Empty;
                acupuncture.Indications = dto.Indications ?? string.Empty;
                acupuncture.Instructions = dto.Instructions ?? string.Empty;
                acupuncture.Notes = dto.Notes ?? string.Empty;
                acupuncture.Contraindications = dto.Contraindications ?? string.Empty;
            }

            var dtoIds = acupunctureDtos.Where(a => a.Id > 0).Select(a => a.Id).ToList();
            var toRemove = treatment.Acupunctures
                .Where(a => a.Id > 0 && !dtoIds.Contains(a.Id))
                .ToList();
            foreach (var item in toRemove)
            {
                _context.Acupunctures.Remove(item);
            }
        }

        /// <summary>
        /// 更新艾灸治疗
        /// </summary>
        private async Task UpdateMoxibustionsAsync(Treatment treatment, List<MoxibustionDto> moxibustionDtos)
        {
            // 如果DTO列表为空或null，删除所有现有记录
            if (moxibustionDtos == null || !moxibustionDtos.Any())
            {
                var allMoxibustions = treatment.Moxibustions.ToList();
                foreach (var moxibustion in allMoxibustions)
                {
                    _context.Moxibustions.Remove(moxibustion);
                }
                return;
            }

            foreach (var dto in moxibustionDtos)
            {
                Moxibustion? moxibustion = null;
                if (dto.Id > 0)
                {
                    moxibustion = treatment.Moxibustions.FirstOrDefault(m => m.Id == dto.Id);
                }

                if (moxibustion == null)
                {
                    moxibustion = new Moxibustion
                    {
                        TreatmentId = treatment.Id,
                        Treatment = treatment
                    };
                    treatment.Moxibustions.Add(moxibustion);
                }

                moxibustion.PointName = dto.PointName ?? string.Empty;
                moxibustion.Location = dto.Location ?? string.Empty;
                moxibustion.Method = dto.Method ?? string.Empty;
                moxibustion.MoxaType = dto.MoxaType ?? string.Empty;
                moxibustion.Technique = dto.Technique ?? string.Empty;
                moxibustion.TemperatureControl = dto.TemperatureControl ?? string.Empty;
                moxibustion.Duration = dto.Duration ?? string.Empty;
                moxibustion.Frequency = dto.Frequency ?? string.Empty;
                moxibustion.CourseDuration = dto.CourseDuration ?? string.Empty;
                moxibustion.Efficacy = dto.Efficacy ?? string.Empty;
                moxibustion.Indications = dto.Indications ?? string.Empty;
                moxibustion.TechniquePoints = dto.TechniquePoints ?? string.Empty;
                moxibustion.Precautions = dto.Precautions ?? string.Empty;
                moxibustion.Contraindications = dto.Contraindications ?? string.Empty;
                moxibustion.PostTreatmentCare = dto.PostTreatmentCare ?? string.Empty;
                moxibustion.CombinationTherapy = dto.CombinationTherapy ?? string.Empty;
            }

            var dtoIds = moxibustionDtos.Where(m => m.Id > 0).Select(m => m.Id).ToList();
            var toRemove = treatment.Moxibustions
                .Where(m => m.Id > 0 && !dtoIds.Contains(m.Id))
                .ToList();
            foreach (var item in toRemove)
            {
                _context.Moxibustions.Remove(item);
            }
        }

        /// <summary>
        /// 更新拔罐治疗
        /// </summary>
        private async Task UpdateCuppingsAsync(Treatment treatment, List<CuppingDto> cuppingDtos)
        {
            // 如果DTO列表为空或null，删除所有现有记录
            if (cuppingDtos == null || !cuppingDtos.Any())
            {
                var allCuppings = treatment.Cuppings.ToList();
                foreach (var cupping in allCuppings)
                {
                    _context.Cuppings.Remove(cupping);
                }
                return;
            }

            foreach (var dto in cuppingDtos)
            {
                Cupping? cupping = null;
                if (dto.Id > 0)
                {
                    cupping = treatment.Cuppings.FirstOrDefault(c => c.Id == dto.Id);
                }

                if (cupping == null)
                {
                    cupping = new Cupping
                    {
                        TreatmentId = treatment.Id,
                        Treatment = treatment
                    };
                    treatment.Cuppings.Add(cupping);
                }

                cupping.Area = dto.Area ?? string.Empty;
                cupping.SpecificPoints = dto.SpecificPoints ?? string.Empty;
                cupping.SuitableFor = dto.SuitableFor ?? string.Empty;
                cupping.Method = dto.Method ?? string.Empty;
                cupping.CupType = dto.CupType ?? string.Empty;
                cupping.CupSize = dto.CupSize ?? string.Empty;
                cupping.SuctionStrength = dto.SuctionStrength ?? string.Empty;
                cupping.Duration = dto.Duration ?? string.Empty;
                cupping.Frequency = dto.Frequency ?? string.Empty;
                cupping.Efficacy = dto.Efficacy ?? string.Empty;
                cupping.Indications = dto.Indications ?? string.Empty;
                cupping.TechniquePoints = dto.TechniquePoints ?? string.Empty;
                cupping.Precautions = dto.Precautions ?? string.Empty;
            }

            var dtoIds = cuppingDtos.Where(c => c.Id > 0).Select(c => c.Id).ToList();
            var toRemove = treatment.Cuppings
                .Where(c => c.Id > 0 && !dtoIds.Contains(c.Id))
                .ToList();
            foreach (var item in toRemove)
            {
                _context.Cuppings.Remove(item);
            }
        }

        /// <summary>
        /// 更新食疗方案
        /// </summary>
        private async Task UpdateDietaryTherapiesAsync(Treatment treatment, List<DietaryTherapyDto> dietaryTherapyDtos)
        {
            // 如果DTO列表为空或null，删除所有现有记录
            if (dietaryTherapyDtos == null || !dietaryTherapyDtos.Any())
            {
                var allTherapies = treatment.DietaryTherapies.ToList();
                foreach (var therapy in allTherapies)
                {
                    // 先删除所有食材
                    var allIngredients = therapy.DietaryTherapyIngredients.ToList();
                    foreach (var ingredient in allIngredients)
                    {
                        _context.DietaryTherapyIngredients.Remove(ingredient);
                    }
                    _context.DietaryTherapies.Remove(therapy);
                }
                return;
            }

            foreach (var dto in dietaryTherapyDtos)
            {
                DietaryTherapy? therapy = null;
                if (dto.Id > 0)
                {
                    therapy = treatment.DietaryTherapies.FirstOrDefault(dt => dt.Id == dto.Id);
                }

                if (therapy == null)
                {
                    therapy = new DietaryTherapy
                    {
                        TreatmentId = treatment.Id,
                        Treatment = treatment
                    };
                    treatment.DietaryTherapies.Add(therapy);
                }

                therapy.Name = dto.Name ?? string.Empty;
                therapy.Category = dto.Category ?? string.Empty;
                therapy.Description = dto.Description ?? string.Empty;
                therapy.Preparation = dto.Preparation ?? string.Empty;
                therapy.Efficacy = dto.Efficacy ?? string.Empty;
                therapy.SuitableFor = dto.SuitableFor ?? string.Empty;
                therapy.Contraindications = dto.Contraindications ?? string.Empty;
                therapy.ServingMethod = dto.ServingMethod ?? string.Empty;
                therapy.StorageMethod = dto.StorageMethod ?? string.Empty;

                // 更新食材列表
                if (dto.Ingredients == null || !dto.Ingredients.Any())
                {
                    // 如果DTO中Ingredients为空，删除所有现有食材
                    var allIngredients = therapy.DietaryTherapyIngredients.ToList();
                    foreach (var ingredient in allIngredients)
                    {
                        _context.DietaryTherapyIngredients.Remove(ingredient);
                    }
                }
                else
                {
                    foreach (var ingredientDto in dto.Ingredients)
                    {
                        DietaryTherapyIngredient? ingredient = null;
                        if (ingredientDto.Id > 0)
                        {
                            ingredient = therapy.DietaryTherapyIngredients.FirstOrDefault(i => i.Id == ingredientDto.Id);
                        }

                        if (ingredient == null)
                        {
                            ingredient = new DietaryTherapyIngredient
                            {
                                DietaryTherapy = therapy
                            };
                            therapy.DietaryTherapyIngredients.Add(ingredient);
                        }

                        ingredient.Name = ingredientDto.Name ?? string.Empty;
                        ingredient.Dosage = ingredientDto.Dosage ?? string.Empty;
                        ingredient.ProcessingMethod = ingredientDto.ProcessingMethod ?? string.Empty;
                        ingredient.Notes = ingredientDto.Notes ?? string.Empty;
                    }

                    var dtoIngredientIds = dto.Ingredients.Where(i => i.Id > 0).Select(i => i.Id).ToList();
                    var ingredientsToRemove = therapy.DietaryTherapyIngredients
                        .Where(i => i.Id > 0 && !dtoIngredientIds.Contains(i.Id))
                        .ToList();
                    foreach (var ingredient in ingredientsToRemove)
                    {
                        _context.DietaryTherapyIngredients.Remove(ingredient);
                    }
                }
            }

            var dtoIds = dietaryTherapyDtos.Where(dt => dt.Id > 0).Select(dt => dt.Id).ToList();
            var toRemove = treatment.DietaryTherapies
                .Where(dt => dt.Id > 0 && !dtoIds.Contains(dt.Id))
                .ToList();
            foreach (var item in toRemove)
            {
                // 先删除所有食材
                var allIngredients = item.DietaryTherapyIngredients.ToList();
                foreach (var ingredient in allIngredients)
                {
                    _context.DietaryTherapyIngredients.Remove(ingredient);
                }
                _context.DietaryTherapies.Remove(item);
            }
        }

        /// <summary>
        /// 更新生活方式建议
        /// </summary>
        private async Task UpdateLifestyleAdvicesAsync(Treatment treatment, List<LifestyleAdviceDto> lifestyleAdviceDtos)
        {
            // 如果DTO列表为空或null，删除所有现有记录
            if (lifestyleAdviceDtos == null || !lifestyleAdviceDtos.Any())
            {
                var allAdvices = treatment.LifestyleAdvices.ToList();
                foreach (var advice in allAdvices)
                {
                    _context.LifestyleAdvices.Remove(advice);
                }
                return;
            }

            foreach (var dto in lifestyleAdviceDtos)
            {
                LifestyleAdvice? advice = null;
                if (dto.Id > 0)
                {
                    advice = treatment.LifestyleAdvices.FirstOrDefault(la => la.Id == dto.Id);
                }

                if (advice == null)
                {
                    advice = new LifestyleAdvice
                    {
                        TreatmentId = treatment.Id,
                        Treatment = treatment
                    };
                    treatment.LifestyleAdvices.Add(advice);
                }

                advice.Category = dto.Category ?? string.Empty;
                advice.Title = dto.Title ?? string.Empty;
                advice.Content = dto.Content ?? string.Empty;
                advice.Rationale = dto.Rationale ?? string.Empty;
                advice.Implementation = dto.Implementation ?? string.Empty;
                advice.Frequency = dto.Frequency ?? string.Empty;
                advice.Precautions = dto.Precautions ?? string.Empty;
                advice.Benefits = dto.Benefits ?? string.Empty;
            }

            var dtoIds = lifestyleAdviceDtos.Where(la => la.Id > 0).Select(la => la.Id).ToList();
            var toRemove = treatment.LifestyleAdvices
                .Where(la => la.Id > 0 && !dtoIds.Contains(la.Id))
                .ToList();
            foreach (var item in toRemove)
            {
                _context.LifestyleAdvices.Remove(item);
            }
        }

        /// <summary>
        /// 更新饮食建议
        /// </summary>
        private async Task UpdateDietaryAdvicesAsync(Treatment treatment, List<DietaryAdviceDto> dietaryAdviceDtos)
        {
            // 如果DTO列表为空或null，删除所有现有记录
            if (dietaryAdviceDtos == null || !dietaryAdviceDtos.Any())
            {
                var allAdvices = treatment.DietaryAdvices.ToList();
                foreach (var advice in allAdvices)
                {
                    // 先删除所有推荐和避免食物
                    var allRecommendedFoods = advice.RecommendedFoods.ToList();
                    foreach (var food in allRecommendedFoods)
                    {
                        _context.RecommendedFoods.Remove(food);
                    }
                    var allAvoidedFoods = advice.AvoidedFoods.ToList();
                    foreach (var food in allAvoidedFoods)
                    {
                        _context.AvoidedFoods.Remove(food);
                    }
                    _context.DietaryAdvices.Remove(advice);
                }
                return;
            }

            foreach (var dto in dietaryAdviceDtos)
            {
                DietaryAdvice? advice = null;
                if (dto.Id > 0)
                {
                    advice = treatment.DietaryAdvices.FirstOrDefault(da => da.Id == dto.Id);
                }

                if (advice == null)
                {
                    advice = new DietaryAdvice
                    {
                        TreatmentId = treatment.Id,
                        Treatment = treatment
                    };
                    treatment.DietaryAdvices.Add(advice);
                }

                advice.Category = dto.Category ?? string.Empty;
                advice.Title = dto.Title ?? string.Empty;
                advice.DietaryPrinciples = dto.DietaryPrinciples ?? string.Empty;
                advice.MealTiming = dto.MealTiming ?? string.Empty;
                advice.CookingMethods = dto.CookingMethods ?? string.Empty;
                advice.Rationale = dto.Rationale ?? string.Empty;
                advice.SeasonalAdjustment = dto.SeasonalAdjustment ?? string.Empty;
                advice.Precautions = dto.Precautions ?? string.Empty;

                // 更新推荐食物
                if (dto.RecommendedFoods == null || !dto.RecommendedFoods.Any())
                {
                    // 如果DTO中RecommendedFoods为空，删除所有现有推荐食物
                    var allFoods = advice.RecommendedFoods.ToList();
                    foreach (var food in allFoods)
                    {
                        _context.RecommendedFoods.Remove(food);
                    }
                }
                else
                {
                    foreach (var foodDto in dto.RecommendedFoods)
                    {
                        RecommendedFood? food = null;
                        if (foodDto.Id > 0)
                        {
                            food = advice.RecommendedFoods.FirstOrDefault(rf => rf.Id == foodDto.Id);
                        }

                        if (food == null)
                        {
                            food = new RecommendedFood
                            {
                                DietaryAdvice = advice
                            };
                            advice.RecommendedFoods.Add(food);
                        }

                        food.FoodName = foodDto.FoodName ?? string.Empty;
                    }

                    var dtoFoodIds = dto.RecommendedFoods.Where(f => f.Id > 0).Select(f => f.Id).ToList();
                    var foodsToRemove = advice.RecommendedFoods
                        .Where(f => f.Id > 0 && !dtoFoodIds.Contains(f.Id))
                        .ToList();
                    foreach (var food in foodsToRemove)
                    {
                        _context.RecommendedFoods.Remove(food);
                    }
                }

                // 更新避免食物
                if (dto.AvoidedFoods == null || !dto.AvoidedFoods.Any())
                {
                    // 如果DTO中AvoidedFoods为空，删除所有现有避免食物
                    var allFoods = advice.AvoidedFoods.ToList();
                    foreach (var food in allFoods)
                    {
                        _context.AvoidedFoods.Remove(food);
                    }
                }
                else
                {
                    foreach (var foodDto in dto.AvoidedFoods)
                    {
                        AvoidedFood? food = null;
                        if (foodDto.Id > 0)
                        {
                            food = advice.AvoidedFoods.FirstOrDefault(af => af.Id == foodDto.Id);
                        }

                        if (food == null)
                        {
                            food = new AvoidedFood
                            {
                                DietaryAdvice = advice
                            };
                            advice.AvoidedFoods.Add(food);
                        }

                        food.FoodName = foodDto.FoodName ?? string.Empty;
                    }

                    var dtoFoodIds = dto.AvoidedFoods.Where(f => f.Id > 0).Select(f => f.Id).ToList();
                    var foodsToRemove = advice.AvoidedFoods
                        .Where(f => f.Id > 0 && !dtoFoodIds.Contains(f.Id))
                        .ToList();
                    foreach (var food in foodsToRemove)
                    {
                        _context.AvoidedFoods.Remove(food);
                    }
                }
            }

            var dtoIds = dietaryAdviceDtos.Where(da => da.Id > 0).Select(da => da.Id).ToList();
            var toRemove = treatment.DietaryAdvices
                .Where(da => da.Id > 0 && !dtoIds.Contains(da.Id))
                .ToList();
            foreach (var item in toRemove)
            {
                // 先删除所有推荐和避免食物
                var allRecommendedFoods = item.RecommendedFoods.ToList();
                foreach (var food in allRecommendedFoods)
                {
                    _context.RecommendedFoods.Remove(food);
                }
                var allAvoidedFoods = item.AvoidedFoods.ToList();
                foreach (var food in allAvoidedFoods)
                {
                    _context.AvoidedFoods.Remove(food);
                }
                _context.DietaryAdvices.Remove(item);
            }
        }

        /// <summary>
        /// 更新随访建议
        /// </summary>
        private async Task UpdateFollowUpAdvicesAsync(Treatment treatment, List<FollowUpAdviceDto> followUpAdviceDtos)
        {
            // 如果DTO列表为空或null，删除所有现有记录
            if (followUpAdviceDtos == null || !followUpAdviceDtos.Any())
            {
                var allAdvices = treatment.FollowUpAdvices.ToList();
                foreach (var advice in allAdvices)
                {
                    // 先删除所有监测指标
                    var allIndicators = advice.MonitoringIndicators.ToList();
                    foreach (var indicator in allIndicators)
                    {
                        _context.MonitoringIndicators.Remove(indicator);
                    }
                    _context.FollowUpAdvices.Remove(advice);
                }
                return;
            }

            foreach (var dto in followUpAdviceDtos)
            {
                FollowUpAdvice? advice = null;
                if (dto.Id > 0)
                {
                    advice = treatment.FollowUpAdvices.FirstOrDefault(fa => fa.Id == dto.Id);
                }

                if (advice == null)
                {
                    advice = new FollowUpAdvice
                    {
                        TreatmentId = treatment.Id,
                        Treatment = treatment
                    };
                    treatment.FollowUpAdvices.Add(advice);
                }

                advice.FollowUpType = dto.FollowUpType ?? string.Empty;
                advice.Title = dto.Title ?? string.Empty;
                advice.Timing = dto.Timing ?? string.Empty;
                advice.Purpose = dto.Purpose ?? string.Empty;
                advice.PreparationRequired = dto.PreparationRequired ?? string.Empty;
                advice.EmergencyConditions = dto.EmergencyConditions ?? string.Empty;
                advice.SelfMonitoring = dto.SelfMonitoring ?? string.Empty;
                advice.ContactInformation = dto.ContactInformation ?? string.Empty;

                // 更新监测指标
                if (dto.MonitoringIndicators == null || !dto.MonitoringIndicators.Any())
                {
                    // 如果DTO中MonitoringIndicators为空，删除所有现有监测指标
                    var allIndicators = advice.MonitoringIndicators.ToList();
                    foreach (var indicator in allIndicators)
                    {
                        _context.MonitoringIndicators.Remove(indicator);
                    }
                }
                else
                {
                    foreach (var indicatorDto in dto.MonitoringIndicators)
                    {
                        MonitoringIndicator? indicator = null;
                        if (indicatorDto.Id > 0)
                        {
                            indicator = advice.MonitoringIndicators.FirstOrDefault(mi => mi.Id == indicatorDto.Id);
                        }

                        if (indicator == null)
                        {
                            indicator = new MonitoringIndicator
                            {
                                FollowUpAdvice = advice
                            };
                            advice.MonitoringIndicators.Add(indicator);
                        }

                        indicator.IndicatorName = indicatorDto.IndicatorName ?? string.Empty;
                    }

                    var dtoIndicatorIds = dto.MonitoringIndicators.Where(i => i.Id > 0).Select(i => i.Id).ToList();
                    var indicatorsToRemove = advice.MonitoringIndicators
                        .Where(i => i.Id > 0 && !dtoIndicatorIds.Contains(i.Id))
                        .ToList();
                    foreach (var indicator in indicatorsToRemove)
                    {
                        _context.MonitoringIndicators.Remove(indicator);
                    }
                }
            }

            var dtoIds = followUpAdviceDtos.Where(fa => fa.Id > 0).Select(fa => fa.Id).ToList();
            var toRemove = treatment.FollowUpAdvices
                .Where(fa => fa.Id > 0 && !dtoIds.Contains(fa.Id))
                .ToList();
            foreach (var item in toRemove)
            {
                // 先删除所有监测指标
                var allIndicators = item.MonitoringIndicators.ToList();
                foreach (var indicator in allIndicators)
                {
                    _context.MonitoringIndicators.Remove(indicator);
                }
                _context.FollowUpAdvices.Remove(item);
            }
        }

        /// <summary>
        /// 更新治疗方案状态（安全检查通过/失败等）
        /// </summary>
        public async Task UpdateTreatmentStatusAsync(int treatmentId, Entities.Enums.TreatmentStatus newStatus, int? userId = null)
        {
            try
            {
                var treatment = await _context.Treatments.FirstOrDefaultAsync(t => t.Id == treatmentId);
                if (treatment == null)
                {
                    throw new InvalidOperationException($"治疗方案 {treatmentId} 不存在");
                }

                treatment.Status = newStatus;
                treatment.UpdatedAt = DateTime.UtcNow;
                if (userId.HasValue) treatment.UpdatedByUserId = userId.Value;

                await _context.SaveChangesAsync();
                _logger.LogInformation("已更新治疗方案 {TreatmentId} 状态为 {Status}", treatmentId, newStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新治疗方案 {TreatmentId} 状态时发生错误", treatmentId);
                throw;
            }
        }

        /// <summary>
        /// 异步生成并保存AI治疗方案 - 核心业务方法
        /// 实现高性能并发控制，采用"先检查，后锁定"(Check-Then-Lock)策略
        /// </summary>
        /// <param name="syndromeId">证候ID</param>
        /// <returns>异步任务</returns>
        /// <exception cref="ArgumentException">当证候ID无效时抛出</exception>
        /// <exception cref="InvalidOperationException">当证候或就诊数据无效时抛出</exception>
        public async Task GenerateAndSaveAiTreatmentAsync(int syndromeId)
        {
            if (syndromeId <= 0)
            {
                throw new ArgumentException("证候ID必须大于0", nameof(syndromeId));
            }

            try
            {
                _logger.LogInformation("开始为证候 {SyndromeId} 生成AI治疗方案", syndromeId);

                // 步骤 1: 快速检查 - 避免不必要的锁竞争
                var existingTreatment = await _context.Treatments
                    .Where(t => t.SyndromeId == syndromeId && t.IsLatest)
                    .FirstOrDefaultAsync();

                // 如果已存在最新版本，需要生成新版本（主版本递增）
                string newVersion = "V1.0.0";
                if (existingTreatment != null)
                {
                    // 标记所有同证候ID的最新版本为非最新（确保数据一致性）
                    var latestTreatments = await _context.Treatments
                        .Where(t => t.SyndromeId == syndromeId && t.IsLatest)
                        .ToListAsync();
                    
                    foreach (var treatment in latestTreatments)
                    {
                        treatment.IsLatest = false;
                    }
                    
                    await _context.SaveChangesAsync();
                    // 生成新版本号（主版本+1）
                    newVersion = GenerateNextVersion(existingTreatment.Version, VersionIncrementType.Major);
                    _logger.LogInformation("证候 {SyndromeId} 的治疗方案已存在，将创建新版本 {NewVersion}，原版本: {OldVersion}，已标记 {Count} 个旧版本为非最新",
                        syndromeId, newVersion, existingTreatment.Version, latestTreatments.Count);
                }

                // 步骤 2: 验证基础数据
                var syndrome = await _context.Syndromes
                    .Include(s => s.Visit)
                        .ThenInclude(v => v.Series)
                            .ThenInclude(vs => vs.PatientUser)
                    .FirstOrDefaultAsync(s => s.SyndromeId == syndromeId);

                if (syndrome == null)
                {
                    throw new SyndromeNotFoundException(syndromeId);
                }

                if (syndrome.VisitId <= 0)
                {
                    throw new InvalidSyndromeDataException(syndromeId, "未关联有效的就诊记录");
                }

                var visit = syndrome.Visit;
                if (visit?.Series?.PatientUser == null)
                {
                    throw new InvalidSyndromeDataException(syndromeId, "关联的就诊或患者信息不完整");
                }

                // 步骤 2.5: 验证输入数据
                _validator.ValidateInputData(syndrome, visit, visit.Series.PatientUser);

                // 步骤 2.6: 获取当前登录用户
                var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User);
                if (currentUser == null)
                {
                    throw new InvalidOperationException("未获取到当前用户信息，请确保用户已登录");
                }

                // 步骤 3: 尝试创建占位符 (获取分布式锁)
                var placeholder = new Treatment
                {
                    SyndromeId = syndromeId,
                    VisitId = visit.VisitId,
                    PatientId = visit.Series.PatientUserId,
                    Status = TreatmentStatus.Generating,
                    IsLatest = true,
                    IsAiOriginated = true,
                    Version = newVersion, // 使用计算出的新版本号
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = currentUser.Id,
                    TenantId = visit.TenantId,
                    TcmDiagnosis = string.Empty,
                    SyndromeAnalysis = string.Empty,
                    TreatmentPrinciple = string.Empty,
                    ExpectedOutcome = string.Empty,
                    Precautions = string.Empty
                };

                _context.Treatments.Add(placeholder);

                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("成功创建证候 {SyndromeId} 的治疗方案占位符，获得生成锁", syndromeId);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogInformation("证候 {SyndromeId} 治疗方案生成锁竞争，本次请求跳过。异常: {Exception}",
                        syndromeId, ex.Message);
                    throw new ConcurrencyLockException(syndromeId);
                }

                // 步骤 4: 成功获取锁，执行耗时的AI生成操作
                try
                {
                    _logger.LogInformation("开始调用Dify API为证候 {SyndromeId} 生成治疗方案", syndromeId);

                    // 准备API请求数据
                    var requestInputs = await PrepareDifyInputsAsync(syndrome, visit.Series.PatientUser, visit);

                    // 验证API输入数据
                    _validator.ValidateApiInputs(requestInputs);

                    // 调用Dify API
                    var apiResponse = await CallDifyTreatmentApiAsync(requestInputs, visit.Series.PatientUser.PhoneNumber);

                    // 验证并解析API响应
                    var validatedResponse = _validator.ValidateAndParseApiResponse(apiResponse);

                    // 解析API响应并更新占位符
                    await UpdateTreatmentFromApiResponseAsync(placeholder, apiResponse);

                    // 验证最终的治疗方案实体
                    _validator.ValidateTreatmentEntity(placeholder);

                    _logger.LogInformation("成功为证候 {SyndromeId} 生成AI治疗方案", syndromeId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "为证候 {SyndromeId} 生成AI治疗方案时发生错误", syndromeId);
                    _logger.LogWarning("将为证候 {SyndromeId} 使用本地 fallback 治疗方案", syndromeId);
                    await PopulateFallbackTreatmentAsync(placeholder, syndrome, visit);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成证候 {SyndromeId} 的AI治疗方案过程中发生未处理的错误", syndromeId);
                throw;
            }
        }

        /// <summary>
        /// 准备Dify API请求的输入数据
        /// 根据开发计划文档要求，构建包含证候信息、患者信息、就诊描述和证候详情的输入对象
        /// </summary>
        /// <param name="syndrome">证候实体</param>
        /// <param name="patientUser">患者用户实体</param>
        /// <param name="visit">就诊实体</param>
        /// <returns>Dify API输入字典</returns>
        private async Task<Dictionary<string, string>> PrepareDifyInputsAsync(Syndrome syndrome, User patientUser, Visit visit)
        {
            try
            {
                _logger.LogInformation("开始准备证候 {SyndromeId} 的Dify API输入数据", syndrome.SyndromeId);

                // 1. 构建确证证候信息
                var confirmedSyndrome = $"证候名称：{syndrome.SyndromeName}\n" +
                                       $"诊断置信度：{syndrome.Confidence:F1}%\n" +
                                       $"主要症状：{JsonSerializer.Serialize(syndrome.MainSymptoms?.Split(',') ?? new string[0])}\n" +
                                       $"治疗原则：{syndrome.TreatmentPrinciple ?? "待确定"}";

                // 2. 构建患者信息 - 使用UserInfoFormatter格式化
                var patientInfo = UserInfoFormatter.ToTextDescription(patientUser, patientUser.Detail);

                // 3. 构建就诊描述
                var visitDescription = $"当次就诊/回访（第{visit.Sequence}次）\n" +
                                     $"就诊日期: {visit.VisitDate:yyyy-MM-dd HH:mm}\n" +
                                     $"主诉: {visit.ChiefComplaint ?? "无"}\n" +
                                     $"伴随症状: {visit.AccompanyingSymptoms ?? "无"}\n" +
                                     $"既往治疗效果: {visit.PreviousTreatmentEffect ?? "无"}\n" +
                                     $"舌质: {visit.TongueQuality ?? "无"} 舌苔: {visit.TongueCoating ?? "无"}\n" +
                                     $"脉象: {visit.PulseType ?? "无"} {visit.PulseFeatures ?? ""}";

                // 4. 构建证候详情 - 如果有详细信息的话
                var syndromeDetail = $"病机分析：{syndrome.PathogenesisAnalysis ?? "脾胃阳气不足，寒邪内生"}\n" +
                                   $"推荐治疗方法：{syndrome.TreatmentPrinciple ?? "温中散寒、健脾益胃"}";

                var inputs = new Dictionary<string, string>
                {
                    ["confirmed_syndrome"] = confirmedSyndrome,
                    ["patient_info"] = patientInfo,
                    ["visit_description"] = visitDescription,
                    ["syndrome_detail"] = syndromeDetail
                };

                _logger.LogInformation("成功准备证候 {SyndromeId} 的Dify API输入数据，包含 {InputCount} 个字段",
                    syndrome.SyndromeId, inputs.Count);

                return inputs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "准备证候 {SyndromeId} 的Dify API输入数据时发生错误", syndrome.SyndromeId);
                throw;
            }
        }

        /// <summary>
        /// 调用Dify治疗方案生成API
        /// 实现带重试机制的HTTP请求和超时处理
        /// </summary>
        /// <param name="inputs">API输入数据</param>
        /// <param name="userPhoneNumber">用户手机号</param>
        /// <returns>API响应字符串</returns>
        /// <exception cref="DifyApiException">当API调用失败时抛出</exception>
        private async Task<string> CallDifyTreatmentApiAsync(Dictionary<string, string> inputs, string? userPhoneNumber)
        {
            try
            {
                _logger.LogInformation("开始调用Dify治疗方案生成API（带重试机制）");

                // 使用重试策略执行API调用
                var responseContent = await _retryPolicyService.ExecuteWithRetryAsync(
                    async () =>
                    {
                        using var httpClient = _retryPolicyService.CreateHttpClientWithTimeout(
                            _httpClientFactory, _difyApiOptions.TimeoutSeconds);

                        // 构建请求体
                        var requestData = new
                        {
                            inputs = inputs,
                            response_mode = _difyApiOptions.ResponseMode,
                            user = userPhoneNumber ?? _difyApiOptions.User
                        };

                        var json = JsonSerializer.Serialize(requestData, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        // 设置请求头
                        httpClient.DefaultRequestHeaders.Clear();
                        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_difyApiOptions.TreatmentWorkflowApiKey}");

                        _logger.LogInformation("发送Dify API请求，URL: {Url}", _difyApiOptions.TreatmentUrl);

                        return await httpClient.PostAsync(_difyApiOptions.TreatmentUrl, content);
                    },
                    maxRetries: 3,
                    baseDelayMs: 1000,
                    operationName: "Dify治疗方案生成API调用"
                );

                _logger.LogInformation("Dify API调用成功，响应长度: {ResponseLength} 字符", responseContent.Length);
                return responseContent;
            }
            catch (DifyApiException)
            {
                // 重新抛出Dify API异常
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调用Dify治疗方案生成API时发生未预期的错误");
                throw new DifyApiException("调用Dify API时发生未知错误", ex);
            }
        }

        /// <summary>
        /// 解析Dify API响应并更新治疗方案实体
        /// 根据输入输出文档的JSON Schema解析响应数据
        /// </summary>
        /// <param name="treatment">治疗方案实体</param>
        /// <param name="apiResponse">API响应字符串</param>
        /// <returns>异步任务</returns>
        /// <exception cref="JsonException">当JSON解析失败时抛出</exception>
        /// <exception cref="InvalidOperationException">当响应数据格式无效时抛出</exception>
        private async Task UpdateTreatmentFromApiResponseAsync(Treatment treatment, string apiResponse)
        {
            try
            {
                _logger.LogInformation("开始解析Dify API响应并更新治疗方案 {TreatmentId}", treatment.Id);

                // 解析API响应
                var apiResponseObj = JsonSerializer.Deserialize<JsonElement>(apiResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                // 获取outputs字段
                if (!apiResponseObj.TryGetProperty("data", out var dataElement) ||
                    !dataElement.TryGetProperty("outputs", out var outputsElement))
                {
                    throw new ApiResponseParseException("API响应格式无效：缺少data.outputs字段", apiResponse);
                }

                // // 尝试获取治疗方案数据
                // JsonElement treatmentData;
                // string? foundFieldName = null;
                // // 根据API文档JsonSchema定义的字段名称
                // var possibleFieldNames = new[] { "herbal_prescriptions", "acupuncture_points", "lifestyle_advice", "dietary_advice", "follow_up_advice", "treatment", "treatment_plan", "result", "output", "data" };

                // foreach (var fieldName in possibleFieldNames)
                // {
                //     if (outputsElement.TryGetProperty(fieldName, out treatmentData))
                //     {
                //         foundFieldName = fieldName;
                //         _logger.LogInformation("找到治疗方案数据字段: '{FieldName}'", fieldName);
                //         break;
                //     }
                // }

                // if (foundFieldName == null)
                // {
                //     throw new ApiResponseParseException("API响应中未找到治疗方案数据", apiResponse);
                // }

                // treatmentData = outputsElement.GetProperty(foundFieldName);

                // // 如果是字符串，尝试解析为JSON
                // if (treatmentData.ValueKind == JsonValueKind.String)
                // {
                //     var jsonString = treatmentData.GetString();
                //     if (string.IsNullOrWhiteSpace(jsonString))
                //     {
                //         throw new ApiResponseParseException("治疗方案数据为空", apiResponse);
                //     }

                //     treatmentData = JsonSerializer.Deserialize<JsonElement>(jsonString);
                // }

                // 解析治疗方案数据并更新实体
                // await ParseAndUpdateTreatmentDataAsync(treatment, treatmentData);
                await ParseAndUpdateTreatmentDataAsync(treatment, outputsElement);

                // 更新治疗方案状态
                treatment.Status = TreatmentStatus.AIGenerated;
                treatment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("成功解析并更新治疗方案 {TreatmentId}", treatment.Id);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "解析Dify API响应JSON时发生错误，响应内容: {ApiResponse}",
                    apiResponse.Length > 1000 ? apiResponse.Substring(0, 1000) + "..." : apiResponse);
                throw new ApiResponseParseException("API响应JSON格式无效", apiResponse, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新治疗方案 {TreatmentId} 时发生错误", treatment.Id);
                throw new TreatmentGenerationFailedException(treatment.SyndromeId, "更新治疗方案数据时发生错误", ex);
            }
        }

        /// <summary>
        /// 解析治疗方案数据并更新实体
        /// 根据输入输出文档的JSON Schema解析九大治疗模块的数据
        /// </summary>
        /// <param name="treatment">治疗方案实体</param>
        /// <param name="treatmentData">治疗方案JSON数据</param>
        /// <returns>异步任务</returns>
        private async Task ParseAndUpdateTreatmentDataAsync(Treatment treatment, JsonElement treatmentData)
        {
            try
            {
                _logger.LogInformation("开始解析治疗方案数据，包含九大治疗模块");

                // 1. 解析中药处方 (herbal_prescriptions)
                if (treatmentData.TryGetProperty("herbal_prescriptions", out var prescriptionsElement) ||
                    treatmentData.TryGetProperty("herbalPrescriptions", out prescriptionsElement))
                {
                    await ParsePrescriptionsAsync(treatment, prescriptionsElement);
                }

                // 2. 解析中药安全警告 (herbal_warnings)
                if (treatmentData.TryGetProperty("herbal_warnings", out var herbalWarningsElement) ||
                    treatmentData.TryGetProperty("herbalWarnings", out herbalWarningsElement))
                {
                    await ParseHerbalWarningsAsync(treatment, herbalWarningsElement);
                }

                // 3. 解析食疗方案 (dietary_therapies)
                if (treatmentData.TryGetProperty("dietary_therapies", out var dietaryTherapiesElement) ||
                    treatmentData.TryGetProperty("dietaryTherapies", out dietaryTherapiesElement))
                {
                    await ParseDietaryTherapiesAsync(treatment, dietaryTherapiesElement);
                }

                // 4. 解析针灸穴位 (acupuncture_points)
                if (treatmentData.TryGetProperty("acupuncture_points", out var acupunctureElement) ||
                    treatmentData.TryGetProperty("acupuncturePoints", out acupunctureElement))
                {
                    await ParseAcupunctureAsync(treatment, acupunctureElement);
                }

                // 5. 解析艾灸穴位 (moxibustion_points)
                if (treatmentData.TryGetProperty("moxibustion_points", out var moxibustionElement) ||
                    treatmentData.TryGetProperty("moxibustionPoints", out moxibustionElement))
                {
                    await ParseMoxibustionAsync(treatment, moxibustionElement);
                }

                // 6. 解析拔罐部位 (cupping_points)
                if (treatmentData.TryGetProperty("cupping_points", out var cuppingElement) ||
                    treatmentData.TryGetProperty("cuppingPoints", out cuppingElement))
                {
                    await ParseCuppingAsync(treatment, cuppingElement);
                }

                // 7. 解析生活方式建议 (lifestyle_advice)
                if (treatmentData.TryGetProperty("lifestyle_advice", out var lifestyleElement) ||
                    treatmentData.TryGetProperty("lifestyleAdvice", out lifestyleElement))
                {
                    await ParseLifestyleAdviceAsync(treatment, lifestyleElement);
                }

                // 8. 解析饮食建议 (dietary_advice)
                if (treatmentData.TryGetProperty("dietary_advice", out var dietaryAdviceElement) ||
                    treatmentData.TryGetProperty("dietaryAdvice", out dietaryAdviceElement))
                {
                    await ParseDietaryAdviceAsync(treatment, dietaryAdviceElement);
                }

                // 9. 解析随访建议 (follow_up_advice)
                if (treatmentData.TryGetProperty("follow_up_advice", out var followUpElement) ||
                    treatmentData.TryGetProperty("followUpAdvice", out followUpElement))
                {
                    await ParseFollowUpAdviceAsync(treatment, followUpElement);
                }

                _logger.LogInformation("成功解析治疗方案数据的所有模块");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解析治疗方案数据时发生错误");
                throw;
            }
        }

        private async Task PopulateFallbackTreatmentAsync(Treatment treatment, Syndrome syndrome, Visit visit)
        {
            treatment.TcmDiagnosis = string.IsNullOrWhiteSpace(syndrome.SyndromeName)
                ? "中医辨证治疗方案"
                : $"{syndrome.SyndromeName}调理方案";
            treatment.SyndromeAnalysis = !string.IsNullOrWhiteSpace(syndrome.PathogenesisAnalysis)
                ? syndrome.PathogenesisAnalysis
                : "基于当前证候与症状，先采用辨证调理、饮食干预和随访观察的保守治疗路径。";
            treatment.TreatmentPrinciple = !string.IsNullOrWhiteSpace(syndrome.TreatmentPrinciple)
                ? syndrome.TreatmentPrinciple
                : "辨证施治，调和脏腑，改善主症。";
            treatment.ExpectedOutcome = "缓解主症，观察一周至两周内症状变化，并根据复诊反馈动态调整方案。";
            treatment.Precautions = "当前为本地 fallback 方案，请结合患者实际情况、既往病史和复诊结果进行人工确认。";

            treatment.Prescriptions.Clear();
            treatment.Acupunctures.Clear();
            treatment.Moxibustions.Clear();
            treatment.Cuppings.Clear();
            treatment.DietaryTherapies.Clear();
            treatment.LifestyleAdvices.Clear();
            treatment.DietaryAdvices.Clear();
            treatment.FollowUpAdvices.Clear();
            treatment.HerbalWarnings.Clear();

            var prescription = new Prescription
            {
                Treatment = treatment,
                Name = string.IsNullOrWhiteSpace(syndrome.SyndromeName) ? "辨证基础方" : $"{syndrome.SyndromeName}基础方",
                Category = "本地 fallback",
                Description = "用于本机调试和流程验证的示例处方，请医生结合临床实际调整。",
                Usage = "每日一剂，分早晚温服，连用 3-5 天后复诊评估。",
                Efficacy = treatment.TreatmentPrinciple,
                Indications = string.IsNullOrWhiteSpace(visit.ChiefComplaint) ? "改善当前主要不适" : visit.ChiefComplaint,
                Contraindications = "孕产妇、儿童及合并严重基础疾病者需谨慎评估。",
                Notes = "当前处方为示例内容，用于本地预览和确认流程。",
                PatientFriendlyName = "调理基础方",
                PatientFriendlyDescription = "帮助缓解当前主要不适的基础中药方案。"
            };
            prescription.PrescriptionItems.Add(new PrescriptionItem
            {
                Prescription = prescription,
                Name = "党参",
                Dosage = "10",
                Unit = "g",
                ProcessingMethod = "切片",
                Notes = "补气健脾"
            });
            prescription.PrescriptionItems.Add(new PrescriptionItem
            {
                Prescription = prescription,
                Name = "白术",
                Dosage = "10",
                Unit = "g",
                ProcessingMethod = "炒制",
                Notes = "健脾祛湿"
            });
            prescription.PrescriptionItems.Add(new PrescriptionItem
            {
                Prescription = prescription,
                Name = "茯苓",
                Dosage = "12",
                Unit = "g",
                ProcessingMethod = "生用",
                Notes = "健脾渗湿"
            });
            treatment.Prescriptions.Add(prescription);

            treatment.Acupunctures.Add(new Acupuncture
            {
                Treatment = treatment,
                PointName = "足三里",
                Location = "小腿前外侧",
                Method = "常规针刺",
                Technique = "平补平泻",
                NeedleSpecification = "0.25mm x 40mm",
                Depth = "0.8-1.2寸",
                Duration = "20分钟",
                Frequency = "隔日一次",
                Efficacy = "调理脾胃，扶正培元",
                Indications = string.IsNullOrWhiteSpace(visit.ChiefComplaint) ? "体虚乏力、纳差" : visit.ChiefComplaint,
                Instructions = "局部常规消毒后进针。",
                Notes = "饭后半小时内不建议针刺。",
                Contraindications = "局部皮肤破损者慎用。"
            });

            treatment.Moxibustions.Add(new Moxibustion
            {
                Treatment = treatment,
                PointName = "关元",
                Location = "下腹部正中线",
                Method = "温和灸",
                MoxaType = "艾条",
                Technique = "悬灸",
                TemperatureControl = "局部温热舒适",
                Duration = "10-15分钟",
                Frequency = "每日一次",
                CourseDuration = "5天",
                Efficacy = "温阳散寒，调和气血",
                Indications = "畏寒、乏力、恢复期调养",
                TechniquePoints = "保持皮肤温热，避免烫伤。",
                Precautions = "出现灼痛时及时移开艾条。",
                Contraindications = "发热、局部炎症时暂不建议。",
                PostTreatmentCare = "艾灸后注意保暖，多饮温水。",
                CombinationTherapy = "可配合饮食和作息调理。"
            });

            treatment.Cuppings.Add(new Cupping
            {
                Treatment = treatment,
                Area = "背部膀胱经",
                SpecificPoints = "肺俞、脾俞",
                SuitableFor = "肌肉紧张、疲劳乏力",
                Method = "留罐",
                CupType = "玻璃罐",
                CupSize = "中号",
                SuctionStrength = "中等",
                Duration = "8-10分钟",
                Frequency = "每周2次",
                Efficacy = "疏通经络，缓解疲劳",
                Indications = "肩背紧张、体倦乏力",
                TechniquePoints = "观察皮肤反应，避免时间过长。",
                Precautions = "皮肤敏感或出血倾向者慎用。"
            });

            var dietTherapy = new DietaryTherapy
            {
                Treatment = treatment,
                Name = "山药小米粥",
                Category = "本地 fallback",
                Description = "适合作为恢复期和脾胃调养期的基础食疗。",
                Preparation = "山药切块与小米同煮，煮至软烂即可。",
                Efficacy = "健脾和胃，温和调养",
                SuitableFor = "纳差、乏力、恢复期患者",
                Contraindications = "血糖异常者注意总量控制。",
                ServingMethod = "早餐或晚餐温服",
                StorageMethod = "现做现食为佳",
                PatientFriendlyName = "调理粥"
            };
            dietTherapy.DietaryTherapyIngredients.Add(new DietaryTherapyIngredient
            {
                DietaryTherapy = dietTherapy,
                Name = "山药",
                Dosage = "50g",
                ProcessingMethod = "去皮切块",
                Notes = "健脾"
            });
            dietTherapy.DietaryTherapyIngredients.Add(new DietaryTherapyIngredient
            {
                DietaryTherapy = dietTherapy,
                Name = "小米",
                Dosage = "50g",
                ProcessingMethod = "淘洗",
                Notes = "和胃"
            });
            treatment.DietaryTherapies.Add(dietTherapy);

            treatment.LifestyleAdvices.Add(new LifestyleAdvice
            {
                Treatment = treatment,
                Category = "起居调护",
                Title = "规律作息",
                Content = "尽量在 23:00 前入睡，连续观察睡眠和精神状态变化。",
                Rationale = "稳定作息有助于脏腑功能恢复。",
                Implementation = "保持固定睡眠时间，避免熬夜。",
                Frequency = "每日执行",
                Precautions = "避免过劳和情绪波动过大。",
                Benefits = "改善疲劳感和恢复质量。"
            });

            var dietaryAdvice = new DietaryAdvice
            {
                Treatment = treatment,
                Category = "饮食管理",
                Title = "清淡温软饮食",
                DietaryPrinciples = "少油少辣，温软易消化，避免生冷刺激。",
                MealTiming = "三餐定时，晚餐不过饱。",
                CookingMethods = "蒸、煮、炖为主。",
                Rationale = "减轻脾胃负担，帮助症状恢复。",
                SeasonalAdjustment = "天气转凉时适当增加温热食物比例。",
                Precautions = "合并基础病者按医生建议调整。"
            };
            dietaryAdvice.RecommendedFoods.Add(new RecommendedFood
            {
                DietaryAdvice = dietaryAdvice,
                FoodName = "山药"
            });
            dietaryAdvice.RecommendedFoods.Add(new RecommendedFood
            {
                DietaryAdvice = dietaryAdvice,
                FoodName = "小米"
            });
            dietaryAdvice.AvoidedFoods.Add(new AvoidedFood
            {
                DietaryAdvice = dietaryAdvice,
                FoodName = "辛辣烧烤"
            });
            dietaryAdvice.AvoidedFoods.Add(new AvoidedFood
            {
                DietaryAdvice = dietaryAdvice,
                FoodName = "冰镇饮料"
            });
            treatment.DietaryAdvices.Add(dietaryAdvice);

            var followUpAdvice = new FollowUpAdvice
            {
                Treatment = treatment,
                FollowUpType = "门诊复诊",
                Title = "一周后复诊评估",
                Timing = "7天内",
                Purpose = "评估主症变化、睡眠、饮食和体力恢复情况。",
                PreparationRequired = "记录近一周症状变化和服药情况。",
                EmergencyConditions = "如症状明显加重或出现异常不适，请提前就诊。",
                SelfMonitoring = "观察主症频率、食欲和睡眠。",
                ContactInformation = "按门诊常规方式预约复诊。"
            };
            followUpAdvice.MonitoringIndicators.Add(new MonitoringIndicator
            {
                FollowUpAdvice = followUpAdvice,
                IndicatorName = "主症变化"
            });
            followUpAdvice.MonitoringIndicators.Add(new MonitoringIndicator
            {
                FollowUpAdvice = followUpAdvice,
                IndicatorName = "睡眠质量"
            });
            treatment.FollowUpAdvices.Add(followUpAdvice);

            var herbalWarning = new HerbalWarning
            {
                Treatment = treatment,
                WarningType = "本地 fallback",
                Title = "示例药物安全提醒",
                Content = "当前为本地生成的示例方案，正式使用前请结合过敏史、妊娠情况和基础疾病人工核对。",
                SeverityLevel = "中",
                SymptomsToWatch = "恶心、腹泻、皮疹等不适",
                ActionRequired = "出现明显不适及时停用并复诊",
                PreventionMeasures = "首次使用从小剂量、短疗程开始观察",
                SpecialPopulations = "孕产妇、儿童、老年慢病患者"
            };
            herbalWarning.AffectedMedications.Add(new AffectedMedication
            {
                HerbalWarning = herbalWarning,
                MedicationName = "请结合患者现用西药再次核对"
            });
            treatment.HerbalWarnings.Add(herbalWarning);

            treatment.Status = TreatmentStatus.AIGenerated;
            treatment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("已为证候 {SyndromeId} 生成本地 fallback 治疗方案 {TreatmentId}", syndrome.SyndromeId, treatment.Id);
        }

        /// <summary>
        /// 解析中药处方数据
        /// </summary>
        private async Task ParsePrescriptionsAsync(Treatment treatment, JsonElement prescriptionsElement)
        {
            if (prescriptionsElement.ValueKind != JsonValueKind.Array)
            {
                _logger.LogWarning("中药处方数据格式无效，期望数组类型");
                return;
            }

            foreach (var prescriptionElement in prescriptionsElement.EnumerateArray())
            {
                var prescription = new Prescription
                {
                    TreatmentId = treatment.Id,
                    Name = GetStringProperty(prescriptionElement, "name") ?? "未命名处方",
                    Category = GetStringProperty(prescriptionElement, "category") ?? "汤剂",
                    Description = GetStringProperty(prescriptionElement, "description") ?? string.Empty,
                    Usage = GetStringProperty(prescriptionElement, "usage") ?? string.Empty,
                    Efficacy = GetStringProperty(prescriptionElement, "efficacy") ?? string.Empty,
                    Indications = GetStringProperty(prescriptionElement, "indications") ?? string.Empty,
                    Contraindications = GetStringProperty(prescriptionElement, "contraindications") ?? string.Empty,
                    Notes = GetStringProperty(prescriptionElement, "notes") ?? string.Empty
                };

                // 解析药材列表
                if (prescriptionElement.TryGetProperty("herbs", out var herbsElement) &&
                    herbsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var herbElement in herbsElement.EnumerateArray())
                    {
                        var prescriptionItem = new PrescriptionItem
                        {
                            // 不设置 PrescriptionId，让 EF Core 通过导航属性自动处理
                            Name = GetStringProperty(herbElement, "name") ?? "未知药材",
                            Dosage = GetStringProperty(herbElement, "amount") ?? "适量",
                            Unit = GetStringProperty(herbElement, "unit") ?? "g",
                            ProcessingMethod = GetStringProperty(herbElement, "processing_method") ??
                                             GetStringProperty(herbElement, "processingMethod") ?? "生用",
                            Notes = GetStringProperty(herbElement, "notes") ?? string.Empty,
                            Prescription = prescription
                        };
                        prescription.PrescriptionItems.Add(prescriptionItem);
                    }
                }

                treatment.Prescriptions.Add(prescription);
            }

            _logger.LogInformation("解析了 {Count} 个中药处方", treatment.Prescriptions.Count);
        }

        /// <summary>
        /// 解析中药安全警告数据
        /// </summary>
        private async Task ParseHerbalWarningsAsync(Treatment treatment, JsonElement warningsElement)
        {
            if (warningsElement.ValueKind != JsonValueKind.Array)
            {
                _logger.LogWarning("中药安全警告数据格式无效，期望数组类型");
                return;
            }

            foreach (var warningElement in warningsElement.EnumerateArray())
            {
                var warning = new HerbalWarning
                {
                    TreatmentId = treatment.Id,
                    WarningType = GetStringProperty(warningElement, "warning_type") ??
                                 GetStringProperty(warningElement, "warningType") ?? "一般警告",
                    Title = GetStringProperty(warningElement, "title") ?? "安全提醒",
                    Content = GetStringProperty(warningElement, "content") ?? string.Empty,
                    SeverityLevel = GetStringProperty(warningElement, "severity_level") ??
                               GetStringProperty(warningElement, "severityLevel") ?? "中等",
                    SymptomsToWatch = GetStringProperty(warningElement, "symptoms_to_watch") ??
                                     GetStringProperty(warningElement, "symptomsToWatch") ?? string.Empty,
                    ActionRequired = GetStringProperty(warningElement, "action_required") ??
                                    GetStringProperty(warningElement, "actionRequired") ?? string.Empty,
                    PreventionMeasures = GetStringProperty(warningElement, "prevention_measures") ??
                                        GetStringProperty(warningElement, "preventionMeasures") ?? string.Empty,
                    SpecialPopulations = GetStringProperty(warningElement, "special_populations") ??
                                        GetStringProperty(warningElement, "specialPopulations") ?? string.Empty
                };

                // 解析涉及药物列表
                if (warningElement.TryGetProperty("affected_medications", out var medicationsElement) ||
                    warningElement.TryGetProperty("affectedMedications", out medicationsElement))
                {
                    if (medicationsElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var medicationElement in medicationsElement.EnumerateArray())
                        {
                            if (medicationElement.ValueKind == JsonValueKind.String)
                            {
                                var affectedMedication = new AffectedMedication
                                {
                                    // 不设置 HerbalWarningId，让 EF Core 通过导航属性自动处理
                                    MedicationName = medicationElement.GetString() ?? "未知药物",
                                    HerbalWarning = warning
                                };
                                warning.AffectedMedications.Add(affectedMedication);
                            }
                        }
                    }
                }

                treatment.HerbalWarnings.Add(warning);
            }

            _logger.LogInformation("解析了 {Count} 个中药安全警告", treatment.HerbalWarnings.Count);
        }

        /// <summary>
        /// 解析食疗方案数据
        /// </summary>
        private async Task ParseDietaryTherapiesAsync(Treatment treatment, JsonElement therapiesElement)
        {
            if (therapiesElement.ValueKind != JsonValueKind.Array)
            {
                _logger.LogWarning("食疗方案数据格式无效，期望数组类型");
                return;
            }

            foreach (var therapyElement in therapiesElement.EnumerateArray())
            {
                var therapy = new DietaryTherapy
                {
                    TreatmentId = treatment.Id,
                    Name = GetStringProperty(therapyElement, "name") ?? "未知食疗方",
                    Category = GetStringProperty(therapyElement, "category") ?? "未分类",
                    Description = GetStringProperty(therapyElement, "description") ?? string.Empty,
                    Preparation = GetStringProperty(therapyElement, "preparation") ??
                                 GetStringProperty(therapyElement, "method") ?? string.Empty,
                    Efficacy = GetStringProperty(therapyElement, "efficacy") ?? string.Empty,
                    SuitableFor = GetStringProperty(therapyElement, "suitable_for") ??
                                 GetStringProperty(therapyElement, "suitableFor") ?? string.Empty,
                    Contraindications = GetStringProperty(therapyElement, "contraindications") ?? string.Empty,
                    ServingMethod = GetStringProperty(therapyElement, "serving_method") ??
                                   GetStringProperty(therapyElement, "servingMethod") ?? string.Empty,
                    StorageMethod = GetStringProperty(therapyElement, "storage_method") ??
                                   GetStringProperty(therapyElement, "storageMethod") ?? string.Empty
                };

                // 解析食材列表
                if (therapyElement.TryGetProperty("ingredients", out var ingredientsElement) &&
                    ingredientsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var ingredientElement in ingredientsElement.EnumerateArray())
                    {
                        var ingredient = new DietaryTherapyIngredient
                        {
                            // 不设置 DietaryTherapyId，让 EF Core 通过导航属性自动处理
                            Name = GetStringProperty(ingredientElement, "name") ?? "未知食材",
                            Dosage = GetStringProperty(ingredientElement, "amount") ?? "适量",
                            ProcessingMethod = GetStringProperty(ingredientElement, "processing_method") ??
                                              GetStringProperty(ingredientElement, "processingMethod") ?? "常规处理",
                            Notes = GetStringProperty(ingredientElement, "notes") ?? string.Empty,
                            DietaryTherapy = therapy
                        };
                        therapy.DietaryTherapyIngredients.Add(ingredient);
                    }
                }

                treatment.DietaryTherapies.Add(therapy);
            }

            _logger.LogInformation("解析了 {Count} 个食疗方案", treatment.DietaryTherapies.Count);
        }

        /// <summary>
        /// 获取JSON元素的字符串属性值
        /// </summary>
        private static string? GetStringProperty(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var property) &&
                property.ValueKind == JsonValueKind.String)
            {
                return property.GetString();
            }
            return null;
        }


        /// <summary>
        /// 解析针灸穴位数据
        /// </summary>
        private async Task ParseAcupunctureAsync(Treatment treatment, JsonElement acupunctureElement)
        {
            if (acupunctureElement.ValueKind != JsonValueKind.Array)
            {
                _logger.LogWarning("针灸穴位数据格式无效，期望数组类型");
                return;
            }

            foreach (var pointElement in acupunctureElement.EnumerateArray())
            {
                var acupuncture = new Acupuncture
                {
                    TreatmentId = treatment.Id,
                    PointName = GetStringProperty(pointElement, "point_name") ??
                               GetStringProperty(pointElement, "pointName") ?? "未知穴位",
                    Location = GetStringProperty(pointElement, "location") ?? string.Empty,
                    Method = GetStringProperty(pointElement, "method") ?? "毫针刺法",
                    Technique = GetStringProperty(pointElement, "technique") ?? string.Empty,
                    NeedleSpecification = GetStringProperty(pointElement, "needle_specification") ??
                                         GetStringProperty(pointElement, "needleSpecification") ?? string.Empty,
                    Depth = GetStringProperty(pointElement, "depth") ?? string.Empty,
                    Duration = GetStringProperty(pointElement, "duration") ?? string.Empty,
                    Frequency = GetStringProperty(pointElement, "frequency") ?? string.Empty,
                    Efficacy = GetStringProperty(pointElement, "efficacy") ?? string.Empty,
                    Indications = GetStringProperty(pointElement, "indications") ?? string.Empty,
                    Instructions = GetStringProperty(pointElement, "instructions") ?? string.Empty,
                    Notes = GetStringProperty(pointElement, "notes") ?? string.Empty,
                    Contraindications = GetStringProperty(pointElement, "contraindications") ?? string.Empty
                };

                treatment.Acupunctures.Add(acupuncture);
            }

            _logger.LogInformation("解析了 {Count} 个针灸穴位", treatment.Acupunctures.Count);
        }

        /// <summary>
        /// 解析艾灸穴位数据
        /// </summary>
        private async Task ParseMoxibustionAsync(Treatment treatment, JsonElement moxibustionElement)
        {
            if (moxibustionElement.ValueKind != JsonValueKind.Array)
            {
                _logger.LogWarning("艾灸穴位数据格式无效，期望数组类型");
                return;
            }

            foreach (var pointElement in moxibustionElement.EnumerateArray())
            {
                var moxibustion = new Moxibustion
                {
                    TreatmentId = treatment.Id,
                    PointName = GetStringProperty(pointElement, "point_name") ??
                               GetStringProperty(pointElement, "pointName") ?? "未知穴位",
                    Location = GetStringProperty(pointElement, "location") ?? string.Empty,
                    Method = GetStringProperty(pointElement, "method") ?? "温和灸",
                    MoxaType = GetStringProperty(pointElement, "moxa_type") ??
                              GetStringProperty(pointElement, "moxaType") ?? "艾条",
                    Technique = GetStringProperty(pointElement, "technique") ?? string.Empty,
                    TemperatureControl = GetStringProperty(pointElement, "temperature_control") ??
                                        GetStringProperty(pointElement, "temperatureControl") ?? string.Empty,
                    Duration = GetStringProperty(pointElement, "duration") ?? string.Empty,
                    Frequency = GetStringProperty(pointElement, "frequency") ?? string.Empty,
                    CourseDuration = GetStringProperty(pointElement, "course_duration") ??
                                    GetStringProperty(pointElement, "courseDuration") ?? string.Empty,
                    Efficacy = GetStringProperty(pointElement, "efficacy") ?? string.Empty,
                    Indications = GetStringProperty(pointElement, "indications") ?? string.Empty,
                    TechniquePoints = GetStringProperty(pointElement, "technique_points") ??
                                     GetStringProperty(pointElement, "techniquePoints") ?? string.Empty,
                    Precautions = GetStringProperty(pointElement, "precautions") ?? string.Empty,
                    Contraindications = GetStringProperty(pointElement, "contraindications") ?? string.Empty,
                    PostTreatmentCare = GetStringProperty(pointElement, "post_treatment_care") ??
                                       GetStringProperty(pointElement, "postTreatmentCare") ?? string.Empty,
                    CombinationTherapy = GetStringProperty(pointElement, "combination_therapy") ??
                                        GetStringProperty(pointElement, "combinationTherapy") ?? string.Empty
                };

                treatment.Moxibustions.Add(moxibustion);
            }

            _logger.LogInformation("解析了 {Count} 个艾灸穴位", treatment.Moxibustions.Count);
        }

        /// <summary>
        /// 解析拔罐部位数据
        /// </summary>
        private async Task ParseCuppingAsync(Treatment treatment, JsonElement cuppingElement)
        {
            if (cuppingElement.ValueKind != JsonValueKind.Array)
            {
                _logger.LogWarning("拔罐部位数据格式无效，期望数组类型");
                return;
            }

            foreach (var pointElement in cuppingElement.EnumerateArray())
            {
                var cupping = new Cupping
                {
                    TreatmentId = treatment.Id,
                    Area = GetStringProperty(pointElement, "area") ?? "未知部位",
                    SpecificPoints = GetStringProperty(pointElement, "specific_points") ??
                                    GetStringProperty(pointElement, "specificPoints") ?? string.Empty,
                    SuitableFor = GetStringProperty(pointElement, "suitable_for") ??
                                 GetStringProperty(pointElement, "suitableFor") ?? string.Empty,
                    Method = GetStringProperty(pointElement, "method") ?? "干拔法",
                    CupType = GetStringProperty(pointElement, "cup_type") ??
                             GetStringProperty(pointElement, "cupType") ?? "玻璃罐",
                    CupSize = GetStringProperty(pointElement, "cup_size") ??
                             GetStringProperty(pointElement, "cupSize") ?? "中号",
                    SuctionStrength = GetStringProperty(pointElement, "suction_strength") ??
                                     GetStringProperty(pointElement, "suctionStrength") ?? "中度",
                    Duration = GetStringProperty(pointElement, "duration") ?? string.Empty,
                    Frequency = GetStringProperty(pointElement, "frequency") ?? string.Empty,
                    Efficacy = GetStringProperty(pointElement, "efficacy") ?? string.Empty,
                    Indications = GetStringProperty(pointElement, "indications") ?? string.Empty,
                    TechniquePoints = GetStringProperty(pointElement, "technique_points") ??
                                     GetStringProperty(pointElement, "techniquePoints") ?? string.Empty,
                    Precautions = GetStringProperty(pointElement, "precautions") ?? string.Empty
                };

                treatment.Cuppings.Add(cupping);
            }

            _logger.LogInformation("解析了 {Count} 个拔罐部位", treatment.Cuppings.Count);
        }

        /// <summary>
        /// 解析生活方式建议数据
        /// </summary>
        private async Task ParseLifestyleAdviceAsync(Treatment treatment, JsonElement lifestyleElement)
        {
            if (lifestyleElement.ValueKind != JsonValueKind.Array)
            {
                _logger.LogWarning("生活方式建议数据格式无效，期望数组类型");
                return;
            }

            foreach (var adviceElement in lifestyleElement.EnumerateArray())
            {
                var advice = new LifestyleAdvice
                {
                    TreatmentId = treatment.Id,
                    Category = GetStringProperty(adviceElement, "category") ?? "一般建议",
                    Title = GetStringProperty(adviceElement, "title") ?? "生活方式调整",
                    Content = GetStringProperty(adviceElement, "content") ?? string.Empty,
                    Rationale = GetStringProperty(adviceElement, "rationale") ?? string.Empty,
                    Implementation = GetStringProperty(adviceElement, "implementation") ?? string.Empty,
                    Frequency = GetStringProperty(adviceElement, "frequency") ?? string.Empty,
                    Precautions = GetStringProperty(adviceElement, "precautions") ?? string.Empty,
                    Benefits = GetStringProperty(adviceElement, "benefits") ?? string.Empty
                };

                treatment.LifestyleAdvices.Add(advice);
            }

            _logger.LogInformation("解析了 {Count} 个生活方式建议", treatment.LifestyleAdvices.Count);
        }

        /// <summary>
        /// 解析饮食建议数据
        /// </summary>
        private async Task ParseDietaryAdviceAsync(Treatment treatment, JsonElement dietaryAdviceElement)
        {
            if (dietaryAdviceElement.ValueKind != JsonValueKind.Array)
            {
                _logger.LogWarning("饮食建议数据格式无效，期望数组类型");
                return;
            }

            foreach (var adviceElement in dietaryAdviceElement.EnumerateArray())
            {
                var advice = new DietaryAdvice
                {
                    TreatmentId = treatment.Id,
                    Category = GetStringProperty(adviceElement, "category") ?? "一般饮食建议",
                    Title = GetStringProperty(adviceElement, "title") ?? "饮食调理",
                    DietaryPrinciples = GetStringProperty(adviceElement, "dietary_principles") ??
                                       GetStringProperty(adviceElement, "dietaryPrinciples") ?? string.Empty,
                    MealTiming = GetStringProperty(adviceElement, "meal_timing") ??
                                GetStringProperty(adviceElement, "mealTiming") ?? string.Empty,
                    CookingMethods = GetStringProperty(adviceElement, "cooking_methods") ??
                                    GetStringProperty(adviceElement, "cookingMethods") ?? string.Empty,
                    Rationale = GetStringProperty(adviceElement, "rationale") ?? string.Empty,
                    SeasonalAdjustment = GetStringProperty(adviceElement, "seasonal_adjustment") ??
                                        GetStringProperty(adviceElement, "seasonalAdjustment") ?? string.Empty,
                    Precautions = GetStringProperty(adviceElement, "precautions") ?? string.Empty
                };

                // 解析推荐食物列表
                if (adviceElement.TryGetProperty("foods_recommended", out var recommendedElement) ||
                    adviceElement.TryGetProperty("foodsRecommended", out recommendedElement))
                {
                    if (recommendedElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var foodElement in recommendedElement.EnumerateArray())
                        {
                            if (foodElement.ValueKind == JsonValueKind.String)
                            {
                                var recommendedFood = new RecommendedFood
                                {
                                    // 不设置 DietaryAdviceId，让 EF Core 通过导航属性自动处理
                                    FoodName = foodElement.GetString() ?? "未知食物",
                                    DietaryAdvice = advice
                                };
                                advice.RecommendedFoods.Add(recommendedFood);
                            }
                        }
                    }
                }

                // 解析避免食物列表
                if (adviceElement.TryGetProperty("foods_to_avoid", out var avoidedElement) ||
                    adviceElement.TryGetProperty("foodsToAvoid", out avoidedElement))
                {
                    if (avoidedElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var foodElement in avoidedElement.EnumerateArray())
                        {
                            if (foodElement.ValueKind == JsonValueKind.String)
                            {
                                var avoidedFood = new AvoidedFood
                                {
                                    // 不设置 DietaryAdviceId，让 EF Core 通过导航属性自动处理
                                    FoodName = foodElement.GetString() ?? "未知食物",
                                    DietaryAdvice = advice
                                };
                                advice.AvoidedFoods.Add(avoidedFood);
                            }
                        }
                    }
                }

                treatment.DietaryAdvices.Add(advice);
            }

            _logger.LogInformation("解析了 {Count} 个饮食建议", treatment.DietaryAdvices.Count);
        }

        /// <summary>
        /// 解析随访建议数据
        /// </summary>
        private async Task ParseFollowUpAdviceAsync(Treatment treatment, JsonElement followUpElement)
        {
            if (followUpElement.ValueKind != JsonValueKind.Array)
            {
                _logger.LogWarning("随访建议数据格式无效，期望数组类型");
                return;
            }

            foreach (var adviceElement in followUpElement.EnumerateArray())
            {
                var advice = new FollowUpAdvice
                {
                    TreatmentId = treatment.Id,
                    FollowUpType = GetStringProperty(adviceElement, "follow_up_type") ??
                                  GetStringProperty(adviceElement, "followUpType") ?? "定期复诊",
                    Title = GetStringProperty(adviceElement, "title") ?? "随访建议",
                    Timing = GetStringProperty(adviceElement, "timing") ?? string.Empty,
                    Purpose = GetStringProperty(adviceElement, "purpose") ?? string.Empty,
                    PreparationRequired = GetStringProperty(adviceElement, "preparation_required") ??
                                         GetStringProperty(adviceElement, "preparationRequired") ?? string.Empty,
                    EmergencyConditions = GetStringProperty(adviceElement, "emergency_conditions") ??
                                         GetStringProperty(adviceElement, "emergencyConditions") ?? string.Empty,
                    SelfMonitoring = GetStringProperty(adviceElement, "self_monitoring") ??
                                    GetStringProperty(adviceElement, "selfMonitoring") ?? string.Empty,
                    ContactInformation = GetStringProperty(adviceElement, "contact_information") ??
                                        GetStringProperty(adviceElement, "contactInformation") ?? string.Empty
                };

                // 解析监测指标列表
                if (adviceElement.TryGetProperty("monitoring_indicators", out var indicatorsElement) ||
                    adviceElement.TryGetProperty("monitoringIndicators", out indicatorsElement))
                {
                    if (indicatorsElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var indicatorElement in indicatorsElement.EnumerateArray())
                        {
                            if (indicatorElement.ValueKind == JsonValueKind.String)
                            {
                                var indicator = new MonitoringIndicator
                                {
                                    // 不设置 FollowUpAdviceId，让 EF Core 通过导航属性自动处理
                                    IndicatorName = indicatorElement.GetString() ?? "未知指标",
                                    FollowUpAdvice = advice
                                };
                                advice.MonitoringIndicators.Add(indicator);
                            }
                        }
                    }
                }

                treatment.FollowUpAdvices.Add(advice);
            }

            _logger.LogInformation("解析了 {Count} 个随访建议", treatment.FollowUpAdvices.Count);
        }

        /// <summary>
        /// 获取AI生成的原始治疗方案
        /// </summary>
        /// <param name="syndromeId">证候ID</param>
        /// <returns>AI原始治疗方案DTO，如果不存在则返回null</returns>
        public async Task<TreatmentDto?> GetAiOriginatedTreatmentAsync(int syndromeId)
        {
            if (syndromeId <= 0)
            {
                throw new ArgumentException("证候ID必须大于0", nameof(syndromeId));
            }

            try
            {
                _logger.LogInformation("开始获取证候 {SyndromeId} 的AI原始治疗方案", syndromeId);

                var treatment = await _context.Treatments
                    .Where(t => t.SyndromeId == syndromeId && t.IsAiOriginated)
                    .FirstOrDefaultAsync();

                if (treatment == null)
                {
                    _logger.LogInformation("证候 {SyndromeId} 的AI原始治疗方案不存在", syndromeId);
                    return null;
                }

                // 使用已有的GetTreatmentByIdAsync方法加载完整数据
                return await GetTreatmentByIdAsync(treatment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取证候 {SyndromeId} 的AI原始治疗方案时发生错误", syndromeId);
                throw;
            }
        }

        /// <summary>
        /// 重置治疗方案到AI原始版本
        /// </summary>
        /// <param name="currentTreatmentId">当前治疗方案ID</param>
        /// <param name="aiOriginalTreatmentId">AI原始治疗方案ID</param>
        /// <param name="doctorUserId">医生用户ID</param>
        public async Task ResetTreatmentToAiOriginalAsync(int currentTreatmentId, int aiOriginalTreatmentId, int doctorUserId)
        {
            if (currentTreatmentId <= 0 || aiOriginalTreatmentId <= 0)
            {
                throw new ArgumentException("治疗方案ID必须大于0");
            }

            try
            {
                _logger.LogInformation("开始重置治疗方案 {CurrentTreatmentId} 到AI原始版本 {AiOriginalTreatmentId}", 
                    currentTreatmentId, aiOriginalTreatmentId);

                // 获取AI原始版本
                var aiOriginal = await GetTreatmentByIdAsync(aiOriginalTreatmentId);
                if (aiOriginal == null)
                {
                    throw new InvalidOperationException($"AI原始治疗方案 {aiOriginalTreatmentId} 不存在");
                }

                // 获取当前治疗方案实体
                var currentTreatment = await _context.Treatments
                    .Include(t => t.Prescriptions)
                        .ThenInclude(p => p.PrescriptionItems)
                    .Include(t => t.Acupunctures)
                    .Include(t => t.Moxibustions)
                    .Include(t => t.Cuppings)
                    .Include(t => t.DietaryTherapies)
                        .ThenInclude(dt => dt.DietaryTherapyIngredients)
                    .Include(t => t.LifestyleAdvices)
                    .Include(t => t.DietaryAdvices)
                        .ThenInclude(da => da.RecommendedFoods)
                    .Include(t => t.DietaryAdvices)
                        .ThenInclude(da => da.AvoidedFoods)
                    .Include(t => t.FollowUpAdvices)
                        .ThenInclude(fa => fa.MonitoringIndicators)
                    .FirstOrDefaultAsync(t => t.Id == currentTreatmentId);

                if (currentTreatment == null)
                {
                    throw new InvalidOperationException($"当前治疗方案 {currentTreatmentId} 不存在");
                }

                // 复制基础信息
                currentTreatment.TcmDiagnosis = aiOriginal.TcmDiagnosis;
                currentTreatment.SyndromeAnalysis = aiOriginal.SyndromeAnalysis;
                currentTreatment.TreatmentPrinciple = aiOriginal.TreatmentPrinciple;
                currentTreatment.ExpectedOutcome = aiOriginal.ExpectedOutcome;
                currentTreatment.Precautions = aiOriginal.Precautions;
                currentTreatment.UpdatedAt = DateTime.UtcNow;
                currentTreatment.UpdatedByUserId = doctorUserId;
                currentTreatment.Status = TreatmentStatus.Editing;

                // 清空当前版本的所有子项
                currentTreatment.Prescriptions.Clear();
                currentTreatment.Acupunctures.Clear();
                currentTreatment.Moxibustions.Clear();
                currentTreatment.Cuppings.Clear();
                currentTreatment.DietaryTherapies.Clear();
                currentTreatment.LifestyleAdvices.Clear();
                currentTreatment.DietaryAdvices.Clear();
                currentTreatment.FollowUpAdvices.Clear();

                // 复制AI原始版本的所有子项
                // 使用UpdateTreatmentBasicInfoAsync的逻辑来复制数据
                var editDto = new TreatmentDto
                {
                    Id = currentTreatmentId,
                    TcmDiagnosis = aiOriginal.TcmDiagnosis,
                    SyndromeAnalysis = aiOriginal.SyndromeAnalysis,
                    TreatmentPrinciple = aiOriginal.TreatmentPrinciple,
                    ExpectedOutcome = aiOriginal.ExpectedOutcome,
                    Precautions = aiOriginal.Precautions,
                    Prescriptions = aiOriginal.Prescriptions,
                    Acupunctures = aiOriginal.Acupunctures,
                    Moxibustions = aiOriginal.Moxibustions,
                    Cuppings = aiOriginal.Cuppings,
                    DietaryTherapies = aiOriginal.DietaryTherapies,
                    LifestyleAdvices = aiOriginal.LifestyleAdvices,
                    DietaryAdvices = aiOriginal.DietaryAdvices,
                    FollowUpAdvices = aiOriginal.FollowUpAdvices
                };

                // 使用已有的更新方法
                await UpdateTreatmentBasicInfoAsync(editDto, doctorUserId);

                _logger.LogInformation("成功重置治疗方案 {CurrentTreatmentId} 到AI原始版本", currentTreatmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重置治疗方案 {CurrentTreatmentId} 到AI原始版本时发生错误", currentTreatmentId);
                throw;
            }
        }
    }
}
