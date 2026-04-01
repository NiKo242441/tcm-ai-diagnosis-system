using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using TcmAiDiagnosis.Domain;
using TcmAiDiagnosis.EFContext;
using TcmAiDiagnosis.Entities;
using TcmAiDiagnosis.Web.Data;
using TcmAiDiagnosis.Web.Services;
using TcmAiDiagnosis.Web.Extensions;

namespace TcmAiDiagnosis.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("Connection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            ServerVersion serverVersion = ServerVersion.AutoDetect(connectionString);
            builder.Services.AddDbContext<TcmAiDiagnosisContext>(options => options.UseMySql(connectionString, serverVersion));

            builder.Services.AddIdentity<User, Role>(options =>
            {
                // 密码策略配置（开发环境可以放宽要求）
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;

                // 其他配置...
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            })
            .AddEntityFrameworkStores<TcmAiDiagnosisContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.LogoutPath = "/Identity/Account/Logout";
                options.LoginPath = "/Identity/Account/Login";
            });

            // 添加服务
            builder.Services.AddRazorPages()
                .AddRazorPagesOptions(options =>
                {
                    // 明确配置Area页面路由
                    options.Conventions.AddAreaPageRoute("Identity", "/Account/TestData", "TestData");
                });

            builder.Services.AddControllers();

            // 配置 Dify API 选项
            builder.Services.Configure<DifyApiOptions>(
                builder.Configuration.GetSection(DifyApiOptions.SectionName));

            builder.Services.AddOptions();
            builder.Services.AddHttpClient();

            // 配置API HTTP客户端 - 修复这里，使用正确的地址
            builder.Services.AddHttpClient("API", client =>
            {
                client.BaseAddress = new Uri("https://test2.icode8.net/"); // 改为您的服务器地址
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            // 注册Domain服务
            builder.Services.AddScoped<UserDomain>();
            builder.Services.AddScoped<TenantDomain>();
            builder.Services.AddScoped<VisitDomain>();
            builder.Services.AddScoped<PatientDomain>();
            builder.Services.AddScoped<HerbDomain>();
            builder.Services.AddScoped<HerbContraindicationDomain>();
            builder.Services.AddScoped<SyndromeDomain>();
            builder.Services.AddScoped<InventoryAlertRuleDomain>();
            builder.Services.AddScoped<InventoryAlertService>();
            builder.Services.AddHostedService<BackgroundAlertCheckService>();
            builder.Services.AddHostedService<VisitSeriesAutoEndService>();
            builder.Services.AddScoped<InventoryManagementDomain>();
            builder.Services.AddDbContext<TcmAiDiagnosisContext>();
            // 注册数据库上下文
            builder.Services.AddDbContext<TcmAiDiagnosisContext>();

            // 注册领域服务
            builder.Services.AddScoped<InventoryAlertRuleDomain>();

            // 注册权限系统服务
            builder.Services.AddPermissionServices(builder.Configuration);

            var app = builder.Build();

            // 添加开发环境异常处理
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(); // 显示详细错误页面
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }




            // 自动执行数据库迁移 + 数据初始化
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    // 自动数据库迁移
                    var context = services.GetRequiredService<TcmAiDiagnosisContext>();
                    context.Database.Migrate();

                    // 数据初始化：角色 + 默认用户
                    DataInitializer.InitializeRolesAndUsersAsync(services).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "数据库迁移或数据初始化过程中发生错误");
                }
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors("AllowLocal");
            app.UseAuthentication();
            app.UseAuthorization();

            // 使用权限验证中间件（必须在UseAuthorization之后）
            app.UsePermissionAuthorization();

            app.MapRazorPages();
            app.MapControllers();

            app.Run();
        }

       
        private static async Task InitializeBaseDataAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<Role>>();
            var userManager = services.GetRequiredService<UserManager<User>>();

            // 创建系统角色
            var roles = new[] { "Doctor", "Patient", "Manager" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new Role
                    {
                        Name = roleName,
                        ShowName = roleName switch
                        {
                            "Doctor" => "医生",
                            "Patient" => "患者",
                            "Manager" => "管理员",
                            _ => roleName
                        }
                    });
                }
            }

            // 创建默认管理员用户（如果不存在）
            var adminUser = await userManager.FindByNameAsync("admin");
            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = "admin",
                    Email = "admin@tcmaidiagnosis.com",
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
        }
    }
}