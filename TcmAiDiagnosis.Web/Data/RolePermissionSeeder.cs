using TcmAiDiagnosis.EFContext;
using TcmAiDiagnosis.Entities;
using TcmAiDiagnosis.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace TcmAiDiagnosis.Web.Data
{
    /// <summary>
    /// 角色权限数据种子 - 初始化角色与权限的关联关系
    /// </summary>
    public static class RolePermissionSeeder
    {
        /// <summary>
        /// 初始化角色权限关联
        /// </summary>
        public static async Task SeedRolePermissionsAsync(
            TcmAiDiagnosisContext context, 
            RoleManager<Role> roleManager, 
            UserManager<User> userManager)
        {
            // 检查是否已有角色权限数据
            if (await context.RolePermissions.AnyAsync())
            {
                return; // 已有数据，跳过初始化
            }

            var now = DateTime.UtcNow;
            var rolePermissions = new List<RolePermission>();

            // 获取所有角色
            var managerRole = await roleManager.FindByNameAsync("Manager");
            var doctorRole = await roleManager.FindByNameAsync("Doctor");
            var patientRole = await roleManager.FindByNameAsync("Patient");
            var pharmacistRole = await roleManager.FindByNameAsync("Pharmacist");

            // 获取所有权限
            var allPermissions = await context.Permissions.ToListAsync();
            var permissionDict = allPermissions.ToDictionary(p => p.PermissionCode, p => p.PermissionId);

            // 1. 管理员角色权限（租户管理员）
            if (managerRole != null)
            {
                var managerPermissions = GetManagerPermissions();
                foreach (var permCode in managerPermissions)
                {
                    if (permissionDict.TryGetValue(permCode, out var permId))
                    {
                        rolePermissions.Add(new RolePermission
                        {
                            RoleId = managerRole.Id,
                            PermissionId = permId,
                            GrantedAt = now
                        });
                    }
                }
            }

            // 2. 医生角色权限
            if (doctorRole != null)
            {
                var doctorPermissions = GetDoctorPermissions();
                foreach (var permCode in doctorPermissions)
                {
                    if (permissionDict.TryGetValue(permCode, out var permId))
                    {
                        rolePermissions.Add(new RolePermission
                        {
                            RoleId = doctorRole.Id,
                            PermissionId = permId,
                            GrantedAt = now
                        });
                    }
                }
            }

            // 3. 患者角色权限
            if (patientRole != null)
            {
                var patientPermissions = GetPatientPermissions();
                foreach (var permCode in patientPermissions)
                {
                    if (permissionDict.TryGetValue(permCode, out var permId))
                    {
                        rolePermissions.Add(new RolePermission
                        {
                            RoleId = patientRole.Id,
                            PermissionId = permId,
                            GrantedAt = now
                        });
                    }
                }
            }

            // 4. 药剂师角色权限（基础权限）
            if (pharmacistRole != null)
            {
                var pharmacistPermissions = GetPharmacistPermissions();
                foreach (var permCode in pharmacistPermissions)
                {
                    if (permissionDict.TryGetValue(permCode, out var permId))
                    {
                        rolePermissions.Add(new RolePermission
                        {
                            RoleId = pharmacistRole.Id,
                            PermissionId = permId,
                            GrantedAt = now
                        });
                    }
                }
            }

            await context.RolePermissions.AddRangeAsync(rolePermissions);
            await context.SaveChangesAsync();

            Console.WriteLine($"角色权限初始化完成，共创建 {rolePermissions.Count} 个角色权限关联");
        }

        /// <summary>
        /// 获取管理员（租户管理员）权限列表
        /// </summary>
        private static List<string> GetManagerPermissions()
        {
            return new List<string>
            {
                // 用户管理
                PermissionConstants.UserManagement.CreateDoctor,
                PermissionConstants.UserManagement.EditDoctor,
                PermissionConstants.UserManagement.ViewDoctor,
                PermissionConstants.UserManagement.EnableDoctor,
                PermissionConstants.UserManagement.DisableDoctor,
                PermissionConstants.UserManagement.AssignRole,
                PermissionConstants.UserManagement.ResetPassword,

                // 租户配置
                PermissionConstants.TenantConfiguration.EditBasicInfo,
                PermissionConstants.TenantConfiguration.EnableModules,
                PermissionConstants.TenantConfiguration.ConfigureWorkflow,
                PermissionConstants.TenantConfiguration.SetQuotas,

                // 租户监控
                PermissionConstants.TenantMonitoring.ViewUsageStats,
                PermissionConstants.TenantMonitoring.ViewOperationLogs,
                PermissionConstants.TenantMonitoring.HandleIssues,

                // 租户数据管理
                PermissionConstants.TenantDataManagement.ViewStorageUsage,
                PermissionConstants.TenantDataManagement.ConfigureBackup,
                PermissionConstants.TenantDataManagement.HandleDataIssues,

                // 患者管理（查看权限）
                PermissionConstants.PatientManagement.View,
                PermissionConstants.PatientManagement.Search,

                // 知识库（查看权限）
                PermissionConstants.KnowledgeBase.ViewTheory,
                PermissionConstants.KnowledgeBase.ViewFormula,
                PermissionConstants.KnowledgeBase.ViewHerb,
                PermissionConstants.KnowledgeBase.ViewAcupoint,
                PermissionConstants.KnowledgeBase.ViewDiagnosisRules
            };
        }

        /// <summary>
        /// 获取医生权限列表
        /// </summary>
        private static List<string> GetDoctorPermissions()
        {
            return new List<string>
            {
                // 患者管理
                PermissionConstants.PatientManagement.Create,
                PermissionConstants.PatientManagement.Edit,
                PermissionConstants.PatientManagement.View,
                PermissionConstants.PatientManagement.Search,
                PermissionConstants.PatientManagement.AdvancedSearch,
                PermissionConstants.PatientManagement.Associate,
                PermissionConstants.PatientManagement.Disassociate,

                // 诊疗操作
                PermissionConstants.DiagnosisOperation.CreateVisit,
                PermissionConstants.DiagnosisOperation.EditVisit,
                PermissionConstants.DiagnosisOperation.ViewVisit,
                PermissionConstants.DiagnosisOperation.RecordSymptoms,
                PermissionConstants.DiagnosisOperation.CollectFourDiagnosis,
                PermissionConstants.DiagnosisOperation.UseAiDiagnosis,
                PermissionConstants.DiagnosisOperation.ConfirmDiagnosis,
                PermissionConstants.DiagnosisOperation.ModifyDiagnosis,

                // 治疗方案
                PermissionConstants.TreatmentPlan.Create,
                PermissionConstants.TreatmentPlan.Edit,
                PermissionConstants.TreatmentPlan.View,
                PermissionConstants.TreatmentPlan.UseAiGeneration,
                PermissionConstants.TreatmentPlan.SaveDraft,
                PermissionConstants.TreatmentPlan.Submit,
                PermissionConstants.TreatmentPlan.ViewHistory,
                PermissionConstants.TreatmentPlan.HandleRiskWarning,
                PermissionConstants.TreatmentPlan.CreatePrescription,
                PermissionConstants.TreatmentPlan.EditPrescription,
                PermissionConstants.TreatmentPlan.CreateAcupuncture,
                PermissionConstants.TreatmentPlan.CreateMoxibustion,
                PermissionConstants.TreatmentPlan.CreateCupping,
                PermissionConstants.TreatmentPlan.CreateDietaryTherapy,
                PermissionConstants.TreatmentPlan.CreateLifestyleAdvice,

                // 病历管理
                PermissionConstants.MedicalRecordManagement.Create,
                PermissionConstants.MedicalRecordManagement.Edit,
                PermissionConstants.MedicalRecordManagement.View,
                PermissionConstants.MedicalRecordManagement.UseTemplate,
                PermissionConstants.MedicalRecordManagement.Sign,
                PermissionConstants.MedicalRecordManagement.Print,
                PermissionConstants.MedicalRecordManagement.Export,
                PermissionConstants.MedicalRecordManagement.ShareForConsultation,

                // 知识库
                PermissionConstants.KnowledgeBase.ViewTheory,
                PermissionConstants.KnowledgeBase.ViewFormula,
                PermissionConstants.KnowledgeBase.ViewHerb,
                PermissionConstants.KnowledgeBase.ViewAcupoint,
                PermissionConstants.KnowledgeBase.ViewDiagnosisRules,

                // 医案管理
                PermissionConstants.MedicalCaseManagement.ViewAiGenerated,
                PermissionConstants.MedicalCaseManagement.Review,
                PermissionConstants.MedicalCaseManagement.Edit,
                PermissionConstants.MedicalCaseManagement.Confirm,
                PermissionConstants.MedicalCaseManagement.Export,
                PermissionConstants.MedicalCaseManagement.ViewSeries,

                // 会诊场景
                PermissionConstants.Consultation.ViewSharedRecord,
                PermissionConstants.Consultation.ParticipateConsultation
            };
        }

        /// <summary>
        /// 获取患者权限列表
        /// </summary>
        private static List<string> GetPatientPermissions()
        {
            return new List<string>
            {
                // 个人信息
                PermissionConstants.PersonalInformation.ViewBasic,
                PermissionConstants.PersonalInformation.ViewMedical,

                // 诊疗记录查看
                PermissionConstants.DiagnosisRecordView.ViewHistory,
                PermissionConstants.DiagnosisRecordView.ViewFourDiagnosis,
                PermissionConstants.DiagnosisRecordView.ViewDiagnosisResult,

                // 治疗方案查看
                PermissionConstants.TreatmentPlanView.ViewPrescription,
                PermissionConstants.TreatmentPlanView.ViewNonDrugTherapy,
                PermissionConstants.TreatmentPlanView.ViewAdvice,

                // 病历查看
                PermissionConstants.MedicalRecordView.View,
                PermissionConstants.MedicalRecordView.Print,
                PermissionConstants.MedicalRecordView.Export
            };
        }

        /// <summary>
        /// 获取药剂师权限列表
        /// </summary>
        private static List<string> GetPharmacistPermissions()
        {
            return new List<string>
            {
                // 患者基本信息查看
                PermissionConstants.PatientManagement.View,
                PermissionConstants.PatientManagement.Search,

                // 处方查看
                PermissionConstants.TreatmentPlan.View,
                PermissionConstants.TreatmentPlan.ViewHistory,

                // 知识库（中药相关）
                PermissionConstants.KnowledgeBase.ViewHerb,
                PermissionConstants.KnowledgeBase.ViewFormula
            };
        }
    }
}
