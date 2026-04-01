using System.Collections.Generic;

namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 系统同步初审结果 DTO
    /// </summary>
    public class SystemCheckResultDto
    {
        public RiskLevel RiskLevel { get; set; } = RiskLevel.Low;
        public bool RequiresVerification { get; set; } = false;
        public string VerificationType { get; set; } = string.Empty; // MediumConfirm / HighPassword / CriticalPassword
        public List<string> Warnings { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// 风险等级枚举（用于页面交互）
    /// </summary>
    public enum RiskLevel
    {
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>
    /// 同步验证请求 DTO
    /// </summary>
    public class SystemCheckVerificationRequestDto
    {
        public int TreatmentId { get; set; }
        public int DoctorId { get; set; }
        public string VerificationType { get; set; } = string.Empty;
        public bool IsConfirmed { get; set; } = false; // 用于 MediumConfirm
        public string Password { get; set; } = string.Empty; // 用于密码验证
        public string Reason { get; set; } = string.Empty; // 高/极高风险时的理由
    }
}