using TcmAiDiagnosis.Domain.Services;
using TcmAiDiagnosis.Web.Configuration;
using TcmAiDiagnosis.Web.Middleware;

namespace TcmAiDiagnosis.Web.Extensions
{
    /// <summary>
    /// 权限服务注册扩展方法
    /// </summary>
    public static class PermissionServiceExtensions
    {
        /// <summary>
        /// 注册权限系统服务
        /// </summary>
        public static IServiceCollection AddPermissionServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 注册配置
            services.Configure<PermissionSettings>(
                configuration.GetSection(PermissionSettings.SectionName));

            // 注册权限服务
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IPermissionCacheService, PermissionCacheService>();
            services.AddScoped<IPermissionAuditLogger, PermissionAuditLogger>();

            // 注册内存缓存（如果尚未注册）
            services.AddMemoryCache();

            return services;
        }

        /// <summary>
        /// 使用权限验证中间件
        /// </summary>
        public static IApplicationBuilder UsePermissionAuthorization(
            this IApplicationBuilder app)
        {
            // 从配置读取是否启用权限检查
            var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
            var enablePermissionCheck = configuration
                .GetValue<bool>("PermissionSettings:EnablePermissionCheck", true);

            if (enablePermissionCheck)
            {
                app.UseMiddleware<PermissionAuthorizationMiddleware>();
            }

            return app;
        }
    }
}
