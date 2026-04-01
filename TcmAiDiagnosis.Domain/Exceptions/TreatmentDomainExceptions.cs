using System;

namespace TcmAiDiagnosis.Domain.Exceptions
{
    /// <summary>
    /// 治疗方案领域异常基类
    /// </summary>
    public abstract class TreatmentDomainException : Exception
    {
        /// <summary>
        /// 证候ID
        /// </summary>
        public int? SyndromeId { get; }

        /// <summary>
        /// 治疗方案ID
        /// </summary>
        public int? TreatmentId { get; }

        /// <summary>
        /// 错误代码
        /// </summary>
        public string ErrorCode { get; }

        protected TreatmentDomainException(string message, string errorCode, int? syndromeId = null, int? treatmentId = null) 
            : base(message)
        {
            ErrorCode = errorCode;
            SyndromeId = syndromeId;
            TreatmentId = treatmentId;
        }

        protected TreatmentDomainException(string message, string errorCode, Exception innerException, int? syndromeId = null, int? treatmentId = null) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            SyndromeId = syndromeId;
            TreatmentId = treatmentId;
        }
    }

    /// <summary>
    /// 证候不存在异常
    /// </summary>
    public class SyndromeNotFoundException : TreatmentDomainException
    {
        public SyndromeNotFoundException(int syndromeId) 
            : base($"证候 {syndromeId} 不存在", "SYNDROME_NOT_FOUND", syndromeId)
        {
        }
    }

    /// <summary>
    /// 证候数据无效异常
    /// </summary>
    public class InvalidSyndromeDataException : TreatmentDomainException
    {
        public InvalidSyndromeDataException(int syndromeId, string reason) 
            : base($"证候 {syndromeId} 数据无效：{reason}", "INVALID_SYNDROME_DATA", syndromeId)
        {
        }
    }

    /// <summary>
    /// 治疗方案已存在异常
    /// </summary>
    public class TreatmentAlreadyExistsException : TreatmentDomainException
    {
        public TreatmentAlreadyExistsException(int syndromeId, int treatmentId) 
            : base($"证候 {syndromeId} 的治疗方案已存在（ID: {treatmentId}）", "TREATMENT_ALREADY_EXISTS", syndromeId, treatmentId)
        {
        }
    }

    /// <summary>
    /// 治疗方案生成失败异常
    /// </summary>
    public class TreatmentGenerationFailedException : TreatmentDomainException
    {
        public TreatmentGenerationFailedException(int syndromeId, string reason) 
            : base($"生成证候 {syndromeId} 的治疗方案失败：{reason}", "TREATMENT_GENERATION_FAILED", syndromeId)
        {
        }

        public TreatmentGenerationFailedException(int syndromeId, string reason, Exception innerException) 
            : base($"生成证候 {syndromeId} 的治疗方案失败：{reason}", "TREATMENT_GENERATION_FAILED", innerException, syndromeId)
        {
        }
    }

    /// <summary>
    /// Dify API调用异常
    /// </summary>
    public class DifyApiException : TreatmentDomainException
    {
        /// <summary>
        /// HTTP状态码
        /// </summary>
        public int? HttpStatusCode { get; }

        /// <summary>
        /// API响应内容
        /// </summary>
        public string? ResponseContent { get; }

        public DifyApiException(string message, int? httpStatusCode = null, string? responseContent = null) 
            : base(message, "DIFY_API_ERROR")
        {
            HttpStatusCode = httpStatusCode;
            ResponseContent = responseContent;
        }

        public DifyApiException(string message, Exception innerException, int? httpStatusCode = null, string? responseContent = null) 
            : base(message, "DIFY_API_ERROR", innerException)
        {
            HttpStatusCode = httpStatusCode;
            ResponseContent = responseContent;
        }
    }

    /// <summary>
    /// API响应解析异常
    /// </summary>
    public class ApiResponseParseException : TreatmentDomainException
    {
        /// <summary>
        /// 原始响应内容
        /// </summary>
        public string ResponseContent { get; }

        public ApiResponseParseException(string message, string responseContent) 
            : base(message, "API_RESPONSE_PARSE_ERROR")
        {
            ResponseContent = responseContent;
        }

        public ApiResponseParseException(string message, string responseContent, Exception innerException) 
            : base(message, "API_RESPONSE_PARSE_ERROR", innerException)
        {
            ResponseContent = responseContent;
        }
    }

    /// <summary>
    /// 数据验证异常
    /// </summary>
    public class TreatmentDataValidationException : TreatmentDomainException
    {
        /// <summary>
        /// 验证错误详情
        /// </summary>
        public string[] ValidationErrors { get; }

        public TreatmentDataValidationException(string message, params string[] validationErrors) 
            : base(message, "TREATMENT_DATA_VALIDATION_ERROR")
        {
            ValidationErrors = validationErrors ?? new string[0];
        }
    }

    /// <summary>
    /// 并发锁竞争异常
    /// </summary>
    public class ConcurrencyLockException : TreatmentDomainException
    {
        public ConcurrencyLockException(int syndromeId) 
            : base($"证候 {syndromeId} 的治疗方案正在生成中，请稍后重试", "CONCURRENCY_LOCK_ERROR", syndromeId)
        {
        }
    }

    /// <summary>
    /// 配置无效异常
    /// </summary>
    public class InvalidConfigurationException : TreatmentDomainException
    {
        /// <summary>
        /// 配置节名称
        /// </summary>
        public string ConfigurationSection { get; }

        public InvalidConfigurationException(string configurationSection, string message) 
            : base($"配置节 '{configurationSection}' 无效：{message}", "INVALID_CONFIGURATION")
        {
            ConfigurationSection = configurationSection;
        }
    }
}