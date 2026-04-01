using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using TcmAiDiagnosis.Domain.Paged;
using TcmAiDiagnosis.EFContext;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Domain
{
    public partial class PatientDomain
    {

        private readonly TcmAiDiagnosisContext _context;
        private readonly UserManager<User> _userManager;

        public PatientDomain(TcmAiDiagnosisContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// 添加患者（符合Identity规范的角色分配方式，包含数据库验证逻辑）
        /// </summary>
        public async Task<User> AddPatientAsync(User user, UserDetail userDetail, string password, int doctorId, int tenantId)
        {
            if (tenantId <= 0)
            {
                throw new ArgumentException("TenantId 非法，无法创建患者与租户关系");
            }

            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
                throw new ArgumentException("手机号不能为空");
            // 1. 数据库预验证：检查手机号是否已注册（对应Detail.cshtml可编辑的联系方式字段）
            var existingUser = await _userManager.FindByNameAsync(user.PhoneNumber);
            if (existingUser != null)
                throw new InvalidOperationException("该手机号已注册为患者账户");

            // 2. 基础字段验证（对应Detail.cshtml可编辑的姓名、性别等字段）
            if (string.IsNullOrWhiteSpace(user.FullName))
                throw new ArgumentException("患者姓名不能为空");
            if (user.Gender != "男" && user.Gender != "女")
                throw new ArgumentException("性别只能为'男'或'女'");

            // 3. 患者TenantId必须为null（原有逻辑保留）
            user.TenantId = null;
            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;
            user.UserName = user.PhoneNumber;

            // 4. 使用Identity的UserManager创建用户（原有逻辑保留）
            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
                throw new InvalidOperationException("用户创建失败: " + string.Join(", ", createResult.Errors.Select(e => e.Description)));

            // 5. 查询名称为"Patient"的角色（原有逻辑保留）
            var patientRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Patient");
            if (patientRole == null)
                throw new ArgumentException("系统中未找到名称为'Patient'的角色");

            // 6. 使用Identity的UserManager添加角色（原有逻辑保留）
            var roleResult = await _userManager.AddToRoleAsync(user, patientRole.Name);
            if (!roleResult.Succeeded)
                throw new InvalidOperationException("角色分配失败: " + string.Join(", ", roleResult.Errors.Select(e => e.Description)));

            // 7. 添加用户详细信息（补充详细信息验证）
            if (!string.IsNullOrWhiteSpace(userDetail.EmergencyContactPhone) && !IsValidPhoneNumber(userDetail.EmergencyContactPhone))
                throw new ArgumentException("紧急联系人电话格式不正确");

            // 新增出生日期验证
            if (user.DateOfBirth == default)
                throw new ArgumentException("出生日期不能为空");

            var existingDetail = await _context.UserDetails
                                .FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (existingDetail == null)
            {
                userDetail.UserId = user.Id;
                userDetail.CreatedAt = DateTime.Now;
                userDetail.UpdatedAt = DateTime.Now;
                _context.UserDetails.Add(userDetail);
            }
            else
            {
                // 如果想更新已有记录
                existingDetail.UpdatedAt = DateTime.Now;
            }

            await _context.DoctorPatients.AddAsync(new DoctorPatient { DoctorUserId = doctorId, PatientUserId = user.Id });
            await _context.PatientTenants.AddAsync(new PatientTenant { TenantId = tenantId, PatientUserId = user.Id });
            await _context.SaveChangesAsync();

            return user;
        }

        /// <summary>
        /// 验证手机号格式（辅助方法）
        /// </summary>
        private bool IsValidPhoneNumber(string phoneNumber)
        {
            // 简单验证：11位数字且以1开头（可根据实际需求调整正则）
            return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, "^1[3-9]\\d{9}$");
        }

        /// <summary>
        /// 更新患者信息（异步版本）
        /// </summary>
        public async Task UpdatePatientAsync(User user, UserDetail userDetail, int doctorId, int tenantId)
        {
            try
            {
                // 1. 数据库预验证：检查手机号是否被其他患者占用（排除当前患者）
                var existingUser = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == user.PhoneNumber && u.Id != user.Id);
                if (existingUser != null)
                    throw new InvalidOperationException("该手机号已被其他患者注册");

                // 2. 基础字段验证（对应前端可编辑的姓名、性别字段）
                if (string.IsNullOrWhiteSpace(user.FullName))
                    throw new ArgumentException("患者姓名不能为空");
                if (user.Gender != "男" && user.Gender != "女")
                    throw new ArgumentException("性别只能为'男'或'女'");

                // 3. 详细信息验证（对应前端可编辑的紧急联系人信息）
                if (!string.IsNullOrWhiteSpace(userDetail.EmergencyContactPhone) && !IsValidPhoneNumber(userDetail.EmergencyContactPhone))
                    throw new ArgumentException("紧急联系人电话格式不正确");

                // 4. 获取数据库中的现有实体（避免全量更新）
                var dbUser = await _context.Users.FindAsync(user.Id);

                if (dbUser == null)
                    throw new KeyNotFoundException("未找到对应的患者信息");
                var dbUserDetail = await _context.UserDetails.FirstOrDefaultAsync(ud => ud.UserId == user.Id);

                if (dbUserDetail == null)
                    dbUserDetail = new() { UserId = dbUser.Id, CreatedAt = DateTime.Now };

                // 5. 仅更新前端可编辑的字段（防止意外覆盖系统字段）
                dbUser.FullName = user.FullName;
                dbUser.Gender = user.Gender;
                dbUser.PhoneNumber = user.PhoneNumber;
                dbUser.DateOfBirth = user.DateOfBirth;
                dbUser.UpdatedAt = DateTime.Now; // 更新时间戳

                dbUserDetail.EmergencyContactName = userDetail.EmergencyContactName;
                dbUserDetail.EmergencyContactPhone = userDetail.EmergencyContactPhone;
                dbUserDetail.HomeAddress = userDetail.HomeAddress;
                dbUserDetail.AllergyHistory = userDetail.AllergyHistory;
                dbUserDetail.FamilyMedicalHistory = userDetail.FamilyMedicalHistory;
                dbUserDetail.InfectiousDiseaseHistory = userDetail.InfectiousDiseaseHistory;
                dbUserDetail.BloodType = userDetail.BloodType;
                dbUserDetail.PastMedicalHistory = userDetail.PastMedicalHistory;
                dbUserDetail.BloodType = userDetail.BloodType;
                dbUserDetail.UpdatedAt = DateTime.Now; // 更新时间戳

                if (!_context.DoctorPatients.Any(x => x.DoctorUserId == doctorId && x.PatientUserId == user.Id))
                {
                    await _context.DoctorPatients.AddAsync(new DoctorPatient { DoctorUserId = doctorId, PatientUserId = user.Id });
                }
                if (!_context.PatientTenants.Any(x => x.TenantId == tenantId && x.PatientUserId == user.Id))
                {
                    await _context.PatientTenants.AddAsync(new PatientTenant { TenantId = tenantId, PatientUserId = user.Id });
                }
                var changeTotal = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 根据类型获取医疗历史（如慢性疾病）
        /// </summary>
        public async Task<List<MedicalHistory>> GetMedicalHistoriesByTypeAsync(string type)
        {
            return await _context.MedicalHistories
                .Where(mh => mh.Type == type)
                .ToListAsync();
        }

        public async Task<PagedResult<User>> GetPatientsByDoctorAsync(PagedRequest request, int doctorId)
        {
            var query = _context.Users
                .Include(u => u.Detail)
                .Where(u => u.DoctorPatients.Any(dp => dp.DoctorUserId == doctorId));

            if (!string.IsNullOrWhiteSpace(request.SearchKeyword))
            {
                query = query.Where(u =>
                    u.FullName.Contains(request.SearchKeyword) ||
                    u.PhoneNumber.Contains(request.SearchKeyword));
            }

            return await query
                .OrderBy(u => u.FullName)
                .ToPagedResultAsync(request.Page, request.PageSize);
        }

        public async Task<PagedResult<User>> GetPatientsByTenantAsync(PagedRequest request, int tenantId)
        {
            var query = _context.Users
                .Include(u => u.Detail)
                .Where(u => u.PatientTenants.Any(pt => pt.TenantId == tenantId));

            if (!string.IsNullOrWhiteSpace(request.SearchKeyword))
            {
                query = query.Where(u =>
                    u.FullName.Contains(request.SearchKeyword) ||
                    u.PhoneNumber.Contains(request.SearchKeyword));
            }

            return await query
                .OrderBy(u => u.FullName)
                .ToPagedResultAsync(request.Page, request.PageSize);
        }

        public async Task<User> GetPatientByIdAsync(int patientId)
        {
            var user = await _userManager.FindByIdAsync(patientId.ToString());
            if (user == null || user.TenantId != null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Patient"))
                return null;

            await _context.Entry(user)
                .Reference(u => u.Detail)
                .LoadAsync();

            return user;
        }

        /// <summary>
        /// 根据手机号查询患者信息（包含详细信息）
        /// </summary>
        public async Task<User> GetPatientByPhoneNumberAsync(string phoneNumber)
        {
            var user = await _userManager.FindByNameAsync(phoneNumber);
            if (user == null || user.TenantId != null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Patient"))
                return null;

            // 加载用户详细信息（假设通过导航属性关联）
            await _context.Entry(user)
                .Reference(u => u.Detail)
                .LoadAsync();

            return user;
        }

        public async Task<string> GetPatientNotes(int patientId)
        {
            var patient = await GetPatientByIdAsync(patientId);


            if (patient == null)
                throw new ArgumentNullException(nameof(patient), "患者不存在");
            if (patient.Detail == null)
                throw new ArgumentNullException(nameof(patient.Detail), "患者详情不存在");

            return GetPatientNotes(patient);
        }

        public string GetPatientNotes(User patient)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("患者姓名：" + (patient.FullName ?? "未设置"));
            stringBuilder.AppendLine("患者性别：" + (patient.Gender ?? "未知"));
            stringBuilder.AppendLine("出生日期：" + (patient.DateOfBirth.HasValue ? patient.DateOfBirth.Value.ToString("yyyy年MM月dd日") : "未设置"));
            if (patient.DateOfBirth.HasValue)
            {
                int age = DateTime.Now.Year - patient.DateOfBirth.Value.Year;
                if (DateTime.Now < patient.DateOfBirth.Value.AddYears(age))
                {
                    age--;
                }
                stringBuilder.AppendLine("患者年龄：" + age);
            }
            else
            {
                stringBuilder.AppendLine("患者年龄：未设置");
            }

            // 添加对patient.Detail的空值检查
            if (patient.Detail != null)
            {
                stringBuilder.AppendLine("慢性病史：" + (patient.Detail.PastMedicalHistory ?? "无"));
                stringBuilder.AppendLine("传染病史：" + (patient.Detail.InfectiousDiseaseHistory ?? "无"));
                stringBuilder.AppendLine("过敏史：" + (patient.Detail.AllergyHistory ?? "无"));
                stringBuilder.AppendLine("家族病史：" + (patient.Detail.FamilyMedicalHistory ?? "无"));
            }
            else
            {
                stringBuilder.AppendLine("慢性病史：无");
                stringBuilder.AppendLine("传染病史：无");
                stringBuilder.AppendLine("过敏史：无");
                stringBuilder.AppendLine("家族病史：无");
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// 跨租户搜索患者（通过手机号）
        /// </summary>
        /// <param name="phoneNumber">患者手机号</param>
        /// <param name="currentTenantId">当前租户ID</param>
        /// <returns>患者搜索结果列表</returns>
        public async Task<List<User>> SearchPatientsByPhoneAsync(string phoneNumber, int currentTenantId)
        {
            // 1. 验证手机号格式
            if (!IsValidPhoneNumber(phoneNumber))
            {
                throw new ArgumentException("手机号格式不正确");
            }

            // 2. 根据手机号搜索所有患者
            var patients = await _context.Users
                .Include(u => u.Detail)
                .Include(u => u.PatientTenants)
                .ThenInclude(pt => pt.Tenant)
                .Where(u => u.PhoneNumber == phoneNumber && u.TenantId == null) // 患者的TenantId为null
                .ToListAsync();

            // 3. 验证用户角色
            var validPatients = new List<User>();
            foreach (var patient in patients)
            {
                var roles = await _userManager.GetRolesAsync(patient);
                if (roles.Contains("Patient"))
                {
                    validPatients.Add(patient);
                }
            }

            return validPatients;
        }

        /// <summary>
        /// 添加现有患者到当前租户
        /// </summary>
        /// <param name="patientId">患者ID</param>
        /// <param name="doctorId">当前医生ID</param>
        /// <param name="tenantId">当前租户ID</param>
        /// <returns>是否添加成功</returns>
        public async Task<bool> AddExistingPatientToTenantAsync(int patientId, int doctorId, int tenantId)
        {
            // 1. 验证患者是否存在
            var patient = await GetPatientByIdAsync(patientId);
            if (patient == null)
                throw new ArgumentException("患者不存在");

            // 2. 检查是否已关联到目标租户
            var existingRelation = await _context.PatientTenants
                .FirstOrDefaultAsync(pt => pt.PatientUserId == patientId && pt.TenantId == tenantId);

            if (existingRelation != null)
            {
                if (existingRelation.IsActive)
                {
                    throw new InvalidOperationException("患者已关联到该租户");
                }
                else
                {
                    // 重新激活已停用的关联关系
                    existingRelation.IsActive = true;
                    existingRelation.ReactivatedBy = doctorId;
                    existingRelation.ReactivatedAt = DateTime.Now;
                    existingRelation.UpdatedAt = DateTime.Now;
                }
            }
            else
            {
                // 3. 创建新的患者-租户关联关系
                var patientTenant = new PatientTenant
                {
                    PatientUserId = patientId,
                    TenantId = tenantId,
                    AddedBy = doctorId,
                    AddedAt = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsActive = true,
                    RelationType = PatientTenantRelationType.CrossTenant
                };

                await _context.PatientTenants.AddAsync(patientTenant);
            }

            // 4. 建立医生-患者关联（如果不存在）
            var existingDoctorPatient = await _context.DoctorPatients
                .FirstOrDefaultAsync(dp => dp.DoctorUserId == doctorId && dp.PatientUserId == patientId);

            if (existingDoctorPatient == null)
            {
                await _context.DoctorPatients.AddAsync(new DoctorPatient
                {
                    DoctorUserId = doctorId,
                    PatientUserId = patientId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 检查患者是否已关联到指定租户
        /// </summary>
        /// <param name="patientId">患者ID</param>
        /// <param name="tenantId">租户ID</param>
        /// <returns>是否已关联</returns>
        public async Task<bool> IsPatientLinkedToTenantAsync(int patientId, int tenantId)
        {
            return await _context.PatientTenants
                .AnyAsync(pt => pt.PatientUserId == patientId && pt.TenantId == tenantId && pt.IsActive);
        }

        /// <summary>
        /// 获取患者关联的租户数量
        /// </summary>
        /// <param name="patientId">患者ID</param>
        /// <returns>关联的租户数量</returns>
        public async Task<int> GetPatientLinkedTenantCountAsync(int patientId)
        {
            return await _context.PatientTenants
                .CountAsync(pt => pt.PatientUserId == patientId && pt.IsActive);
        }

        /// <summary>
        /// 获取患者的原始租户
        /// </summary>
        /// <param name="patientId">患者ID</param>
        /// <returns>原始租户</returns>
        public async Task<Tenant> GetPatientOriginalTenantAsync(int patientId)
        {
            var originalRelation = await _context.PatientTenants
                .Include(pt => pt.Tenant)
                .FirstOrDefaultAsync(pt => pt.PatientUserId == patientId &&
                                          pt.RelationType == PatientTenantRelationType.Original &&
                                          pt.IsActive);

            return originalRelation?.Tenant;
        }

        /// <summary>
        /// 身份证脱敏处理
        /// </summary>
        /// <param name="idCard">身份证号</param>
        /// <returns>脱敏后的身份证号</returns>
        private string MaskIdCard(string idCard)
        {
            if (string.IsNullOrEmpty(idCard) || idCard.Length < 8)
                return idCard;

            return idCard.Substring(0, 4) + "****" + idCard.Substring(idCard.Length - 4);
        }
    }
}