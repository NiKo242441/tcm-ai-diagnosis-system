using Microsoft.AspNetCore.Mvc;
using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.Domain.Paged;
using TcmAiDiagnosis.Dtos;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Web.Controllers
{
    /// <summary>
    /// 中药配伍禁忌管理API
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HerbContraindicationController : ControllerBase
    {
        private readonly HerbContraindicationDomain _contraindicationDomain;
        private readonly ILogger<HerbContraindicationController> _logger;

        public HerbContraindicationController(
            HerbContraindicationDomain contraindicationDomain,
            ILogger<HerbContraindicationController> logger)
        {
            _contraindicationDomain = contraindicationDomain;
            _logger = logger;
        }

        /// <summary>
        /// 创建配伍禁忌记录
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<HerbContraindicationDto>> CreateContraindication(
            [FromBody] CreateHerbContraindicationDto request)
        {
            try
            {
                // 验证主药材和冲突药材是否存在
                var primaryHerbExists = await _contraindicationDomain.CheckHerbExistsAsync(request.PrimaryHerbId);
                var conflictHerbExists = await _contraindicationDomain.CheckHerbExistsAsync(request.ConflictHerbId);

                if (!primaryHerbExists)
                    return NotFound($"主药材ID {request.PrimaryHerbId} 不存在");

                if (!conflictHerbExists)
                    return NotFound($"冲突药材ID {request.ConflictHerbId} 不存在");

                var contraindication = new HerbContraindication
                {
                    PrimaryHerbId = request.PrimaryHerbId,
                    ConflictHerbId = request.ConflictHerbId,
                    ContraindicationType = request.ContraindicationType,
                    RiskLevel = request.RiskLevel,
                    Description = request.Description,
                    AdverseReactions = request.AdverseReactions,
                    Evidence = request.Evidence,
                    IsAbsoluteContraindication = request.IsAbsoluteContraindication,
                    SpecialNotes = request.SpecialNotes,
                    AlternativeSuggestions = request.AlternativeSuggestions,
                    IsActive = true,
                    ReviewStatus = 0, // 默认为待审核
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                var created = await _contraindicationDomain.AddContraindicationAsync(contraindication);
                return Ok(MapToDto(created));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建配伍禁忌失败");
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 更新配伍禁忌记录
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateContraindication(
            [FromBody] UpdateHerbContraindicationDto request)
        {
            try
            {
                var contraindication = await _contraindicationDomain.GetContraindicationByIdAsync(request.ContraindicationId);
                if (contraindication == null)
                    return NotFound("未找到指定配伍禁忌记录");

                contraindication.PrimaryHerbId = request.PrimaryHerbId;
                contraindication.ConflictHerbId = request.ConflictHerbId;
                contraindication.ContraindicationType = request.ContraindicationType;
                contraindication.RiskLevel = request.RiskLevel;
                contraindication.Description = request.Description;
                contraindication.AdverseReactions = request.AdverseReactions;
                contraindication.Evidence = request.Evidence;
                contraindication.IsAbsoluteContraindication = request.IsAbsoluteContraindication;
                contraindication.SpecialNotes = request.SpecialNotes;
                contraindication.AlternativeSuggestions = request.AlternativeSuggestions;
                contraindication.IsActive = request.IsActive;
                contraindication.UpdatedAt = DateTime.Now;

                await _contraindicationDomain.UpdateContraindicationAsync(contraindication);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新配伍禁忌失败");
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 分页查询配伍禁忌记录
        /// </summary>
        [HttpGet("query")]
        public async Task<ActionResult<PagedResult<HerbContraindicationDto>>> QueryContraindications(
            [FromQuery] HerbContraindicationQueryDto query)
        {
            try
            {
                var result = await _contraindicationDomain.QueryContraindicationsAsync(query);
                return Ok(new PagedResult<HerbContraindicationDto>
                {
                    TotalCount = result.TotalCount,
                    Page = query.PageNumber,
                    PageSize = query.PageSize,
                    Items = result.Items.Select(MapToDto).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询配伍禁忌失败");
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 检查药材组合的配伍禁忌
        /// </summary>
        [HttpPost("check")]
        public async Task<ActionResult<ContraindicationCheckResult>> CheckContraindications(
            [FromBody] ContraindicationCheckRequest request)
        {
            try
            {
                // 验证药材ID列表
                if (request.HerbIds.Distinct().Count() != request.HerbIds.Count)
                    return BadRequest("药材ID列表中存在重复值");

                var result = await _contraindicationDomain.CheckContraindicationsAsync(request.HerbIds);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查配伍禁忌失败");
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 审核配伍禁忌记录
        /// </summary>
        [HttpPost("review")]
        public async Task<IActionResult> ReviewContraindication(
            [FromBody] ReviewHerbContraindicationDto request)
        {
            try
            {
                await _contraindicationDomain.ReviewContraindicationAsync(
                    request.ContraindicationId,
                    request.ReviewStatus,
                    request.ReviewComments);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "审核配伍禁忌失败");
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 获取单个配伍禁忌记录详情
        /// </summary>
        [HttpGet("{contraindicationId}")]
        public async Task<ActionResult<HerbContraindicationDto>> GetContraindication(int contraindicationId)
        {
            try
            {
                var contraindication = await _contraindicationDomain.GetContraindicationByIdAsync(contraindicationId);
                if (contraindication == null)
                    return NotFound("未找到指定配伍禁忌记录");

                return Ok(MapToDto(contraindication));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取配伍禁忌详情失败");
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 删除配伍禁忌记录（软删除）
        /// </summary>
        [HttpDelete("{contraindicationId}")]
        public async Task<IActionResult> DeleteContraindication(int contraindicationId)
        {
            try
            {
                var contraindication = await _contraindicationDomain.GetContraindicationByIdAsync(contraindicationId);
                if (contraindication == null)
                    return NotFound("未找到指定配伍禁忌记录");

                contraindication.IsActive = false;
                contraindication.UpdatedAt = DateTime.Now;

                await _contraindicationDomain.UpdateContraindicationAsync(contraindication);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除配伍禁忌失败");
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 批量导入配伍禁忌记录
        /// </summary>
        [HttpPost("batch-import")]
        public async Task<ActionResult<(int successCount, int skipCount)>> BatchImportContraindications(
            [FromBody] List<CreateHerbContraindicationDto> contraindications)
        {
            try
            {
                var contraindicationEntities = contraindications.Select(c => new HerbContraindication
                {
                    PrimaryHerbId = c.PrimaryHerbId,
                    ConflictHerbId = c.ConflictHerbId,
                    ContraindicationType = c.ContraindicationType,
                    RiskLevel = c.RiskLevel,
                    Description = c.Description,
                    AdverseReactions = c.AdverseReactions,
                    Evidence = c.Evidence,
                    IsAbsoluteContraindication = c.IsAbsoluteContraindication,
                    SpecialNotes = c.SpecialNotes,
                    AlternativeSuggestions = c.AlternativeSuggestions,
                    IsActive = true,
                    ReviewStatus = 0,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }).ToList();

                var result = await _contraindicationDomain.BatchImportContraindicationsAsync(contraindicationEntities);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量导入配伍禁忌失败");
                return StatusCode(500, "服务器内部错误");
            }
        }

        // DTO映射方法
        private HerbContraindicationDto MapToDto(HerbContraindication contraindication)
        {
            return new HerbContraindicationDto
            {
                ContraindicationId = contraindication.ContraindicationId,
                PrimaryHerbId = contraindication.PrimaryHerbId,
                PrimaryHerbName = contraindication.PrimaryHerb?.Name ?? "",
                ConflictHerbId = contraindication.ConflictHerbId,
                ConflictHerbName = contraindication.ConflictHerb?.Name ?? "",
                ContraindicationType = contraindication.ContraindicationType,
                RiskLevel = contraindication.RiskLevel,
                RiskLevelDescription = GetRiskLevelDescription(contraindication.RiskLevel),
                Description = contraindication.Description,
                AdverseReactions = contraindication.AdverseReactions,
                Evidence = contraindication.Evidence,
                IsAbsoluteContraindication = contraindication.IsAbsoluteContraindication,
                SpecialNotes = contraindication.SpecialNotes,
                AlternativeSuggestions = contraindication.AlternativeSuggestions,
                IsActive = contraindication.IsActive,
                ReviewStatus = contraindication.ReviewStatus,
                ReviewStatusDescription = GetReviewStatusDescription(contraindication.ReviewStatus),
                ReviewComments = contraindication.ReviewComments,
                CreatedAt = contraindication.CreatedAt,
                UpdatedAt = contraindication.UpdatedAt
            };
        }

        // 风险等级描述转换
        private string GetRiskLevelDescription(int riskLevel)
        {
            return riskLevel switch
            {
                1 => "低风险",
                2 => "中风险",
                3 => "高风险",
                4 => "严重风险",
                _ => "未知风险"
            };
        }

        // 审核状态描述转换
        private string GetReviewStatusDescription(int reviewStatus)
        {
            return reviewStatus switch
            {
                0 => "待审核",
                1 => "已审核",
                2 => "审核不通过",
                _ => "未知状态"
            };
        }
    }
}