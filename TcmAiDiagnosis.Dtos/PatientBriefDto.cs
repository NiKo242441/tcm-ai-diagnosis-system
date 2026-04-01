namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 患者摘要信息DTO
    /// </summary>
    public class PatientBriefDto
    {
        /// <summary>
        /// 患者ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 患者姓名
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// 性别
        /// </summary>
        public string Gender { get; set; } = string.Empty;

        /// <summary>
        /// 年龄
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// 体质
        /// </summary>
        public string Constitution { get; set; } = string.Empty;
    }
}