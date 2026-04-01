using TcmAiDiagnosis.EFContext;
using TcmAiDiagnosis.Entities;
using TcmAiDiagnosis.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace TcmAiDiagnosis.Web.Data
{
    /// <summary>
    /// 权限数据种子 - 初始化系统权限
    /// </summary>
    public static class PermissionSeeder
    {
        /// <summary>
        /// 初始化权限数据
        /// </summary>
        public static async Task SeedPermissionsAsync(TcmAiDiagnosisContext context)
        {
            // 检查是否已有权限数据
            if (await context.Permissions.AnyAsync())
            {
                return; // 已有数据，跳过初始化
            }

            var permissions = new List<Permission>();
            var now = DateTime.UtcNow;

            // 平台超级管理员权限
            AddPermissionGroup(permissions, "租户管理", "TenantManagement", typeof(PermissionConstants.TenantManagement), now);
            AddPermissionGroup(permissions, "系统配置", "SystemConfiguration", typeof(PermissionConstants.SystemConfiguration), now);
            AddPermissionGroup(permissions, "平台权限管控", "PlatformPermissionControl", typeof(PermissionConstants.PlatformPermissionControl), now);
            AddPermissionGroup(permissions, "平台运维监控", "PlatformOperationMonitoring", typeof(PermissionConstants.PlatformOperationMonitoring), now);
            AddPermissionGroup(permissions, "数据安全", "DataSecurity", typeof(PermissionConstants.DataSecurity), now);

            // 租户管理员权限
            AddPermissionGroup(permissions, "用户管理", "UserManagement", typeof(PermissionConstants.UserManagement), now);
            AddPermissionGroup(permissions, "租户配置", "TenantConfiguration", typeof(PermissionConstants.TenantConfiguration), now);
            AddPermissionGroup(permissions, "租户监控", "TenantMonitoring", typeof(PermissionConstants.TenantMonitoring), now);
            AddPermissionGroup(permissions, "租户数据管理", "TenantDataManagement", typeof(PermissionConstants.TenantDataManagement), now);

            // 中医师权限
            AddPermissionGroup(permissions, "患者管理", "PatientManagement", typeof(PermissionConstants.PatientManagement), now);
            AddPermissionGroup(permissions, "诊疗操作", "DiagnosisOperation", typeof(PermissionConstants.DiagnosisOperation), now);
            AddPermissionGroup(permissions, "治疗方案", "TreatmentPlan", typeof(PermissionConstants.TreatmentPlan), now);
            AddPermissionGroup(permissions, "病历管理", "MedicalRecordManagement", typeof(PermissionConstants.MedicalRecordManagement), now);
            AddPermissionGroup(permissions, "知识库", "KnowledgeBase", typeof(PermissionConstants.KnowledgeBase), now);
            AddPermissionGroup(permissions, "医案管理", "MedicalCaseManagement", typeof(PermissionConstants.MedicalCaseManagement), now);

            // 患者权限
            AddPermissionGroup(permissions, "个人信息", "PersonalInformation", typeof(PermissionConstants.PersonalInformation), now);
            AddPermissionGroup(permissions, "诊疗记录查看", "DiagnosisRecordView", typeof(PermissionConstants.DiagnosisRecordView), now);
            AddPermissionGroup(permissions, "治疗方案查看", "TreatmentPlanView", typeof(PermissionConstants.TreatmentPlanView), now);
            AddPermissionGroup(permissions, "病历查看", "MedicalRecordView", typeof(PermissionConstants.MedicalRecordView), now);

            // 特殊场景权限
            AddPermissionGroup(permissions, "会诊场景", "Consultation", typeof(PermissionConstants.Consultation), now);
            AddPermissionGroup(permissions, "跨租户转诊", "CrossTenantReferral", typeof(PermissionConstants.CrossTenantReferral), now);
            AddPermissionGroup(permissions, "系统维护", "SystemMaintenance", typeof(PermissionConstants.SystemMaintenance), now);

            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();

            Console.WriteLine($"权限初始化完成，共创建 {permissions.Count} 个权限");
        }

        /// <summary>
        /// 添加权限组
        /// </summary>
        private static void AddPermissionGroup(List<Permission> permissions, string category, string module, Type constantsType, DateTime now)
        {
            var fields = constantsType.GetFields(BindingFlags.Public | BindingFlags.Static);
            int sortOrder = 0;

            foreach (var field in fields)
            {
                if (field.FieldType == typeof(string))
                {
                    var code = field.GetValue(null)?.ToString();
                    if (!string.IsNullOrEmpty(code))
                    {
                        permissions.Add(new Permission
                        {
                            PermissionCode = code,
                            PermissionName = GetPermissionName(field.Name),
                            Description = $"{category} - {GetPermissionName(field.Name)}",
                            Category = category,
                            Module = module,
                            IsSystem = true,
                            IsActive = true,
                            SortOrder = sortOrder++,
                            CreatedAt = now,
                            UpdatedAt = now
                        });
                    }
                }
            }
        }

        /// <summary>
        /// 将字段名转换为中文权限名称
        /// </summary>
        private static string GetPermissionName(string fieldName)
        {
            var nameMap = new Dictionary<string, string>
            {
                // 通用操作
                { "Create", "创建" },
                { "Edit", "编辑" },
                { "View", "查看" },
                { "Delete", "删除" },
                { "Enable", "启用" },
                { "Disable", "禁用" },
                { "Search", "搜索" },
                { "Export", "导出" },
                { "Print", "打印" },
                { "Sign", "签名" },
                { "Confirm", "确认" },
                { "Review", "审核" },
                
                // 租户管理
                { "ConfigurePackage", "配置服务套餐" },
                { "SetAdmin", "设置管理员" },
                
                // 系统配置
                { "ConfigureGlobal", "配置全局参数" },
                { "ManageKnowledgeBase", "管理知识库" },
                { "ConfigureMonitoring", "配置监控" },
                
                // 平台权限管控
                { "DefineRoleTemplate", "定义角色模板" },
                { "ViewAllPermissions", "查看所有权限" },
                { "HandleCrossTenantIssues", "处理跨租户问题" },
                
                // 平台运维监控
                { "MonitorResources", "监控资源" },
                { "ViewAllLogs", "查看所有日志" },
                { "HandleSupport", "处理技术支持" },
                
                // 数据安全
                { "ConfigureEncryption", "配置加密" },
                { "ConfigureBackup", "配置备份" },
                { "ExecuteBackup", "执行备份" },
                { "ExecuteRestore", "执行恢复" },
                { "ViewIsolationStatus", "查看隔离状态" },
                
                // 用户管理
                { "CreateDoctor", "创建医生" },
                { "EditDoctor", "编辑医生" },
                { "ViewDoctor", "查看医生" },
                { "EnableDoctor", "启用医生" },
                { "DisableDoctor", "禁用医生" },
                { "AssignRole", "分配角色" },
                { "ResetPassword", "重置密码" },
                
                // 租户配置
                { "EditBasicInfo", "编辑基本信息" },
                { "EnableModules", "启用模块" },
                { "ConfigureWorkflow", "配置工作流" },
                { "SetQuotas", "设置配额" },
                
                // 租户监控
                { "ViewUsageStats", "查看使用统计" },
                { "ViewOperationLogs", "查看操作日志" },
                { "HandleIssues", "处理问题" },
                
                // 租户数据管理
                { "ViewStorageUsage", "查看存储使用" },
                { "HandleDataIssues", "处理数据问题" },
                
                // 患者管理
                { "AdvancedSearch", "高级搜索" },
                { "Associate", "关联" },
                { "Disassociate", "解除关联" },
                { "AddCrossTenant", "跨租户添加" },
                { "ViewCrossTenantBasic", "查看跨租户基本信息" },
                
                // 诊疗操作
                { "CreateVisit", "创建就诊" },
                { "EditVisit", "编辑就诊" },
                { "ViewVisit", "查看就诊" },
                { "RecordSymptoms", "记录症状" },
                { "CollectFourDiagnosis", "采集四诊" },
                { "UseAiDiagnosis", "使用AI诊断" },
                { "ConfirmDiagnosis", "确认诊断" },
                { "ModifyDiagnosis", "修改诊断" },
                
                // 治疗方案
                { "UseAiGeneration", "使用AI生成" },
                { "SaveDraft", "保存草稿" },
                { "Submit", "提交" },
                { "ViewHistory", "查看历史" },
                { "HandleRiskWarning", "处理风险警告" },
                { "CreatePrescription", "创建处方" },
                { "EditPrescription", "编辑处方" },
                { "CreateAcupuncture", "创建针灸" },
                { "CreateMoxibustion", "创建艾灸" },
                { "CreateCupping", "创建拔罐" },
                { "CreateDietaryTherapy", "创建食疗" },
                { "CreateLifestyleAdvice", "创建生活建议" },
                
                // 病历管理
                { "UseTemplate", "使用模板" },
                { "ShareForConsultation", "共享会诊" },
                
                // 知识库
                { "ViewTheory", "查看理论" },
                { "ViewFormula", "查看方剂" },
                { "ViewHerb", "查看中药" },
                { "ViewAcupoint", "查看穴位" },
                { "ViewDiagnosisRules", "查看诊断规则" },
                
                // 医案管理
                { "ViewAiGenerated", "查看AI生成" },
                { "ViewSeries", "查看系列" },
                
                // 个人信息
                { "ViewBasic", "查看基本信息" },
                { "ViewMedical", "查看医疗信息" },
                
                // 诊疗记录查看
                { "ViewFourDiagnosis", "查看四诊" },
                { "ViewDiagnosisResult", "查看诊断结果" },
                
                // 治疗方案查看
                { "ViewPrescription", "查看处方" },
                { "ViewNonDrugTherapy", "查看非药物疗法" },
                { "ViewAdvice", "查看建议" },
                
                // 会诊场景
                { "ViewSharedRecord", "查看共享病历" },
                { "ParticipateConsultation", "参与会诊" },
                
                // 跨租户转诊
                { "TransferRecord", "转移病历" },
                { "ReceiveRecord", "接收病历" },
                { "ViewReferralRecord", "查看转诊病历" },
                
                // 系统维护
                { "AccessTenantData", "访问租户数据" },
                { "PerformMaintenance", "执行维护" }
            };

            return nameMap.ContainsKey(fieldName) ? nameMap[fieldName] : fieldName;
        }
    }
}
