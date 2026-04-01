using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TcmAiDiagnosis.EFContext;
using TcmAiDiagnosis.Entities;
namespace TcmAiDiagnosis.Web.Data
{
    // 初始化数据
    public static class DataInitializer
    {
        public static async Task InitializeRolesAndUsersAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<Role>>();
            var userManager = services.GetRequiredService<UserManager<User>>();
            var dbContext = services.GetRequiredService<TcmAiDiagnosisContext>();

            // 1. 创建角色
            var roles = new Dictionary<string, string>
        {
            { "Doctor", "医生" },
            { "Patient", "患者" },
            { "Pharmacist", "药剂师" },
            { "Manager", "管理员" }
        };

            foreach (var (roleName, showName) in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var role = new Role { Name = roleName, ShowName = showName };
                    await roleManager.CreateAsync(role);
                }
            }

            // 2. 创建默认管理员
            var adminUser = await userManager.FindByNameAsync("admin");
            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = "admin",
                    Email = "admin@tcmaidiagnosis.com",
                    Gender = "男",
                    FullName = "系统管理员",
                    PhoneNumber = "13300000000",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Manager");
                }
            }

            // 3. 创建示例医生、药剂师用户
            var doctor = await userManager.FindByNameAsync("doctor3");
            if (doctor == null)
            {
                doctor = new User
                {
                    UserName = "doctor3",
                    FullName = "示例医生",
                    Email = "doctor1@tcmaidiagnosis.com",
                    Gender = "男",
                    TenantId = 2,
                    PhoneNumber = "13300980001",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                var result = await userManager.CreateAsync(doctor, "Doctor@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(doctor, "Doctor");
                }
            }

            var pharmacist = await userManager.FindByNameAsync("pharmacist1");
            if (pharmacist == null)
            {
                pharmacist = new User
                {
                    UserName = "pharmacist1",
                    FullName = "示例药剂师",
                    Email = "pharmacist1@tcmaidiagnosis.com",
                    Gender = "男",
                    TenantId = 2,
                    PhoneNumber = "13300000002",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                var result = await userManager.CreateAsync(pharmacist, "Pharmacist@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(pharmacist, "Pharmacist");
                }
            }

            // 4. 示例病人，将患者添加到示例医生名下
            var patient = await userManager.FindByNameAsync("patient6");
            if (patient == null)
            {
                patient = new User
                {
                    UserName = "patient6",
                    FullName = "示例患者",
                    Email = "patient6@tcmaidiagnosis.com",
                    Gender = "女",
                    TenantId = doctor.TenantId,
                    PhoneNumber = "133000000090",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                var result = await userManager.CreateAsync(patient, "Patient@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(patient, "Patient");
                }
            }
            
            if (doctor != null && patient != null)
            {
                var exists = await dbContext.DoctorPatients.AnyAsync(dp =>
                    dp.DoctorUserId == doctor.Id &&
                    dp.PatientUserId == patient.Id);

                if (!exists)
                {
                    dbContext.DoctorPatients.Add(new DoctorPatient
                    {
                        DoctorUserId = doctor.Id,
                        PatientUserId = patient.Id,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });

                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
