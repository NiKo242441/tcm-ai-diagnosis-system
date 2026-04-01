using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.Entities;
using Microsoft.AspNetCore.Hosting; // 添加这个命名空间

namespace TcmAiDiagnosis.Web.Areas.Identity.Pages.Account
{
    public class TestDataModel : PageModel
    {
        private readonly TenantDomain _tenantDomain;
        private readonly UserDomain _userDomain;
        private readonly RoleManager<Role> _roleManager;
        private readonly IWebHostEnvironment _env; // 添加这个字段

        [TempData]
        public string Message { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public TestDataModel(TenantDomain tenantDomain, UserDomain userDomain, RoleManager<Role> roleManager, IWebHostEnvironment env)
        {
            _tenantDomain = tenantDomain;
            _userDomain = userDomain;
            _roleManager = roleManager;
            _env = env; // 在构造函数中注入
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostCreateRolesAsync()
        {
            try
            {
                // 确保在开发环境
                if (!_env.IsDevelopment())
                {
                    ErrorMessage = "此功能仅在开发环境可用";
                    return RedirectToPage();
                }

                // 定义角色及其显示名称
                var roles = new Dictionary<string, string>
                {
                    { "Doctor", "医生" },
                    { "Patient", "患者" },
                    { "Manager", "管理员" }
                };

                var createdRoles = new List<string>();

                foreach (var (roleName, showName) in roles)
                {
                    var roleExists = await _roleManager.RoleExistsAsync(roleName);
                    if (!roleExists)
                    {
                        // 创建角色并提供 ShowName
                        var role = new Role
                        {
                            Name = roleName,
                            ShowName = showName  // 提供必需的 ShowName 值
                        };

                        var result = await _roleManager.CreateAsync(role);
                        if (result.Succeeded)
                        {
                            createdRoles.Add($"{showName}({roleName})");
                        }
                        else
                        {
                            var errors = string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                            throw new InvalidOperationException($"创建角色 {roleName} 失败: {errors}");
                        }
                    }
                    else
                    {
                        // 获取已存在角色的显示名称
                        var existingRole = await _roleManager.FindByNameAsync(roleName);
                        createdRoles.Add($"{existingRole?.ShowName ?? roleName}({roleName} - 已存在)");
                    }
                }

                Message = $"角色创建成功: {string.Join(", ", createdRoles)}";
            }
            catch (Exception ex)
            {
                // 显示完整的异常信息
                var fullErrorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    fullErrorMessage += $" | 内部异常: {ex.InnerException.Message}";
                }
                ErrorMessage = $"创建角色失败: {fullErrorMessage}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateTenantAsync()
        {
            try
            {
                // 确保在开发环境
                if (!_env.IsDevelopment())
                {
                    ErrorMessage = "此功能仅在开发环境可用";
                    return RedirectToPage();
                }

                // 检查是否已存在租户
                var existingTenant = await _tenantDomain.GetTenantByNameAsync("中原工学院校医院");
                if (existingTenant != null)
                {
                    Message = $"租户已存在，ID: {existingTenant.TenantId}";
                    return RedirectToPage();
                }

                var tenant = await _tenantDomain.CreateTenantAsync(new Tenant()
                {
                    Address = "河南省郑州市",
                    ContactPerson = "乔宽",
                    ContactPhone = "13253536617",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    ExpirationTime = new DateTime(2099, 12, 31),
                    TenantName = "中原工学院校医院",
                    Status = 1
                });

                Message = $"租户创建成功！ID: {tenant?.TenantId}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"创建租户失败: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateDoctorAsync()
        {
            try
            {
                // 确保在开发环境
                if (!_env.IsDevelopment())
                {
                    ErrorMessage = "此功能仅在开发环境可用";
                    return RedirectToPage();
                }

                // 检查Doctor角色是否存在
                var doctorRoleExists = await _roleManager.RoleExistsAsync("Doctor");
                if (!doctorRoleExists)
                {
                    ErrorMessage = "请先创建角色！Doctor角色不存在。";
                    return RedirectToPage();
                }

                // 检查是否已存在医生
                var existingUser = await _userDomain.GetUserByUserNameAsync("13300000001");
                if (existingUser != null)
                {
                    Message = $"医生用户已存在，用户名: {existingUser.UserName}";
                    return RedirectToPage();
                }

                var doctor = await _userDomain.AddDoctorAsync(new User()
                {
                    Email = "13300000001@126.com",
                    PhoneNumber = "13300000001",
                    UserName = "13300000001",
                    FullName = "乔宽",
                    Gender = "男",
                    IsDisabled = 0,
                    DateOfBirth = new DateTime(2000, 1, 1),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    TenantId = 1
                }, "Test@123456");

                Message = $"医生创建成功！用户名: {doctor?.UserName}，密码: Test@123456";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"创建医生失败: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateAllAsync()
        {
            try
            {
                // 确保在开发环境
                if (!_env.IsDevelopment())
                {
                    ErrorMessage = "此功能仅在开发环境可用";
                    return RedirectToPage();
                }

                // 1. 先创建角色
                var roles = new Dictionary<string, string>
                {
                    { "Doctor", "医生" },
                    { "Patient", "患者" },
                    { "Manager", "管理员" }
                };

                foreach (var (roleName, showName) in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(roleName))
                    {
                        await _roleManager.CreateAsync(new Role
                        {
                            Name = roleName,
                            ShowName = showName
                        });
                    }
                }

                // 2. 创建租户
                var tenant = await _tenantDomain.CreateTenantAsync(new Tenant()
                {
                    Address = "河南省郑州市",
                    ContactPerson = "乔宽",
                    ContactPhone = "13253536617",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    ExpirationTime = new DateTime(2099, 12, 31),
                    TenantName = "中原工学院校医院",
                    Status = 1
                });

                // 3. 创建医生
                var doctor = await _userDomain.AddDoctorAsync(new User()
                {
                    Email = "13300000001@126.com",
                    PhoneNumber = "13300000001",
                    UserName = "13300000001",
                    FullName = "乔宽",
                    Gender = "男",
                    IsDisabled = 0,
                    DateOfBirth = new DateTime(2000, 1, 1),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    TenantId = tenant.TenantId
                }, "Test@123456");

                Message = $"全部数据创建成功！<br>" +
                         $"租户ID: {tenant.TenantId}<br>" +
                         $"医生用户名: {doctor.UserName}<br>" +
                         $"密码: Test@123456<br>" +
                         $"角色: 医生(Doctor), 患者(Patient), 管理员(Manager)";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"创建数据失败: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCheckRolesAsync()
        {
            try
            {
                // 确保在开发环境
                if (!_env.IsDevelopment())
                {
                    ErrorMessage = "此功能仅在开发环境可用";
                    return RedirectToPage();
                }

                var roles = new Dictionary<string, string>
                {
                    { "Doctor", "医生" },
                    { "Patient", "患者" },
                    { "Manager", "管理员" }
                };

                var existingRoles = new List<string>();
                var missingRoles = new List<string>();

                foreach (var (roleName, showName) in roles)
                {
                    var roleExists = await _roleManager.RoleExistsAsync(roleName);
                    if (roleExists)
                    {
                        var role = await _roleManager.FindByNameAsync(roleName);
                        existingRoles.Add($"{role?.ShowName ?? showName}({roleName})");
                    }
                    else
                    {
                        missingRoles.Add($"{showName}({roleName})");
                    }
                }

                if (existingRoles.Any())
                {
                    Message = $"现有角色: {string.Join(", ", existingRoles)}";
                }
                if (missingRoles.Any())
                {
                    ErrorMessage = $"缺少角色: {string.Join(", ", missingRoles)}";
                }
                if (!existingRoles.Any() && !missingRoles.Any())
                {
                    Message = "未找到任何系统角色";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"检查角色失败: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}