//using TcmDatabase.Enums; // Assuming your enums are in this namespace

namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 个人与家族病史信息表
    /// </summary>
    public class MedicalHistory
    {
        /// <summary>
        /// 病史ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 病史名称 (例如: 高血压、糖尿病)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 病史类型：慢性疾病、传染病史、过敏史、家族病史
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
    }
}