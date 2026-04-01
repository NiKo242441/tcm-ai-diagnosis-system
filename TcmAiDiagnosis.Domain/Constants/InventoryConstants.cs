namespace TcmAiDiagnosis.Constants
{
    /// <summary>
    /// 库存相关常量
    /// </summary>
    public static class InventoryConstants
    {
        /// <summary>
        /// 库存状态常量
        /// </summary>
        public static class Status
        {
            public const string Normal = "Normal";
            public const string LowStock = "LowStock";
            public const string Expiring = "Expiring";
            public const string Expired = "Expired";
            public const string OutOfStock = "OutOfStock";
        }

        /// <summary>
        /// 质量状态常量
        /// </summary>
        public static class QualityStatus
        {
            public const string Pending = "Pending";
            public const string Qualified = "Qualified";
            public const string Unqualified = "Unqualified";
        }

        /// <summary>
        /// 预警类型常量
        /// </summary>
        public static class AlertType
        {
            public const string LowStock = "LowStock";
            public const string Expiring = "Expiring";
            public const string Overstock = "Overstock";
        }

        /// <summary>
        /// 比较运算符常量
        /// </summary>
        public static class ComparisonOperator
        {
            public const string LessThan = "LT";
            public const string GreaterThan = "GT";
            public const string LessThanOrEqual = "LTE";
            public const string GreaterThanOrEqual = "GTE";
        }

        /// <summary>
        /// 操作类型常量
        /// </summary>
        public static class OperationType
        {
            public const string PurchaseIn = "PurchaseIn";
            public const string SaleOut = "SaleOut";
            public const string Adjust = "Adjust";
            public const string Transfer = "Transfer";
            public const string Return = "Return";
            public const string Loss = "Loss";
            public const string Check = "Check";
        }
    }
}