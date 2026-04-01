using Microsoft.EntityFrameworkCore;
using TcmAiDiagnosis.EFContext;
using TcmAiDiagnosis.Entities;
using TcmAiDiagnosis.Domain.Paged;
using TcmAiDiagnosis.Constants;

namespace TcmAiDiagnosis.Domain
{
    /// <summary>
    /// 库存管理领域服务类
    /// </summary>
    public class InventoryManagementDomain
    {
        private readonly TcmAiDiagnosisContext _context;
        private readonly InventoryAlertRuleDomain _alertRuleDomain;

        public InventoryManagementDomain(TcmAiDiagnosisContext context, InventoryAlertRuleDomain alertRuleDomain)
        {
            _context = context;
            _alertRuleDomain = alertRuleDomain;
        }

        /// <summary>
        /// 添加库存明细
        /// </summary>
        public async Task<InventoryItem> AddInventoryItemAsync(InventoryItem item)
        {
            if (string.IsNullOrWhiteSpace(item.BatchNumber))
                throw new ArgumentException("批次号不能为空");

            if (item.HerbId <= 0)
                throw new ArgumentException("药材ID不能为空");

            if (item.CurrentQuantity < 0)
                throw new ArgumentException("库存数量不能为负数");

            // 检查批次号是否重复
            var exists = await _context.InventoryItems
                .AnyAsync(i => i.BatchNumber == item.BatchNumber && i.TenantId == item.TenantId);
            if (exists)
                throw new InvalidOperationException($"批次号「{item.BatchNumber}」已存在");

            // 设置默认值
            item.InitialQuantity = item.CurrentQuantity;
            item.CreatedAt = DateTime.Now;
            item.UpdatedAt = DateTime.Now;
            item.IsActive = true;

            // 自动设置库存状态
            UpdateInventoryStatus(item);

            _context.InventoryItems.Add(item);
            await _context.SaveChangesAsync();

            return item;
        }

        /// <summary>
        /// 更新库存明细
        /// </summary>
        public async Task UpdateInventoryItemAsync(InventoryItem updatedItem)
        {
            var existingItem = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.Id == updatedItem.Id)
                ?? throw new KeyNotFoundException("未找到指定库存明细");

            // 检查批次号是否重复（排除当前明细）
            if (existingItem.BatchNumber != updatedItem.BatchNumber && await _context.InventoryItems
                .AnyAsync(i => i.BatchNumber == updatedItem.BatchNumber &&
                              i.Id != updatedItem.Id &&
                              i.TenantId == updatedItem.TenantId))
            {
                throw new InvalidOperationException($"批次号「{updatedItem.BatchNumber}」已存在");
            }

            // 更新可编辑字段
            existingItem.BatchNumber = updatedItem.BatchNumber;
            existingItem.StorageLocation = updatedItem.StorageLocation;
            existingItem.PurchasePrice = updatedItem.PurchasePrice;
            existingItem.SalePrice = updatedItem.SalePrice;
            existingItem.ProductionDate = updatedItem.ProductionDate;
            existingItem.ExpiryDate = updatedItem.ExpiryDate;
            existingItem.SupplierName = updatedItem.SupplierName;
            existingItem.QualityStatus = updatedItem.QualityStatus;
            existingItem.Remark = updatedItem.Remark;
            existingItem.UpdatedAt = DateTime.Now;

            // 更新库存状态
            UpdateInventoryStatus(existingItem);

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 根据ID获取库存明细详情
        /// </summary>
        public async Task<InventoryItem?> GetInventoryItemByIdAsync(int inventoryItemId, bool includeHerb = false)
        {
            var query = _context.InventoryItems.AsQueryable();

            if (includeHerb)
            {
                query = query.Include(i => i.Herb);
            }

            return await query.FirstOrDefaultAsync(i => i.Id == inventoryItemId);
        }

        /// <summary>
        /// 分页查询库存明细列表
        /// </summary>
        public async Task<PagedResult<InventoryItem>> GetPagedInventoryItemsAsync(PagedRequest request, int? tenantId = null, int? herbId = null, string? status = null, string? qualityStatus = null)
        {
            var query = _context.InventoryItems
                .Include(i => i.Herb)
                .AsQueryable();

            // 租户筛选
            if (tenantId.HasValue)
                query = query.Where(i => i.TenantId == tenantId);

            // 药材筛选
            if (herbId.HasValue)
                query = query.Where(i => i.HerbId == herbId.Value);

            // 状态筛选
            if (!string.IsNullOrEmpty(status))
                query = query.Where(i => i.Status == status);

            // 质量状态筛选
            if (!string.IsNullOrEmpty(qualityStatus))
                query = query.Where(i => i.QualityStatus == qualityStatus);

            // 只查询启用的库存项
            query = query.Where(i => i.IsActive);

            // 搜索筛选
            if (!string.IsNullOrWhiteSpace(request.SearchKeyword))
            {
                var keyword = request.SearchKeyword.Trim();
                query = query.Where(i =>
                    i.BatchNumber.Contains(keyword) ||
                    (i.Herb != null && i.Herb.Name.Contains(keyword)) ||
                    (i.StorageLocation != null && i.StorageLocation.Contains(keyword)) ||
                    (i.SupplierName != null && i.SupplierName.Contains(keyword)));
            }

            // 排序
            query = query.OrderByDescending(i => i.CreatedAt)
                        .ThenBy(i => i.Herb != null ? i.Herb.Name : "");

            return await query.ToPagedResultAsync(request.Page, request.PageSize);
        }

        /// <summary>
        /// 删除库存明细
        /// </summary>
        public async Task DeleteInventoryItemAsync(int id)
        {
            var item = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.Id == id)
                ?? throw new KeyNotFoundException("未找到指定库存明细");

            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 启用/禁用库存明细
        /// </summary>
        public async Task ToggleInventoryItemAsync(int id, bool isActive)
        {
            var item = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.Id == id)
                ?? throw new KeyNotFoundException("未找到指定库存明细");

            item.IsActive = isActive;
            item.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 采购入库操作
        /// </summary>
        public async Task<InventoryOperation> PurchaseInAsync(int inventoryItemId, decimal quantity, string referenceNumber, int operatedBy, string reason = "")
        {
            return await UpdateInventoryAsync(inventoryItemId, quantity, InventoryConstants.OperationType.PurchaseIn, referenceNumber, operatedBy, reason);
        }

        /// <summary>
        /// 销售出库操作
        /// </summary>
        public async Task<InventoryOperation> SaleOutAsync(int inventoryItemId, decimal quantity, string referenceNumber, int operatedBy, string reason = "")
        {
            if (quantity <= 0)
                throw new ArgumentException("出库数量必须大于0");

            return await UpdateInventoryAsync(inventoryItemId, -quantity, InventoryConstants.OperationType.SaleOut, referenceNumber, operatedBy, reason);
        }

        /// <summary>
        /// 库存调整
        /// </summary>
        public async Task<InventoryOperation> AdjustInventoryAsync(int inventoryItemId, decimal quantity, int operatedBy, string reason)
        {
            return await UpdateInventoryAsync(inventoryItemId, quantity, InventoryConstants.OperationType.Adjust, null, operatedBy, reason);
        }

        /// <summary>
        /// 库存调拨
        /// </summary>
        public async Task<(InventoryOperation OutOperation, InventoryOperation InOperation)> TransferInventoryAsync(int fromInventoryItemId, int toInventoryItemId, decimal quantity, int operatedBy, string reason)
        {
            // 先出库
            var outOperation = await UpdateInventoryAsync(fromInventoryItemId, -quantity, InventoryConstants.OperationType.Transfer, null, operatedBy, reason);

            // 再入库
            var inOperation = await UpdateInventoryAsync(toInventoryItemId, quantity, InventoryConstants.OperationType.Transfer, null, operatedBy, reason);

            return (outOperation, inOperation);
        }

        /// <summary>
        /// 退货操作
        /// </summary>
        public async Task<InventoryOperation> ReturnInventoryAsync(int inventoryItemId, decimal quantity, string referenceNumber, int operatedBy, string reason = "")
        {
            return await UpdateInventoryAsync(inventoryItemId, quantity, InventoryConstants.OperationType.Return, referenceNumber, operatedBy, reason);
        }

        /// <summary>
        /// 报损操作
        /// </summary>
        public async Task<InventoryOperation> LossInventoryAsync(int inventoryItemId, decimal quantity, int operatedBy, string reason)
        {
            return await UpdateInventoryAsync(inventoryItemId, -quantity, InventoryConstants.OperationType.Loss, null, operatedBy, reason);
        }

        /// <summary>
        /// 盘点操作
        /// </summary>
        public async Task<InventoryOperation> CheckInventoryAsync(int inventoryItemId, decimal actualQuantity, int operatedBy, string reason)
        {
            var item = await _context.InventoryItems
                .Include(i => i.Herb)
                .FirstOrDefaultAsync(i => i.Id == inventoryItemId)
                ?? throw new KeyNotFoundException("未找到指定库存明细");

            var quantity = actualQuantity - item.CurrentQuantity;

            return await UpdateInventoryAsync(inventoryItemId, quantity, InventoryConstants.OperationType.Check, null, operatedBy, reason);
        }

        /// <summary>
        /// 通用库存更新方法
        /// </summary>
        private async Task<InventoryOperation> UpdateInventoryAsync(int inventoryItemId, decimal quantity, string operationType, string referenceNumber, int operatedBy, string reason)
        {
            var item = await _context.InventoryItems
                .Include(i => i.Herb)
                .FirstOrDefaultAsync(i => i.Id == inventoryItemId)
                ?? throw new KeyNotFoundException("未找到指定库存明细");

            if (!item.IsActive)
                throw new InvalidOperationException("该库存项已被禁用，无法进行操作");

            if (quantity < 0 && item.CurrentQuantity < Math.Abs(quantity))
                throw new InvalidOperationException($"库存不足，当前库存：{item.CurrentQuantity}{item.Unit}，需求数量：{Math.Abs(quantity)}{item.Unit}");

            var beforeQuantity = item.CurrentQuantity;
            item.CurrentQuantity += quantity;
            item.UpdatedAt = DateTime.Now;

            // 更新库存状态
            UpdateInventoryStatus(item);

            var operation = new InventoryOperation
            {
                InventoryItemId = inventoryItemId,
                OperationType = operationType,
                Quantity = quantity,
                BeforeQuantity = beforeQuantity,
                AfterQuantity = item.CurrentQuantity,
                ReferenceNumber = referenceNumber,
                Reason = reason,
                OperatedBy = operatedBy,
                OperatedAt = DateTime.Now,
                TenantId = item.TenantId,
                Remark = $"库存{GetOperationTypeName(operationType)}，数量：{Math.Abs(quantity)}{item.Unit}"
            };

            _context.InventoryOperations.Add(operation);
            await _context.SaveChangesAsync();

            // 异步检查预警（不阻塞主要操作）
            _ = Task.Run(async () =>
            {
                try
                {
                    await CheckAlertsAfterOperation(item);
                }
                catch (Exception ex)
                {
                    // 记录日志，但不影响主要操作
                    Console.WriteLine($"预警检查失败：{ex.Message}");
                }
            });

            return operation;
        }

        /// <summary>
        /// 获取库存操作历史
        /// </summary>
        public async Task<PagedResult<InventoryOperation>> GetPagedInventoryOperationsAsync(PagedRequest request, int inventoryItemId)
        {
            var query = _context.InventoryOperations
                .Include(o => o.Operator)
                .Include(o => o.InventoryItem)
                    .ThenInclude(i => i.Herb)
                .Where(o => o.InventoryItemId == inventoryItemId)
                .OrderByDescending(o => o.OperatedAt);

            return await query.ToPagedResultAsync(request.Page, request.PageSize);
        }

        /// <summary>
        /// 获取所有库存操作历史（分页）
        /// </summary>
        public async Task<PagedResult<InventoryOperation>> GetPagedAllInventoryOperationsAsync(PagedRequest request, int? tenantId = null, string? operationType = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.InventoryOperations
                .Include(o => o.Operator)
                .Include(o => o.InventoryItem)
                    .ThenInclude(i => i.Herb)
                .AsQueryable();

            if (tenantId.HasValue)
                query = query.Where(o => o.TenantId == tenantId);

            if (!string.IsNullOrEmpty(operationType))
                query = query.Where(o => o.OperationType == operationType);

            if (startDate.HasValue)
                query = query.Where(o => o.OperatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(o => o.OperatedAt <= endDate.Value);

            if (!string.IsNullOrWhiteSpace(request.SearchKeyword))
            {
                var keyword = request.SearchKeyword.Trim();
                query = query.Where(o =>
                    o.InventoryItem.BatchNumber.Contains(keyword) ||
                    (o.InventoryItem.Herb != null && o.InventoryItem.Herb.Name.Contains(keyword)) ||
                    (o.ReferenceNumber != null && o.ReferenceNumber.Contains(keyword)) ||
                    o.Reason.Contains(keyword));
            }

            query = query.OrderByDescending(o => o.OperatedAt);

            return await query.ToPagedResultAsync(request.Page, request.PageSize);
        }

        /// <summary>
        /// 获取库存概览统计
        /// </summary>
        public async Task<InventorySummary> GetInventorySummaryAsync(int? tenantId = null)
        {
            var items = await _context.InventoryItems
                .Include(i => i.Herb)
                .Where(i => i.TenantId == tenantId && i.IsActive)
                .ToListAsync();

            return new InventorySummary
            {
                TotalItems = items.Count,
                TotalValue = items.Sum(i => i.CurrentQuantity * i.PurchasePrice),
                LowStockCount = items.Count(i => i.Status == InventoryConstants.Status.LowStock),
                ExpiringCount = items.Count(i => i.Status == InventoryConstants.Status.Expiring),
                ExpiredCount = items.Count(i => i.Status == InventoryConstants.Status.Expired),
                OutOfStockCount = items.Count(i => i.Status == InventoryConstants.Status.OutOfStock),
                TotalHerbTypes = items.Select(i => i.HerbId).Distinct().Count(),
                TotalInventoryValue = items.Sum(i => i.CurrentQuantity * i.PurchasePrice),
                TotalActiveItems = items.Count(i => i.IsActive)
            };
        }

        /// <summary>
        /// 获取低库存药材列表
        /// </summary>
        public async Task<List<InventoryItem>> GetLowStockItemsAsync(int? tenantId = null)
        {
            return await _context.InventoryItems
                .Include(i => i.Herb)
                .Where(i => i.TenantId == tenantId &&
                           i.IsActive &&
                           i.Status == InventoryConstants.Status.LowStock)
                .OrderBy(i => i.CurrentQuantity)
                .ThenBy(i => i.Herb != null ? i.Herb.Name : "")
                .ToListAsync();
        }

        /// <summary>
        /// 获取临期药材列表
        /// </summary>
        public async Task<List<InventoryItem>> GetExpiringItemsAsync(int? tenantId = null, int days = 30)
        {
            var targetDate = DateTime.Now.AddDays(days);

            return await _context.InventoryItems
                .Include(i => i.Herb)
                .Where(i => i.TenantId == tenantId &&
                           i.IsActive &&
                           i.ExpiryDate.HasValue &&
                           i.ExpiryDate <= targetDate &&
                           i.ExpiryDate > DateTime.Now)
                .OrderBy(i => i.ExpiryDate)
                .ThenBy(i => i.Herb != null ? i.Herb.Name : "")
                .ToListAsync();
        }

        /// <summary>
        /// 获取过期药材列表
        /// </summary>
        public async Task<List<InventoryItem>> GetExpiredItemsAsync(int? tenantId = null)
        {
            return await _context.InventoryItems
                .Include(i => i.Herb)
                .Where(i => i.TenantId == tenantId &&
                           i.IsActive &&
                           i.ExpiryDate.HasValue &&
                           i.ExpiryDate <= DateTime.Now)
                .OrderBy(i => i.ExpiryDate)
                .ThenBy(i => i.Herb != null ? i.Herb.Name : "")
                .ToListAsync();
        }

        /// <summary>
        /// 根据药材ID获取库存项
        /// </summary>
        public async Task<List<InventoryItem>> GetInventoryItemsByHerbAsync(int herbId, int? tenantId = null)
        {
            return await _context.InventoryItems
                .Include(i => i.Herb)
                .Where(i => i.HerbId == herbId &&
                           i.TenantId == tenantId &&
                           i.IsActive)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// 批量更新库存状态
        /// </summary>
        public async Task BatchUpdateInventoryStatusAsync(int? tenantId = null)
        {
            var items = await _context.InventoryItems
                .Where(i => i.TenantId == tenantId && i.IsActive)
                .ToListAsync();

            foreach (var item in items)
            {
                UpdateInventoryStatus(item);
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 更新库存状态
        /// </summary>
        private void UpdateInventoryStatus(InventoryItem item)
        {
            if (item.CurrentQuantity <= 0)
            {
                item.Status = InventoryConstants.Status.OutOfStock;
            }
            else if (item.CurrentQuantity < item.InitialQuantity * 0.1m) // 低于初始库存10%为低库存
            {
                item.Status = InventoryConstants.Status.LowStock;
            }
            else if (item.ExpiryDate.HasValue && (item.ExpiryDate.Value - DateTime.Now).Days <= 30)
            {
                item.Status = InventoryConstants.Status.Expiring;
            }
            else if (item.ExpiryDate.HasValue && DateTime.Now > item.ExpiryDate.Value)
            {
                item.Status = InventoryConstants.Status.Expired;
            }
            else
            {
                item.Status = InventoryConstants.Status.Normal;
            }
        }

        /// <summary>
        /// 获取操作类型名称
        /// </summary>
        private string GetOperationTypeName(string operationType)
        {
            return operationType switch
            {
                InventoryConstants.OperationType.PurchaseIn => "入库",
                InventoryConstants.OperationType.SaleOut => "出库",
                InventoryConstants.OperationType.Adjust => "调整",
                InventoryConstants.OperationType.Transfer => "调拨",
                InventoryConstants.OperationType.Return => "退货",
                InventoryConstants.OperationType.Loss => "报损",
                InventoryConstants.OperationType.Check => "盘点",
                _ => "未知操作"
            };
        }

        /// <summary>
        /// 操作后检查预警
        /// </summary>
        private async Task CheckAlertsAfterOperation(InventoryItem item)
        {
            var alerts = await _alertRuleDomain.CheckInventoryAlertsAsync(item.TenantId);
            var relevantAlerts = alerts.Where(a => a.InventoryItemId == item.Id).ToList();

            if (relevantAlerts.Any())
            {
                await TriggerAlertNotification(relevantAlerts);
            }
        }

        /// <summary>
        /// 触发预警通知
        /// </summary>
        private async Task TriggerAlertNotification(List<InventoryAlertResult> alerts)
        {
            // 实现通知逻辑
            foreach (var alert in alerts)
            {
                // 这里可以集成邮件、短信、系统消息等通知方式
                Console.WriteLine($"库存预警：{alert.AlertMessage}");

                // 实际项目中可以调用通知服务
                // await _notificationService.SendAlertAsync(alert);
            }

            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// 库存概览信息
    /// </summary>
    public class InventorySummary
    {
        public int TotalItems { get; set; }
        public decimal TotalValue { get; set; }
        public int LowStockCount { get; set; }
        public int ExpiringCount { get; set; }
        public int ExpiredCount { get; set; }
        public int OutOfStockCount { get; set; }
        public int TotalHerbTypes { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public int TotalActiveItems { get; set; }
    }
}