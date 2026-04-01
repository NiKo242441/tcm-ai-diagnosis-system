using System;

namespace TcmAiDiagnosis.Entities.Dtos
{
    /// <summary>
    /// 跨租户患者搜索结果DTO
    /// </summary>
    public class PatientSearchResultDto
    {
        public int PatientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? IdCard { get; set; } // 已脱敏
        public string? OriginalTenantName { get; set; }
        public bool IsLinkedToCurrentTenant { get; set; }
        public bool CanAddToCurrentTenant { get; set; }
        public int LinkedTenantCount { get; set; }
    }

    /// <summary>
    /// 添加现有患者请求DTO
    /// </summary>
    public class AddExistingPatientRequest
    {
        public int PatientId { get; set; }
        public string? Reason { get; set; } // 添加原因
    }

    /// <summary>
    /// 跨租户患者搜索请求DTO
    /// </summary>
    public class CrossTenantPatientSearchRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
    }
}