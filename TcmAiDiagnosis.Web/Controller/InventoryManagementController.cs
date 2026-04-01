using Microsoft.AspNetCore.Mvc;
using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.Dtos;
using TcmAiDiagnosis.Domain.Paged;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Web.Controllers
{
    /// <summary>
    /// 库存管理控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryManagementController : ControllerBase
    {
        private readonly InventoryManagementDomain _inventoryManagementDomain;

        public InventoryManagementController(InventoryManagementDomain inventoryManagementDomain)
        {
            _inventoryManagementDomain = inventoryManagementDomain;
        }

        /// <summary>
        /// 添加库存明细
        /// </summary>
        [HttpPost("items")]
        public async Task<ActionResult<ApiResult<InventoryItem>>> CreateInventoryItem([FromBody] CreateInventoryItemDto dto)
        {
            try
            {
                var item = new InventoryItem
                {
                    BatchNumber = dto.BatchNumber,
                    HerbId = dto.HerbId,
                    CurrentQuantity = dto.CurrentQuantity,
                    Unit = dto.Unit,
                    StorageLocation = dto.StorageLocation,
                    PurchasePrice = dto.PurchasePrice,
                    SalePrice = dto.SalePrice,
                    ProductionDate = dto.ProductionDate,
                    ExpiryDate = dto.ExpiryDate,
                    SupplierName = dto.SupplierName,
                    QualityStatus = dto.QualityStatus,
                    Remark = dto.Remark,
                    TenantId = dto.TenantId
                };

                var result = await _inventoryManagementDomain.AddInventoryItemAsync(item);
                return ApiResult<InventoryItem>.SuccessResult(result, "库存明细创建成功");
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// 更新库存明细
        /// </summary>
        [HttpPut("items/{id}")]
        public async Task<ActionResult<ApiResult>> UpdateInventoryItem(int id, [FromBody] UpdateInventoryItemDto dto)
        {
            try
            {
                var item = new InventoryItem
                {
                    Id = id,
                    BatchNumber = dto.BatchNumber,
                    StorageLocation = dto.StorageLocation,
                    PurchasePrice = dto.PurchasePrice,
                    SalePrice = dto.SalePrice,
                    ProductionDate = dto.ProductionDate,
                    ExpiryDate = dto.ExpiryDate,
                    SupplierName = dto.SupplierName,
                    QualityStatus = dto.QualityStatus,
                    Remark = dto.Remark,
                    TenantId = dto.TenantId
                };

                await _inventoryManagementDomain.UpdateInventoryItemAsync(item);
                return ApiResult.SuccessResult("库存明细更新成功");
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// 获取库存明细详情
        /// </summary>
        [HttpGet("items/{id}")]
        public async Task<ActionResult<ApiResult<InventoryItem>>> GetInventoryItem(int id, [FromQuery] bool includeHerb = false)
        {
            try
            {
                var result = await _inventoryManagementDomain.GetInventoryItemByIdAsync(id, includeHerb);
                if (result == null)
                    return NotFound(ApiResult<object>.ErrorResult("库存明细不存在"));

                return ApiResult<InventoryItem>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// 分页查询库存明细
        /// </summary>
        [HttpGet("items")]
        public async Task<ActionResult<ApiResult<PagedResult<InventoryItem>>>> GetPagedInventoryItems(
            [FromQuery] PagedRequest request,
            [FromQuery] int? tenantId = null,
            [FromQuery] int? herbId = null,
            [FromQuery] string? status = null,
            [FromQuery] string? qualityStatus = null)
        {
            try
            {
                var result = await _inventoryManagementDomain.GetPagedInventoryItemsAsync(request, tenantId, herbId, status, qualityStatus);
                return ApiResult<PagedResult<InventoryItem>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// 采购入库
        /// </summary>
        [HttpPost("purchase-in")]
        public async Task<ActionResult<ApiResult<InventoryOperation>>> PurchaseIn([FromBody] InventoryOperationDto dto)
        {
            try
            {
                var result = await _inventoryManagementDomain.PurchaseInAsync(
                    dto.InventoryItemId,
                    dto.Quantity,
                    dto.ReferenceNumber,
                    dto.OperatedBy,
                    dto.Reason);

                return ApiResult<InventoryOperation>.SuccessResult(result, "采购入库成功");
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// 销售出库
        /// </summary>
        [HttpPost("sale-out")]
        public async Task<ActionResult<ApiResult<InventoryOperation>>> SaleOut([FromBody] InventoryOperationDto dto)
        {
            try
            {
                var result = await _inventoryManagementDomain.SaleOutAsync(
                    dto.InventoryItemId,
                    dto.Quantity,
                    dto.ReferenceNumber,
                    dto.OperatedBy,
                    dto.Reason);

                return ApiResult<InventoryOperation>.SuccessResult(result, "销售出库成功");
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// 库存调整
        /// </summary>
        [HttpPost("adjust")]
        public async Task<ActionResult<ApiResult<InventoryOperation>>> AdjustInventory([FromBody] AdjustInventoryDto dto)
        {
            try
            {
                var result = await _inventoryManagementDomain.AdjustInventoryAsync(
                    dto.InventoryItemId,
                    dto.Quantity,
                    dto.OperatedBy,
                    dto.Reason);

                return ApiResult<InventoryOperation>.SuccessResult(result, "库存调整成功");
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// 获取库存操作历史
        /// </summary>
        [HttpGet("operations/{inventoryItemId}")]
        public async Task<ActionResult<ApiResult<PagedResult<InventoryOperation>>>> GetInventoryOperations(
            int inventoryItemId,
            [FromQuery] PagedRequest request)
        {
            try
            {
                var result = await _inventoryManagementDomain.GetPagedInventoryOperationsAsync(request, inventoryItemId);
                return ApiResult<PagedResult<InventoryOperation>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// 获取库存概览统计
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<ApiResult<InventorySummary>>> GetInventorySummary([FromQuery] int? tenantId = null)
        {
            try
            {
                var result = await _inventoryManagementDomain.GetInventorySummaryAsync(tenantId);
                return ApiResult<InventorySummary>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// 获取低库存药材列表
        /// </summary>
        [HttpGet("low-stock")]
        public async Task<ActionResult<ApiResult<List<InventoryItem>>>> GetLowStockItems([FromQuery] int? tenantId = null)
        {
            try
            {
                var result = await _inventoryManagementDomain.GetLowStockItemsAsync(tenantId);
                return ApiResult<List<InventoryItem>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// 获取临期药材列表
        /// </summary>
        [HttpGet("expiring")]
        public async Task<ActionResult<ApiResult<List<InventoryItem>>>> GetExpiringItems(
            [FromQuery] int? tenantId = null,
            [FromQuery] int days = 30)
        {
            try
            {
                var result = await _inventoryManagementDomain.GetExpiringItemsAsync(tenantId, days);
                return ApiResult<List<InventoryItem>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// 获取过期药材列表
        /// </summary>
        [HttpGet("expired")]
        public async Task<ActionResult<ApiResult<List<InventoryItem>>>> GetExpiredItems([FromQuery] int? tenantId = null)
        {
            try
            {
                var result = await _inventoryManagementDomain.GetExpiredItemsAsync(tenantId);
                return ApiResult<List<InventoryItem>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.ErrorResult(ex.Message));
            }
        }

        /// <summary>
        /// 批量更新库存状态
        /// </summary>
        [HttpPost("batch-update-status")]
        public async Task<ActionResult<ApiResult>> BatchUpdateInventoryStatus([FromQuery] int? tenantId = null)
        {
            try
            {
                await _inventoryManagementDomain.BatchUpdateInventoryStatusAsync(tenantId);
                return ApiResult.SuccessResult("库存状态批量更新成功");
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.ErrorResult(ex.Message));
            }
        }
    }
}