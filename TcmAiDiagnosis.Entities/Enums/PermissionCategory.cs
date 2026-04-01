namespace TcmAiDiagnosis.Entities.Enums
{
    /// <summary>
    /// 权限分类枚举
    /// </summary>
    public enum PermissionCategory
    {
        /// <summary>
        /// 租户管理权限（平台超级管理员）
        /// </summary>
        TenantManagement = 1,

        /// <summary>
        /// 系统配置权限（平台超级管理员）
        /// </summary>
        SystemConfiguration = 2,

        /// <summary>
        /// 权限管控权限（平台超级管理员、租户管理员）
        /// </summary>
        PermissionControl = 3,

        /// <summary>
        /// 运维监控权限（平台超级管理员、租户管理员）
        /// </summary>
        OperationMonitoring = 4,

        /// <summary>
        /// 数据安全权限（平台超级管理员）
        /// </summary>
        DataSecurity = 5,

        /// <summary>
        /// 用户管理权限（租户管理员）
        /// </summary>
        UserManagement = 6,

        /// <summary>
        /// 租户配置权限（租户管理员）
        /// </summary>
        TenantConfiguration = 7,

        /// <summary>
        /// 患者管理权限（中医师）
        /// </summary>
        PatientManagement = 8,

        /// <summary>
        /// 诊疗操作权限（中医师）
        /// </summary>
        DiagnosisOperation = 9,

        /// <summary>
        /// 治疗方案权限（中医师）
        /// </summary>
        TreatmentPlan = 10,

        /// <summary>
        /// 病历管理权限（中医师）
        /// </summary>
        MedicalRecordManagement = 11,

        /// <summary>
        /// 知识库权限（中医师）
        /// </summary>
        KnowledgeBase = 12,

        /// <summary>
        /// 医案管理权限（中医师）
        /// </summary>
        MedicalCaseManagement = 13,

        /// <summary>
        /// 个人信息权限（患者）
        /// </summary>
        PersonalInformation = 14,

        /// <summary>
        /// 诊疗记录查看权限（患者）
        /// </summary>
        DiagnosisRecordView = 15,

        /// <summary>
        /// 治疗方案查看权限（患者）
        /// </summary>
        TreatmentPlanView = 16,

        /// <summary>
        /// 病历查看权限（患者）
        /// </summary>
        MedicalRecordView = 17
    }
}
