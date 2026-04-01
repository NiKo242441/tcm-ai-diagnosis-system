namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 高风险验证请求DTO - 用于前端向后端发起高风险操作的验证请求
    /// </summary>
    public class HighRiskVerificationRequestDto
    {
        /// <summary>
        /// 治疗方案ID
        /// </summary>
        public int TreatmentId { get; set; }

        /// <summary>
        /// 操作类型 (例如: 'HighRiskOverride')
        /// </summary>
        public string OperationType { get; set; } = string.Empty;

        /// <summary>
        /// 验证凭据 (例如: 医生输入的密码)
        /// </summary>
        public string VerificationCredential { get; set; } = string.Empty;

        /// <summary>
        /// 执行此操作的理由
        /// </summary>
        public string Reason { get; set; } = string.Empty;
    }
}