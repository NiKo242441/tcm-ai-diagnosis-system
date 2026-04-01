using Microsoft.AspNetCore.Mvc;
using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.Domain.Paged;
using TcmAiDiagnosis.Dtos;
using TcmAiDiagnosis.Entities;
using TcmAiDiagnosis.Entities.Enums;
using Microsoft.Extensions.Logging;
using TcmAiDiagnosis.Web.Attributes;
using TcmAiDiagnosis.Domain.Constants;

namespace TcmAiDiagnosis.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HerbController : ControllerBase
    {
        private readonly HerbDomain _herbDomain;
        private readonly ILogger<HerbController> _logger;

        public HerbController(HerbDomain herbDomain, ILogger<HerbController> logger)
        {
            _herbDomain = herbDomain;
            _logger = logger;
        }

        /// <summary>
        /// 获取分页药材列表
        /// GET /api/Herb/paged
        /// </summary>
        [HttpGet("paged")]
        [RequirePermission(PermissionConstants.KnowledgeBase.ViewHerb)]
        public async Task<ActionResult<PagedResult<HerbDto>>> GetPagedHerbs(
            [FromQuery] string? searchKeyword = null,
            [FromQuery] string? category = null,
            [FromQuery] int? toxicityLevel = null, // 改为int?类型
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                _logger.LogInformation($"查询药材分页列表: Page={pageNumber}, PageSize={pageSize}, Category={category}, ToxicityLevel={toxicityLevel}");

                var request = new PagedRequest
                {
                    Page = pageNumber,
                    PageSize = pageSize,
                    SearchKeyword = searchKeyword
                };

                var result = await _herbDomain.GetPagedHerbsAsync(request, category, toxicityLevel);
                var herbDtos = result.Items.Select(MapToDto).ToList();

                var pagedResult = new PagedResult<HerbDto>
                {
                    Items = herbDtos,
                    TotalCount = result.TotalCount,
                    Page = result.Page,
                    PageSize = result.PageSize
                };

                _logger.LogInformation($"查询成功，返回 {herbDtos.Count} 条记录");
                return Ok(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询药材分页列表失败");
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 获取所有药材（最大1000条）
        /// GET /api/Herb
        /// </summary>
        [HttpGet]
        [RequirePermission(PermissionConstants.KnowledgeBase.ViewHerb)]
        public async Task<ActionResult<List<HerbDto>>> GetAllHerbs()
        {
            try
            {
                _logger.LogInformation("查询全部药材列表");
                var pagedResult = await _herbDomain.GetPagedHerbsAsync(
                    new PagedRequest { Page = 1, PageSize = 1000 },
                    category: null,
                    toxicityLevel: null);

                var herbDtos = pagedResult.Items.Select(MapToDto).ToList();
                return Ok(herbDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询全部药材列表失败");
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 获取所有药材分类
        /// GET /api/Herb/categories
        /// </summary>
        [HttpGet("categories")]
        public async Task<ActionResult<List<string>>> GetCategories()
        {
            try
            {
                var categories = await _herbDomain.GetHerbCategoriesAsync();
                _logger.LogInformation($"获取到 {categories.Count} 个分类");
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取药材分类失败");
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 根据ID获取药材详情
        /// GET /api/Herb/{id}
        /// </summary>
        [HttpGet("{id}")]
        [RequirePermission(PermissionConstants.KnowledgeBase.ViewHerb)]
        public async Task<ActionResult<HerbDto>> GetHerb(int id, [FromQuery] bool includeContraindications = false)
        {
            try
            {
                var herb = await _herbDomain.GetHerbByIdAsync(id, includeContraindications);
                if (herb == null)
                {
                    _logger.LogWarning($"未找到药材: {id}");
                    return NotFound("未找到指定药材");
                }
                return Ok(MapToDto(herb));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取药材详情失败");
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 创建新药材
        /// POST /api/Herb
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<HerbDto>> CreateHerb([FromBody] CreateHerbDto request)
        {
            try
            {
                _logger.LogInformation($"创建药材: {request.HerbName}");

                var herb = new Herb
                {
                    Name = request.HerbName,
                    Properties = request.Properties ?? string.Empty,
                    Meridians = request.Meridians ?? string.Empty,
                    Efficacy = request.Efficacy ?? string.Empty,
                    Category = request.Category ?? string.Empty,
                    DosageRange = request.Dosage ?? string.Empty,
                    Precautions = request.Precautions ?? string.Empty,
                    ProcessingMethods = request.ProcessingMethod ?? string.Empty,
                    ToxicityLevel = request.ToxicityLevel ?? "无毒",
                    CommonUnit = "g", // 设置默认值
                    Aliases = string.Empty, // 设置默认值
                    Contraindications = string.Empty, // 设置默认值
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false, // 新创建的药材默认未删除
                    IsCommonlyUsed = false, // 设置默认值
                    RequiresSpecialHandling = false, // 设置默认值
                    IsAiOriginated = false, // 设置默认值
                    Status = ReviewStatus.AIGenerated, // 设置默认值
                    TenantId = 1 // 设置默认租户ID，根据实际情况调整
                };

                var createdHerb = await _herbDomain.AddHerbAsync(herb);
                _logger.LogInformation($"药材创建成功: ID={createdHerb.Id}");

                return CreatedAtAction(nameof(GetHerb), new { id = createdHerb.Id }, MapToDto(createdHerb));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"参数错误: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"业务逻辑错误: {ex.Message}");
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建药材失败");
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 更新药材信息
        /// PUT /api/Herb/{id}
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHerb(int id, [FromBody] UpdateHerbDto request)
        {
            if (id != request.HerbId)
            {
                _logger.LogWarning($"药材ID不匹配: 路径={id}, 请求体={request.HerbId}");
                return BadRequest("药材ID不匹配");
            }

            try
            {
                _logger.LogInformation($"更新药材: ID={id}");

                var existingHerb = await _herbDomain.GetHerbByIdAsync(id);
                if (existingHerb == null)
                {
                    return NotFound("未找到指定药材");
                }

                // 更新字段
                existingHerb.Name = request.HerbName;
                existingHerb.Properties = request.Properties ?? string.Empty;
                existingHerb.Meridians = request.Meridians ?? string.Empty;
                existingHerb.Efficacy = request.Efficacy ?? string.Empty;
                existingHerb.Category = request.Category ?? string.Empty;
                existingHerb.DosageRange = request.Dosage ?? string.Empty;
                existingHerb.Precautions = request.Precautions ?? string.Empty;
                existingHerb.ProcessingMethods = request.ProcessingMethod ?? string.Empty;
                existingHerb.ToxicityLevel = request.ToxicityLevel ?? "无毒";
                existingHerb.UpdatedAt = DateTime.UtcNow;
                existingHerb.IsDeleted = !request.IsActive; // 映射IsActive到IsDeleted

                await _herbDomain.UpdateHerbAsync(existingHerb);
                _logger.LogInformation($"药材更新成功: ID={id}");

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"未找到药材: {ex.Message}");
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"业务逻辑错误: {ex.Message}");
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新药材失败");
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 删除药材
        /// DELETE /api/Herb/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHerb(int id)
        {
            try
            {
                _logger.LogInformation($"删除药材: ID={id}");

                var herb = await _herbDomain.GetHerbByIdAsync(id);
                if (herb == null)
                {
                    _logger.LogWarning($"未找到要删除的药材: {id}");
                    return NotFound("未找到指定药材");
                }

                herb.IsDeleted = true;
                herb.UpdatedAt = DateTime.UtcNow;
                await _herbDomain.UpdateHerbAsync(herb);

                _logger.LogInformation($"药材删除成功: ID={id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除药材失败");
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 获取有毒药材列表
        /// GET /api/Herb/toxic
        /// </summary>
        [HttpGet("toxic")]
        public async Task<ActionResult<List<HerbDto>>> GetToxicHerbs()
        {
            try
            {
                var toxicHerbs = await _herbDomain.GetToxicHerbsAsync();
                var toxicHerbDtos = toxicHerbs.Select(MapToDto).ToList();

                _logger.LogInformation($"获取到 {toxicHerbDtos.Count} 个有毒药材");
                return Ok(toxicHerbDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取有毒药材失败");
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 检查药材是否存在配伍禁忌
        /// GET /api/Herb/{id}/has-contraindications
        /// </summary>
        [HttpGet("{id}/has-contraindications")]
        public async Task<ActionResult<bool>> HasContraindications(int id)
        {
            try
            {
                var hasContraindications = await _herbDomain.HasContraindicationsAsync(id);
                return Ok(hasContraindications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查配伍禁忌失败");
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 获取药材的配伍禁忌列表
        /// GET /api/Herb/{id}/contraindications
        /// </summary>
        [HttpGet("{id}/contraindications")]
        public async Task<ActionResult<List<HerbDto>>> GetContraindications(int id)
        {
            try
            {
                var contraindicatedHerbs = await _herbDomain.GetContraindicatedHerbsAsync(id);
                var contraindicatedHerbDtos = contraindicatedHerbs.Select(MapToDto).ToList();

                return Ok(contraindicatedHerbDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取药材配伍禁忌失败");
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 测试端点
        /// GET /api/Herb/test
        /// </summary>
        [HttpGet("test")]
        public ActionResult<string> Test()
        {
            return Ok("Herb API is working!");
        }

        /// <summary>
        /// 数据库测试端点
        /// GET /api/Herb/test-db
        /// </summary>
        [HttpGet("test-db")]
        public async Task<ActionResult<string>> TestDatabase()
        {
            try
            {
                var count = await _herbDomain.GetPagedHerbsAsync(new PagedRequest { Page = 1, PageSize = 1 }, null, null);
                return Ok($"数据库连接正常，共有 {count.TotalCount} 条药材记录");
            }
            catch (Exception ex)
            {
                return BadRequest($"数据库测试失败: {ex.Message}");
            }
        }

        /// <summary>
        /// DTO映射方法 - 根据实体类和DTO结构调整
        /// </summary>
        private HerbDto MapToDto(Herb herb)
        {
            return new HerbDto
            {
                HerbId = herb.Id,
                HerbName = herb.Name,
                Properties = herb.Properties,
                Meridians = herb.Meridians,
                Efficacy = herb.Efficacy,
                Category = herb.Category,
                Dosage = herb.DosageRange,
                Precautions = herb.Precautions,
                ProcessingMethod = herb.ProcessingMethods,
                ToxicityLevel = herb.ToxicityLevel,
                IsActive = !herb.IsDeleted, // 映射IsDeleted到IsActive
                CreatedAt = herb.CreatedAt,
                UpdatedAt = herb.UpdatedAt ?? herb.CreatedAt,
                // Source和Remarks字段在实体类中不存在，设置为null
                Source = null,
                Remarks = null
            };
        }
    }
}