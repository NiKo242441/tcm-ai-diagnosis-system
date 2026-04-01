# 权限验证中间件 - 设计文档

## 概述

本文档描述了权限验证中间件系统的详细设计，包括架构、组件、接口和实现细节。该系统将为TCM AI诊断系统提供统一的、细粒度的权限控制能力。

## 架构设计

### 系统架构图

```
┌─────────────────────────────────────────────────────────────┐
│                      HTTP Request                            │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│              Authentication Middleware                       │
│              (ASP.NET Core Identity)                        │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│         Permission Authorization Middleware                  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  1. 提取端点权限要求 (RequirePermissionAttribute)    │  │
│  │  2. 获取当前用户信息                                  │  │
│  │  3. 调用 PermissionService 验证权限                  │  │
│  │  4. 记录审计日志                                      │  │
│  │  5. 返回结果 (允许/拒绝)                             │  │
│  └──────────────────────────────────────────────────────┘  │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│                  Permission Service                          │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  - CheckPermissionAsync()                            │  │
│  │  - GetUserPermissionsAsync()                         │  │
│  │  - GetRolePermissionsAsync()                         │  │
│  │  - CheckTemporaryPermissionAsync()                   │  │
│  └──────────────────────────────────────────────────────┘  │
└──────────────────────┬──────────────────────────────────────┘
                       │
        ┌──────────────┼──────────────┐
        │              │              │
        ▼              ▼              ▼
┌──────────────┐ ┌──────────┐ ┌────────────┐
│ Permission   │ │  Cache   │ │  Database  │
│ Cache        │ │  Service │ │  Context   │
└──────────────┘ └──────────┘ └────────────┘
```

### 组件职责

1. **RequirePermissionAttribute**：声明式权限要求标注
2. **PermissionAuthorizationMiddleware**：权限验证中间件
3. **IPermissionService**：权限服务接口
4. **PermissionService**：权限服务实现
5. **IPermissionCacheService**：权限缓存服务接口
6. **PermissionCacheService**：权限缓存服务实现
7. **PermissionAuditLogger**：权限审计日志记录器

## 组件设计

### 1. RequirePermissionAttribute

**文件位置**：`TcmAiDiagnosis.Web/Attributes/RequirePermissionAttribute.cs`

**功能**：用于标注控制器或方法的权限要求

**设计**：
```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequirePermissionAttribute : Attribute
{
    /// <summary>
    /// 所需权限代码列表
    /// </summary>
    public string[] PermissionCodes { get; }
    
    /// <summary>
    /// 权限验证模式：RequireAll（需要所有权限）或 RequireAny（需要任一权限）
    /// </summary>
    public PermissionCheckMode Mode { get; set; } = PermissionCheckMode.RequireAll;
    
    /// <summary>
    /// 是否允许临时权限
    /// </summary>
    public bool AllowTemporary { get; set; } = true;
    
    public RequirePermissionAttribute(params string[] permissionCodes)
    {
        PermissionCodes = permissionCodes ?? throw new ArgumentNullException(nameof(permissionCodes));
    }
}

public enum PermissionCheckMode
{
    RequireAll,  // 需要所有权限
    RequireAny   // 需要任一权限
}
```

**使用示例**：
```csharp
// 单个权限
[RequirePermission(PermissionConstants.PatientManagement.Create)]
public async Task<IActionResult> CreatePatient(PatientDto dto) { }

// 多个权限（需要全部）
[RequirePermission(
    PermissionConstants.PatientManagement.Edit,
    PermissionConstants.PatientManagement.View
)]
public async Task<IActionResult> EditPatient(int id, PatientDto dto) { }

// 多个权限（需要任一）
[RequirePermission(
    PermissionConstants.PatientManagement.View,
    PermissionConstants.PatientManagement.Search,
    Mode = PermissionCheckMode.RequireAny
)]
public async Task<IActionResult> GetPatient(int id) { }
```

### 2. PermissionAuthorizationMiddleware

**文件位置**：`TcmAiDiagnosis.Web/Middleware/PermissionAuthorizationMiddleware.cs`

**功能**：拦截HTTP请求并验证权限

**设计**：
```csharp
public class PermissionAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PermissionAuthorizationMiddleware> _logger;
    
    public PermissionAuthorizationMiddleware(
        RequestDelegate next,
        ILogger<PermissionAuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(
        HttpContext context,
        IPermissionService permissionService,
        IPermissionAuditLogger auditLogger)
    {
        // 1. 检查是否在白名单中
        if (IsWhitelisted(context.Request.Path))
        {
            await _next(context);
            return;
        }
        
        // 2. 获取端点的权限要求
        var endpoint = context.GetEndpoint();
        var permissionAttribute = endpoint?.Metadata
            .GetMetadata<RequirePermissionAttribute>();
        
        if (permissionAttribute == null)
        {
            // 没有权限要求，继续处理
            await _next(context);
            return;
        }
        
        // 3. 检查用户是否已认证
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new 
            { 
                error = "Unauthorized",
                message = "用户未认证" 
            });
            return;
        }
        
        // 4. 获取用户ID和租户ID
        var userId = GetUserId(context.User);
        var tenantId = GetTenantId(context.User);
        
        // 5. 验证权限
        var hasPermission = await permissionService.CheckPermissionsAsync(
            userId,
            tenantId,
            permissionAttribute.PermissionCodes,
            permissionAttribute.Mode,
            permissionAttribute.AllowTemporary
        );
        
        if (!hasPermission)
        {
            // 6. 记录审计日志
            await auditLogger.LogPermissionDeniedAsync(
                userId,
                tenantId,
                context.Request.Path,
                permissionAttribute.PermissionCodes,
                context.Connection.RemoteIpAddress?.ToString()
            );
            
            // 7. 返回403
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new 
            { 
                error = "Forbidden",
                message = "权限不足",
                requiredPermissions = permissionAttribute.PermissionCodes
            });
            return;
        }
        
        // 8. 权限验证通过，继续处理
        await _next(context);
    }
    
    private bool IsWhitelisted(PathString path)
    {
        // 白名单路径：登录、健康检查等
        var whitelist = new[] 
        { 
            "/api/auth/login",
            "/api/health",
            "/swagger"
        };
        
        return whitelist.Any(w => path.StartsWithSegments(w));
    }
    
    private int GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim?.Value, out var userId) ? userId : 0;
    }
    
    private int? GetTenantId(ClaimsPrincipal user)
    {
        var tenantIdClaim = user.FindFirst("TenantId");
        return int.TryParse(tenantIdClaim?.Value, out var tenantId) ? tenantId : null;
    }
}
```

### 3. IPermissionService 接口

**文件位置**：`TcmAiDiagnosis.Domain/Services/IPermissionService.cs`

**功能**：定义权限服务接口

**设计**：
```csharp
public interface IPermissionService
{
    /// <summary>
    /// 检查用户是否具有指定权限
    /// </summary>
    Task<bool> CheckPermissionAsync(
        int userId, 
        int? tenantId, 
        string permissionCode,
        bool allowTemporary = true);
    
    /// <summary>
    /// 检查用户是否具有多个权限
    /// </summary>
    Task<bool> CheckPermissionsAsync(
        int userId,
        int? tenantId,
        string[] permissionCodes,
        PermissionCheckMode mode = PermissionCheckMode.RequireAll,
        bool allowTemporary = true);
    
    /// <summary>
    /// 获取用户的所有权限
    /// </summary>
    Task<List<string>> GetUserPermissionsAsync(
        int userId,
        int? tenantId,
        bool includeTemporary = true);
    
    /// <summary>
    /// 获取角色的所有权限
    /// </summary>
    Task<List<string>> GetRolePermissionsAsync(int roleId);
    
    /// <summary>
    /// 检查临时权限
    /// </summary>
    Task<bool> CheckTemporaryPermissionAsync(
        int userId,
        int? tenantId,
        string permissionCode);
    
    /// <summary>
    /// 刷新用户权限缓存
    /// </summary>
    Task RefreshUserPermissionCacheAsync(int userId, int? tenantId);
    
    /// <summary>
    /// 刷新角色权限缓存
    /// </summary>
    Task RefreshRolePermissionCacheAsync(int roleId);
}
```

### 4. PermissionService 实现

**文件位置**：`TcmAiDiagnosis.Domain/Services/PermissionService.cs`

**核心逻辑**：
```csharp
public class PermissionService : IPermissionService
{
    private readonly TcmAiDiagnosisContext _context;
    private readonly IPermissionCacheService _cacheService;
    private readonly ILogger<PermissionService> _logger;
    
    public async Task<bool> CheckPermissionsAsync(
        int userId,
        int? tenantId,
        string[] permissionCodes,
        PermissionCheckMode mode,
        bool allowTemporary)
    {
        // 1. 从缓存获取用户权限
        var userPermissions = await GetUserPermissionsAsync(
            userId, 
            tenantId, 
            allowTemporary);
        
        // 2. 根据模式检查权限
        if (mode == PermissionCheckMode.RequireAll)
        {
            return permissionCodes.All(code => 
                userPermissions.Contains(code));
        }
        else
        {
            return permissionCodes.Any(code => 
                userPermissions.Contains(code));
        }
    }
    
    public async Task<List<string>> GetUserPermissionsAsync(
        int userId,
        int? tenantId,
        bool includeTemporary)
    {
        // 1. 尝试从缓存获取
        var cacheKey = $"user_permissions:{userId}:{tenantId}";
        var cached = await _cacheService.GetAsync<List<string>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }
        
        // 2. 从数据库查询
        var permissions = new List<string>();
        
        // 2.1 获取用户的角色权限
        var rolePermissions = await _context.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Roles)
            .SelectMany(r => r.RolePermissions)
            .Where(rp => rp.IsGranted && rp.Permission.IsActive)
            .Select(rp => rp.Permission.PermissionCode)
            .Distinct()
            .ToListAsync();
        
        permissions.AddRange(rolePermissions);
        
        // 2.2 获取临时权限（如果允许）
        if (includeTemporary)
        {
            var now = DateTime.UtcNow;
            var tempPermissions = await _context.TemporaryPermissions
                .Where(tp => 
                    tp.UserId == userId &&
                    tp.TenantId == tenantId &&
                    tp.Status == TemporaryPermissionStatus.Active &&
                    tp.ValidFrom <= now &&
                    tp.ValidTo >= now)
                .Join(_context.Permissions,
                    tp => tp.PermissionId,
                    p => p.PermissionId,
                    (tp, p) => p.PermissionCode)
                .ToListAsync();
            
            permissions.AddRange(tempPermissions);
        }
        
        // 3. 缓存结果
        await _cacheService.SetAsync(
            cacheKey, 
            permissions, 
            TimeSpan.FromMinutes(30));
        
        return permissions;
    }
}
```

### 5. IPermissionCacheService 接口

**文件位置**：`TcmAiDiagnosis.Domain/Services/IPermissionCacheService.cs`

**功能**：权限缓存服务接口

**设计**：
```csharp
public interface IPermissionCacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
}
```

### 6. PermissionCacheService 实现

**文件位置**：`TcmAiDiagnosis.Domain/Services/PermissionCacheService.cs`

**实现**：使用 `IMemoryCache` 或 `IDistributedCache`

```csharp
public class PermissionCacheService : IPermissionCacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<PermissionCacheService> _logger;
    
    public async Task<T?> GetAsync<T>(string key)
    {
        return _memoryCache.TryGetValue(key, out T? value) ? value : default;
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30)
        };
        
        _memoryCache.Set(key, value, options);
    }
    
    public async Task RemoveAsync(string key)
    {
        _memoryCache.Remove(key);
    }
    
    public async Task RemoveByPatternAsync(string pattern)
    {
        // 实现模式匹配删除
        // 注意：IMemoryCache 不直接支持模式匹配
        // 需要维护一个键列表或使用 IDistributedCache
    }
}
```

### 7. IPermissionAuditLogger 接口

**文件位置**：`TcmAiDiagnosis.Domain/Services/IPermissionAuditLogger.cs`

**功能**：权限审计日志接口

**设计**：
```csharp
public interface IPermissionAuditLogger
{
    Task LogPermissionDeniedAsync(
        int userId,
        int? tenantId,
        string requestPath,
        string[] requiredPermissions,
        string? ipAddress);
    
    Task LogTemporaryPermissionUsedAsync(
        int userId,
        int? tenantId,
        string permissionCode,
        int temporaryPermissionId);
}
```

## 数据流设计

### 权限验证流程

```
1. HTTP请求到达
   ↓
2. Authentication Middleware 验证身份
   ↓
3. Permission Middleware 提取权限要求
   ↓
4. 检查用户是否已认证
   ├─ 否 → 返回 401
   └─ 是 → 继续
   ↓
5. 调用 PermissionService.CheckPermissionsAsync()
   ↓
6. 尝试从缓存获取用户权限
   ├─ 命中 → 使用缓存数据
   └─ 未命中 → 查询数据库
   ↓
7. 查询数据库
   ├─ 角色权限
   └─ 临时权限
   ↓
8. 缓存权限数据
   ↓
9. 验证权限
   ├─ 有权限 → 继续处理请求
   └─ 无权限 → 记录审计日志 → 返回 403
```

## 配置设计

**文件位置**：`TcmAiDiagnosis.Web/appsettings.json`

```json
{
  "PermissionSettings": {
    "EnablePermissionCheck": true,
    "CacheExpirationMinutes": 30,
    "WhitelistPaths": [
      "/api/auth/login",
      "/api/health",
      "/swagger"
    ],
    "AuditLogLevel": "Warning",
    "EnableDetailedErrors": false
  }
}
```

## 错误处理设计

### 错误响应格式

```json
{
  "error": "Forbidden",
  "message": "权限不足",
  "requiredPermissions": ["patient.create"],
  "timestamp": "2026-01-22T10:30:00Z",
  "path": "/api/patients",
  "traceId": "00-abc123..."
}
```

### 错误类型

1. **401 Unauthorized**：用户未认证
2. **403 Forbidden**：用户已认证但无权限
3. **500 Internal Server Error**：权限系统内部错误

## 性能优化设计

### 缓存策略

1. **用户权限缓存**：30分钟过期
2. **角色权限缓存**：60分钟过期
3. **权限定义缓存**：永久缓存（手动刷新）

### 数据库查询优化

1. 使用索引优化权限查询
2. 使用 `Include` 预加载关联数据
3. 使用 `AsNoTracking` 提高查询性能

### 异步处理

1. 所有权限检查使用异步方法
2. 审计日志异步写入
3. 缓存操作异步执行

## 测试策略

### 单元测试

1. 测试 `RequirePermissionAttribute` 的属性设置
2. 测试 `PermissionService` 的各个方法
3. 测试缓存服务的读写操作
4. 测试权限验证逻辑的各种场景

### 集成测试

1. 测试中间件的完整流程
2. 测试与数据库的交互
3. 测试缓存的有效性
4. 测试多租户隔离

### 性能测试

1. 测试权限检查的响应时间
2. 测试高并发场景下的性能
3. 测试缓存命中率

## 部署考虑

### 分布式部署

1. 使用 Redis 作为分布式缓存
2. 确保所有实例使用相同的缓存
3. 考虑缓存一致性问题

### 监控和告警

1. 监控权限验证失败率
2. 监控缓存命中率
3. 监控权限检查响应时间
4. 设置异常告警

---

**文档版本**：v1.0  
**创建时间**：2026-01-22  
**状态**：待审核
