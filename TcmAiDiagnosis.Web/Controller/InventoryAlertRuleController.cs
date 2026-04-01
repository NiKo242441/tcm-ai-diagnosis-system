using Microsoft.AspNetCore.Mvc;
using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.Dtos;
using TcmAiDiagnosis.Domain.Paged;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Web.Controllers
{
    /// <summary>
    /// 库存预警规则控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryAlertRuleController : ControllerBase
    {
        private readonly InventoryAlertRuleDomain _alertRuleDomain;

        public InventoryAlertRuleController(InventoryAlertRuleDomain alertRuleDomain)
        {
            _alertRuleDomain = alertRuleDomain;
        }

        /// <summary>
        /// 添加预警规则
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResult<InventoryAlertRule>>> CreateAlertRule([FromBody] CreateAlertRuleDto dto)
        {
            try
            {
                var rule = new InventoryAlertRule
                {
                    RuleName = dto.RuleName,
                    HerbId = dto.HerbId,
                    AlertType = dto.AlertType,
                    Threshold = dto.Threshold,
                    ComparisonOperator = dto.ComparisonOperator,
                    NotifyUserIds = dto.NotifyUserIds,
                    Priority = dto.Priority,
                    Remark = dto.Remark,
                    TenantId = dto.TenantId,
                    CreatedBy = dto.CreatedBy
                };

                var result = await _alertRuleDomain.AddAlertRuleAsync(rule);
                return ApiResult<InventoryAlertRule>.SuccessResult(result, "预警规则创建成功");
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// 更新预警规则
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResult>> UpdateAlertRule(int id, [FromBody] UpdateAlertRuleDto dto)
        {
            try
            {
                var rule = new InventoryAlertRule
                {
                    Id = id,
                    RuleName = dto.RuleName,
                    HerbId = dto.HerbId,
                    AlertType = dto.AlertType,
                    Threshold = dto.Threshold,
                    ComparisonOperator = dto.ComparisonOperator,
                    NotifyUserIds = dto.NotifyUserIds,
                    IsEnabled = dto.IsEnabled,
                    Priority = dto.Priority,
                    Remark = dto.Remark,
                    TenantId = dto.TenantId
                };

                await _alertRuleDomain.UpdateAlertRuleAsync(rule);
                return ApiResult.SuccessResult("预警规则更新成功");
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// 获取预警规则详情
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResult<InventoryAlertRule>>> GetAlertRule(int id, [FromQuery] bool includeHerb = false)
        {
            try
            {
                var result = await _alertRuleDomain.GetAlertRuleByIdAsync(id, includeHerb);
                if (result == null)
                    return NotFound(ApiResult<object>.ErrorResult("预警规则不存在"));

                return ApiResult<InventoryAlertRule>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// 分页查询预警规则
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResult<PagedResult<InventoryAlertRule>>>> GetPagedAlertRules(
            [FromQuery] PagedRequest request,
            [FromQuery] int? tenantId = null,
            [FromQuery] string? alertType = null,
            [FromQuery] bool? isEnabled = null)
        {
            try
            {
                var result = await _alertRuleDomain.GetPagedAlertRulesAsync(request, tenantId, alertType, isEnabled);
                return ApiResult<PagedResult<InventoryAlertRule>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// 启用/禁用预警规则
        /// </summary>
        [HttpPatch("{id}/toggle")]
        public async Task<ActionResult<ApiResult>> ToggleAlertRule(int id, [FromBody] ToggleAlertRuleDto dto)
        {
            try
            {
                await _alertRuleDomain.ToggleAlertRuleAsync(id, dto.IsEnabled);
                var message = dto.IsEnabled ? "预警规则已启用" : "预警规则已禁用";
                return ApiResult.SuccessResult(message);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// 删除预警规则
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResult>> DeleteAlertRule(int id)
        {
            try
            {
                await _alertRuleDomain.DeleteAlertRuleAsync(id);
                return ApiResult.SuccessResult("预警规则删除成功");
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// 检查库存预警
        /// </summary>
        [HttpGet("check-alerts")]
        public async Task<ActionResult<ApiResult<List<InventoryAlertResult>>>> CheckInventoryAlerts([FromQuery] int? tenantId = null)
        {
            try
            {
                var result = await _alertRuleDomain.CheckInventoryAlertsAsync(tenantId);
                return ApiResult<List<InventoryAlertResult>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// 获取指定药材的预警规则
        /// </summary>
        [HttpGet("by-herb/{herbId}")]
        public async Task<ActionResult<ApiResult<List<InventoryAlertRule>>>> GetAlertRulesByHerb(int herbId, [FromQuery] int? tenantId = null)
        {
            try
            {
                var result = await _alertRuleDomain.GetAlertRulesByHerbAsync(herbId, tenantId);
                return ApiResult<List<InventoryAlertRule>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// 批量更新规则状态
        /// </summary>
        [HttpPatch("batch-update-status")]
        public async Task<ActionResult<ApiResult>> BatchUpdateRuleStatus([FromBody] BatchUpdateRuleStatusDto dto)
        {
            try
            {
                await _alertRuleDomain.BatchUpdateRuleStatusAsync(dto.RuleIds, dto.IsEnabled);
                var message = dto.IsEnabled ? "规则批量启用成功" : "规则批量禁用成功";
                return ApiResult.SuccessResult(message);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.ErrorResult(ex.Message));
            }
        }
    }
}