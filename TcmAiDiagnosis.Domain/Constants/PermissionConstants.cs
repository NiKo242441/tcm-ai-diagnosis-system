namespace TcmAiDiagnosis.Domain.Constants
{
    /// <summary>
    /// 权限常量定义 - 定义系统中所有权限点
    /// </summary>
    public static class PermissionConstants
    {
        #region 平台超级管理员权限

        /// <summary>
        /// 租户管理权限
        /// </summary>
        public static class TenantManagement
        {
            public const string Create = "tenant.create";
            public const string Edit = "tenant.edit";
            public const string View = "tenant.view";
            public const string Delete = "tenant.delete";
            public const string Enable = "tenant.enable";
            public const string Disable = "tenant.disable";
            public const string ConfigurePackage = "tenant.configure_package";
            public const string SetAdmin = "tenant.set_admin";
        }

        /// <summary>
        /// 系统配置权限
        /// </summary>
        public static class SystemConfiguration
        {
            public const string ConfigureGlobal = "system.configure_global";
            public const string ManageKnowledgeBase = "system.manage_knowledge_base";
            public const string ConfigureMonitoring = "system.configure_monitoring";
        }

        /// <summary>
        /// 平台级权限管控
        /// </summary>
        public static class PlatformPermissionControl
        {
            public const string DefineRoleTemplate = "platform.define_role_template";
            public const string ViewAllPermissions = "platform.view_all_permissions";
            public const string HandleCrossTenantIssues = "platform.handle_cross_tenant_issues";
        }

        /// <summary>
        /// 平台级运维监控
        /// </summary>
        public static class PlatformOperationMonitoring
        {
            public const string MonitorResources = "platform.monitor_resources";
            public const string ViewAllLogs = "platform.view_all_logs";
            public const string HandleSupport = "platform.handle_support";
        }

        /// <summary>
        /// 数据安全权限
        /// </summary>
        public static class DataSecurity
        {
            public const string ConfigureEncryption = "data.configure_encryption";
            public const string ConfigureBackup = "data.configure_backup";
            public const string ExecuteBackup = "data.execute_backup";
            public const string ExecuteRestore = "data.execute_restore";
            public const string ViewIsolationStatus = "data.view_isolation_status";
        }

        #endregion

        #region 租户管理员权限

        /// <summary>
        /// 用户管理权限
        /// </summary>
        public static class UserManagement
        {
            public const string CreateDoctor = "user.create_doctor";
            public const string EditDoctor = "user.edit_doctor";
            public const string ViewDoctor = "user.view_doctor";
            public const string EnableDoctor = "user.enable_doctor";
            public const string DisableDoctor = "user.disable_doctor";
            public const string AssignRole = "user.assign_role";
            public const string ResetPassword = "user.reset_password";
        }

        /// <summary>
        /// 租户配置权限
        /// </summary>
        public static class TenantConfiguration
        {
            public const string EditBasicInfo = "tenant_config.edit_basic_info";
            public const string EnableModules = "tenant_config.enable_modules";
            public const string ConfigureWorkflow = "tenant_config.configure_workflow";
            public const string SetQuotas = "tenant_config.set_quotas";
        }

        /// <summary>
        /// 租户级监控管理
        /// </summary>
        public static class TenantMonitoring
        {
            public const string ViewUsageStats = "tenant_monitor.view_usage_stats";
            public const string ViewOperationLogs = "tenant_monitor.view_operation_logs";
            public const string HandleIssues = "tenant_monitor.handle_issues";
        }

        /// <summary>
        /// 租户级数据管理
        /// </summary>
        public static class TenantDataManagement
        {
            public const string ViewStorageUsage = "tenant_data.view_storage_usage";
            public const string ConfigureBackup = "tenant_data.configure_backup";
            public const string HandleDataIssues = "tenant_data.handle_data_issues";
        }

        #endregion

        #region 中医师权限

        /// <summary>
        /// 患者管理权限
        /// </summary>
        public static class PatientManagement
        {
            public const string Create = "patient.create";
            public const string Edit = "patient.edit";
            public const string View = "patient.view";
            public const string Search = "patient.search";
            public const string AdvancedSearch = "patient.advanced_search";
            public const string Associate = "patient.associate";
            public const string Disassociate = "patient.disassociate";
            public const string AddCrossTenant = "patient.add_cross_tenant";
            public const string ViewCrossTenantBasic = "patient.view_cross_tenant_basic";
        }

        /// <summary>
        /// 诊疗操作权限
        /// </summary>
        public static class DiagnosisOperation
        {
            public const string CreateVisit = "diagnosis.create_visit";
            public const string EditVisit = "diagnosis.edit_visit";
            public const string ViewVisit = "diagnosis.view_visit";
            public const string RecordSymptoms = "diagnosis.record_symptoms";
            public const string CollectFourDiagnosis = "diagnosis.collect_four_diagnosis";
            public const string UseAiDiagnosis = "diagnosis.use_ai_diagnosis";
            public const string ConfirmDiagnosis = "diagnosis.confirm_diagnosis";
            public const string ModifyDiagnosis = "diagnosis.modify_diagnosis";
        }

        /// <summary>
        /// 治疗方案权限
        /// </summary>
        public static class TreatmentPlan
        {
            public const string Create = "treatment.create";
            public const string Edit = "treatment.edit";
            public const string View = "treatment.view";
            public const string UseAiGeneration = "treatment.use_ai_generation";
            public const string SaveDraft = "treatment.save_draft";
            public const string Submit = "treatment.submit";
            public const string ViewHistory = "treatment.view_history";
            public const string HandleRiskWarning = "treatment.handle_risk_warning";
            public const string CreatePrescription = "treatment.create_prescription";
            public const string EditPrescription = "treatment.edit_prescription";
            public const string CreateAcupuncture = "treatment.create_acupuncture";
            public const string CreateMoxibustion = "treatment.create_moxibustion";
            public const string CreateCupping = "treatment.create_cupping";
            public const string CreateDietaryTherapy = "treatment.create_dietary_therapy";
            public const string CreateLifestyleAdvice = "treatment.create_lifestyle_advice";
        }

        /// <summary>
        /// 病历管理权限
        /// </summary>
        public static class MedicalRecordManagement
        {
            public const string Create = "medical_record.create";
            public const string Edit = "medical_record.edit";
            public const string View = "medical_record.view";
            public const string UseTemplate = "medical_record.use_template";
            public const string Sign = "medical_record.sign";
            public const string Print = "medical_record.print";
            public const string Export = "medical_record.export";
            public const string ShareForConsultation = "medical_record.share_for_consultation";
        }

        /// <summary>
        /// 知识库权限
        /// </summary>
        public static class KnowledgeBase
        {
            public const string ViewTheory = "knowledge.view_theory";
            public const string ViewFormula = "knowledge.view_formula";
            public const string ViewHerb = "knowledge.view_herb";
            public const string ViewAcupoint = "knowledge.view_acupoint";
            public const string ViewDiagnosisRules = "knowledge.view_diagnosis_rules";
        }

        /// <summary>
        /// 医案管理权限
        /// </summary>
        public static class MedicalCaseManagement
        {
            public const string ViewAiGenerated = "medical_case.view_ai_generated";
            public const string Review = "medical_case.review";
            public const string Edit = "medical_case.edit";
            public const string Confirm = "medical_case.confirm";
            public const string Export = "medical_case.export";
            public const string ViewSeries = "medical_case.view_series";
        }

        #endregion

        #region 患者权限

        /// <summary>
        /// 个人信息权限
        /// </summary>
        public static class PersonalInformation
        {
            public const string ViewBasic = "personal.view_basic";
            public const string ViewMedical = "personal.view_medical";
        }

        /// <summary>
        /// 诊疗记录查看权限
        /// </summary>
        public static class DiagnosisRecordView
        {
            public const string ViewHistory = "diagnosis_view.view_history";
            public const string ViewFourDiagnosis = "diagnosis_view.view_four_diagnosis";
            public const string ViewDiagnosisResult = "diagnosis_view.view_diagnosis_result";
        }

        /// <summary>
        /// 治疗方案查看权限
        /// </summary>
        public static class TreatmentPlanView
        {
            public const string ViewPrescription = "treatment_view.view_prescription";
            public const string ViewNonDrugTherapy = "treatment_view.view_non_drug_therapy";
            public const string ViewAdvice = "treatment_view.view_advice";
        }

        /// <summary>
        /// 病历查看权限
        /// </summary>
        public static class MedicalRecordView
        {
            public const string View = "medical_record_view.view";
            public const string Print = "medical_record_view.print";
            public const string Export = "medical_record_view.export";
        }

        #endregion

        #region 特殊场景权限

        /// <summary>
        /// 会诊场景权限
        /// </summary>
        public static class Consultation
        {
            public const string ViewSharedRecord = "consultation.view_shared_record";
            public const string ParticipateConsultation = "consultation.participate";
        }

        /// <summary>
        /// 跨租户转诊权限
        /// </summary>
        public static class CrossTenantReferral
        {
            public const string TransferRecord = "referral.transfer_record";
            public const string ReceiveRecord = "referral.receive_record";
            public const string ViewReferralRecord = "referral.view_referral_record";
        }

        /// <summary>
        /// 系统维护权限
        /// </summary>
        public static class SystemMaintenance
        {
            public const string AccessTenantData = "maintenance.access_tenant_data";
            public const string PerformMaintenance = "maintenance.perform";
        }

        #endregion
    }
}
