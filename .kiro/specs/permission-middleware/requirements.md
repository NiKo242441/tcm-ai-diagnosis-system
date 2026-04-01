# 权限验证中间件 - 需求文档

## 简介

本文档定义了权限验证中间件系统的需求，该系统将为TCM AI诊断系统提供细粒度的权限控制，确保用户只能访问其被授权的功能和数据。

## 术语表

- **Permission（权限）**：系统中定义的操作权限点，如 `patient.create`
- **Role（角色）**：用户角色，如医生、患者、管理员
- **User（用户）**：系统使用者
- **Middleware（中间件）**：ASP.NET Core请求处理管道中的组件
- **Attribute（特性）**：用于标注控制器或方法的权限要求
- **Cache（缓存）**：用于提高权限检查性能的内存存储
- **Tenant（租户）**：多租户系统中的租户实体

## 需求

### 需求 1：权限验证特性

**用户故事**：作为开发人员，我想使用特性标注来声明API端点的权限要求，以便清晰地表达访问控制规则。

#### 验收标准

1. THE System SHALL 提供 `[RequirePermission]` 特性用于标注控制器和方法
2. WHEN 特性应用于控制器时，THE System SHALL 对该控制器的所有方法应用权限检查
3. WHEN 特性应用于方法时，THE System SHALL 仅对该方法应用权限检查
4. THE System SHALL 支持在单个特性中指定多个权限代码
5. THE System SHALL 支持 `RequireAll`（需要所有权限）和 `RequireAny`（需要任一权限）两种模式

### 需求 2：权限验证中间件

**用户故事**：作为系统架构师，我想要一个中间件来拦截所有HTTP请求并验证权限，以便统一处理权限控制逻辑。

#### 验收标准

1. WHEN 请求到达时，THE Middleware SHALL 检查目标端点是否有权限要求
2. WHEN 端点有权限要求时，THE Middleware SHALL 验证当前用户是否具有所需权限
3. WHEN 用户未认证时，THE Middleware SHALL 返回401 Unauthorized响应
4. WHEN 用户已认证但无权限时，THE Middleware SHALL 返回403 Forbidden响应
5. WHEN 用户具有所需权限时，THE Middleware SHALL 允许请求继续处理
6. THE Middleware SHALL 记录所有权限验证失败的尝试到审计日志

### 需求 3：权限服务

**用户故事**：作为开发人员，我想要一个权限服务来查询和验证用户权限，以便在业务逻辑中进行权限检查。

#### 验收标准

1. THE Permission_Service SHALL 提供方法检查用户是否具有指定权限
2. THE Permission_Service SHALL 提供方法获取用户的所有权限列表
3. THE Permission_Service SHALL 提供方法获取角色的所有权限列表
4. THE Permission_Service SHALL 支持检查临时权限
5. THE Permission_Service SHALL 考虑权限的有效期和状态
6. THE Permission_Service SHALL 支持多租户隔离

### 需求 4：权限缓存

**用户故事**：作为系统管理员，我想要权限数据被缓存，以便提高系统性能并减少数据库查询。

#### 验收标准

1. THE System SHALL 缓存用户的权限列表
2. THE System SHALL 缓存角色的权限列表
3. WHEN 权限数据变更时，THE System SHALL 自动刷新相关缓存
4. THE System SHALL 支持配置缓存过期时间
5. THE System SHALL 使用分布式缓存支持多实例部署
6. THE System SHALL 提供手动清除缓存的方法

### 需求 5：临时权限支持

**用户故事**：作为系统管理员，我想要支持临时权限，以便处理会诊、跨租户转诊等特殊场景。

#### 验收标准

1. WHEN 检查权限时，THE System SHALL 同时检查永久权限和临时权限
2. THE System SHALL 验证临时权限的有效期
3. THE System SHALL 验证临时权限的状态（Active/Expired/Revoked）
4. WHEN 临时权限过期时，THE System SHALL 自动拒绝访问
5. THE System SHALL 记录临时权限的使用情况

### 需求 6：多租户隔离

**用户故事**：作为租户管理员，我想要权限系统支持多租户隔离，以便确保租户间的数据安全。

#### 验收标准

1. THE System SHALL 在权限检查时验证用户的租户ID
2. THE System SHALL 确保用户只能访问其所属租户的数据
3. THE System SHALL 支持跨租户权限（如转诊场景）
4. WHEN 用户尝试访问其他租户数据时，THE System SHALL 拒绝访问
5. THE System SHALL 记录所有跨租户访问尝试

### 需求 7：审计日志

**用户故事**：作为安全审计员，我想要记录所有权限验证活动，以便进行安全审计和问题追踪。

#### 验收标准

1. THE System SHALL 记录所有权限验证失败的尝试
2. THE System SHALL 记录访问的用户、时间、IP地址、请求路径
3. THE System SHALL 记录被拒绝的权限代码
4. THE System SHALL 记录临时权限的使用
5. THE System SHALL 支持按用户、时间、权限等维度查询日志

### 需求 8：性能要求

**用户故事**：作为系统架构师，我想要权限验证系统具有高性能，以便不影响用户体验。

#### 验收标准

1. THE System SHALL 在99%的情况下在10ms内完成权限检查
2. THE System SHALL 使用缓存减少数据库查询
3. THE System SHALL 支持异步权限检查
4. THE System SHALL 在高并发场景下保持稳定性能
5. THE System SHALL 提供性能监控指标

### 需求 9：配置管理

**用户故事**：作为系统管理员，我想要能够配置权限系统的行为，以便适应不同的部署环境。

#### 验收标准

1. THE System SHALL 支持通过配置文件配置缓存策略
2. THE System SHALL 支持配置是否启用权限验证
3. THE System SHALL 支持配置白名单路径（无需权限验证）
4. THE System SHALL 支持配置审计日志级别
5. THE System SHALL 支持热更新配置（无需重启）

### 需求 10：错误处理

**用户故事**：作为开发人员，我想要权限系统提供清晰的错误信息，以便快速定位和解决问题。

#### 验收标准

1. WHEN 权限验证失败时，THE System SHALL 返回清晰的错误消息
2. THE System SHALL 区分"未认证"和"无权限"两种错误
3. THE System SHALL 在开发环境提供详细的调试信息
4. THE System SHALL 在生产环境隐藏敏感信息
5. THE System SHALL 记录所有异常到日志系统

## 特殊场景

### 会诊场景

在会诊场景中，医生需要临时访问其他医生的患者病历：

1. THE System SHALL 支持授予临时的病历查看权限
2. THE System SHALL 验证会诊申请的审批状态
3. THE System SHALL 在会诊结束后自动撤销临时权限

### 跨租户转诊

在跨租户转诊场景中，需要在不同租户间共享患者数据：

1. THE System SHALL 支持跨租户的临时数据访问权限
2. THE System SHALL 验证转诊申请的审批状态
3. THE System SHALL 记录所有跨租户数据访问

### 系统维护

在系统维护场景中，技术支持人员需要临时访问租户数据：

1. THE System SHALL 支持授予临时的系统维护权限
2. THE System SHALL 要求维护权限必须经过审批
3. THE System SHALL 记录所有维护操作的详细日志

## 非功能需求

### 安全性

1. 权限验证必须在业务逻辑执行前完成
2. 权限数据传输必须加密
3. 缓存的权限数据必须定期刷新
4. 必须防止权限提升攻击

### 可维护性

1. 权限代码必须集中管理
2. 权限验证逻辑必须可测试
3. 必须提供清晰的文档和示例

### 可扩展性

1. 必须支持添加新的权限类型
2. 必须支持自定义权限验证逻辑
3. 必须支持插件式扩展

## 约束条件

1. 必须与现有的ASP.NET Core Identity系统集成
2. 必须支持.NET 8.0
3. 必须兼容MySQL数据库
4. 必须支持分布式部署

## 依赖关系

1. 依赖第一阶段的数据库设计
2. 依赖第二阶段的数据初始化
3. 依赖ASP.NET Core Identity
4. 依赖Entity Framework Core

---

**文档版本**：v1.0  
**创建时间**：2026-01-22  
**状态**：待审核
