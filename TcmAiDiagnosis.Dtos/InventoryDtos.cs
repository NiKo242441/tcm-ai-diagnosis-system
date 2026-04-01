using System.ComponentModel.DataAnnotations;

namespace TcmAiDiagnosis.Dtos
{
    public class CreateInventoryItemDto
    {
        [Required(ErrorMessage = "批次号不能为空")]
        [StringLength(100, ErrorMessage = "批次号长度不能超过100个字符")]
        public string BatchNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "药材ID不能为空")]
        [Range(1, int.MaxValue, ErrorMessage = "药材ID必须大于0")]
        public int HerbId { get; set; }

        [Required(ErrorMessage = "当前库存数量不能为空")]
        [Range(0, double.MaxValue, ErrorMessage = "库存数量不能为负数")]
        public decimal CurrentQuantity { get; set; }

        [Required(ErrorMessage = "计量单位不能为空")]
        [StringLength(20, ErrorMessage = "计量单位长度不能超过20个字符")]
        public string Unit { get; set; } = "克";

        [StringLength(100, ErrorMessage = "库存位置长度不能超过100个字符")]
        public string? StorageLocation { get; set; }

        [Required(ErrorMessage = "采购单价不能为空")]
        [Range(0, double.MaxValue, ErrorMessage = "采购单价不能为负数")]
        public decimal PurchasePrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "销售单价不能为负数")]
        public decimal? SalePrice { get; set; }

        public DateTime? ProductionDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [StringLength(200, ErrorMessage = "供应商名称长度不能超过200个字符")]
        public string? SupplierName { get; set; }

        [Required(ErrorMessage = "质量状态不能为空")]
        [StringLength(20, ErrorMessage = "质量状态长度不能超过20个字符")]
        public string QualityStatus { get; set; } = "Pending";

        [StringLength(500, ErrorMessage = "备注长度不能超过500个字符")]
        public string? Remark { get; set; }

        [Required(ErrorMessage = "租户ID不能为空")]
        public int TenantId { get; set; }

        public int? CreatedBy { get; set; }
    }

    public class UpdateInventoryItemDto
    {
        [Required(ErrorMessage = "批次号不能为空")]
        [StringLength(100, ErrorMessage = "批次号长度不能超过100个字符")]
        public string BatchNumber { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "库存位置长度不能超过100个字符")]
        public string? StorageLocation { get; set; }

        [Required(ErrorMessage = "采购单价不能为空")]
        [Range(0, double.MaxValue, ErrorMessage = "采购单价不能为负数")]
        public decimal PurchasePrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "销售单价不能为负数")]
        public decimal? SalePrice { get; set; }

        public DateTime? ProductionDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [StringLength(200, ErrorMessage = "供应商名称长度不能超过200个字符")]
        public string? SupplierName { get; set; }

        [Required(ErrorMessage = "质量状态不能为空")]
        [StringLength(20, ErrorMessage = "质量状态长度不能超过20个字符")]
        public string QualityStatus { get; set; } = "Pending";

        [StringLength(500, ErrorMessage = "备注长度不能超过500个字符")]
        public string? Remark { get; set; }

        [Required(ErrorMessage = "租户ID不能为空")]
        public int TenantId { get; set; }
    }

    public class InventoryOperationDto
    {
        [Required(ErrorMessage = "库存项ID不能为空")]
        [Range(1, int.MaxValue, ErrorMessage = "库存项ID必须大于0")]
        public int InventoryItemId { get; set; }

        [Required(ErrorMessage = "操作数量不能为空")]
        [Range(0.01, double.MaxValue, ErrorMessage = "操作数量必须大于0")]
        public decimal Quantity { get; set; }

        [StringLength(100, ErrorMessage = "参考单号长度不能超过100个字符")]
        public string? ReferenceNumber { get; set; }

        [Required(ErrorMessage = "操作人不能为空")]
        [Range(1, int.MaxValue, ErrorMessage = "操作人ID必须大于0")]
        public int OperatedBy { get; set; }

        [Required(ErrorMessage = "操作原因不能为空")]
        [StringLength(500, ErrorMessage = "操作原因长度不能超过500个字符")]
        public string Reason { get; set; } = string.Empty;
    }

    public class AdjustInventoryDto
    {
        [Required(ErrorMessage = "库存项ID不能为空")]
        [Range(1, int.MaxValue, ErrorMessage = "库存项ID必须大于0")]
        public int InventoryItemId { get; set; }

        [Required(ErrorMessage = "调整数量不能为空")]
        public decimal Quantity { get; set; }

        [Required(ErrorMessage = "操作人不能为空")]
        [Range(1, int.MaxValue, ErrorMessage = "操作人ID必须大于0")]
        public int OperatedBy { get; set; }

        [Required(ErrorMessage = "调整原因不能为空")]
        [StringLength(500, ErrorMessage = "调整原因长度不能超过500个字符")]
        public string Reason { get; set; } = string.Empty;
    }

    public class TransferInventoryDto
    {
        [Required(ErrorMessage = "源库存项ID不能为空")]
        [Range(1, int.MaxValue, ErrorMessage = "源库存项ID必须大于0")]
        public int FromInventoryItemId { get; set; }

        [Required(ErrorMessage = "目标库存项ID不能为空")]
        [Range(1, int.MaxValue, ErrorMessage = "目标库存项ID必须大于0")]
        public int ToInventoryItemId { get; set; }

        [Required(ErrorMessage = "调拨数量不能为空")]
        [Range(0.01, double.MaxValue, ErrorMessage = "调拨数量必须大于0")]
        public decimal Quantity { get; set; }

        [Required(ErrorMessage = "操作人不能为空")]
        [Range(1, int.MaxValue, ErrorMessage = "操作人ID必须大于0")]
        public int OperatedBy { get; set; }

        [Required(ErrorMessage = "调拨原因不能为空")]
        [StringLength(500, ErrorMessage = "调拨原因长度不能超过500个字符")]
        public string Reason { get; set; } = string.Empty;
    }

    public class InventoryQueryDto
    {
        public int PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "每页大小必须在1-100之间")]
        public int PageSize { get; set; } = 10;

        public string? SearchKeyword { get; set; }

        public int? TenantId { get; set; }

        public int? HerbId { get; set; }

        public string? Status { get; set; }

        public string? QualityStatus { get; set; }

        public bool? IncludeHerb { get; set; } = true;
    }

    public class InventoryItemResponseDto
    {
        public int Id { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public int HerbId { get; set; }
        public string HerbName { get; set; } = string.Empty;
        public decimal CurrentQuantity { get; set; }
        public decimal InitialQuantity { get; set; }
        public string Unit { get; set; } = "克";
        public string? StorageLocation { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal? SalePrice { get; set; }
        public DateTime? ProductionDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? SupplierName { get; set; }
        public string Status { get; set; } = "Normal";
        public string StatusName { get; set; } = string.Empty;
        public string QualityStatus { get; set; } = "Pending";
        public string QualityStatusName { get; set; } = string.Empty;
        public int? TenantId { get; set; }
        public string? TenantName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatorName { get; set; }
        public string? Remark { get; set; }

        // 计算属性
        public decimal TotalPurchaseAmount { get; set; }
        public decimal TotalValue { get; set; }
        public int? DaysUntilExpiry { get; set; }
        public bool IsExpired { get; set; }
        public bool IsLowStock { get; set; }
    }

    public class InventoryOperationResponseDto
    {
        public int Id { get; set; }
        public int InventoryItemId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public string HerbName { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public string OperationTypeName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal BeforeQuantity { get; set; }
        public decimal AfterQuantity { get; set; }
        public string? ReferenceNumber { get; set; }
        public string Reason { get; set; } = string.Empty;
        public int OperatedBy { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public DateTime OperatedAt { get; set; }
        public int? TenantId { get; set; }
        public string? TenantName { get; set; }
        public string? Remark { get; set; }
    }

    public class InventorySummaryDto
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

    public class InventoryAlertCheckDto
    {
        public int? TenantId { get; set; }
        public bool IncludeDetails { get; set; } = true;
        public bool OnlyEnabledRules { get; set; } = true;
    }
}