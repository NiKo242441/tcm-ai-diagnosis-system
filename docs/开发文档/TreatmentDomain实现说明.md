# TreatmentDomain 实现说明文档

## 概述

`TreatmentDomain` 是智能治疗方案生成模块的核心业务逻辑类，位于 `TcmAiDiagnosis.Domain` 命名空间中。该类负责实现从证候确认到AI治疗方案生成的完整流程，包括数据验证、API调用、响应解析和数据持久化等关键功能。本文档详细说明了该类的设计理念、实现细节和使用方法。

## 类结构概览

```csharp
namespace TcmAiDiagnosis.Domain
{
    /// <summary>
    /// 治疗方案业务逻辑类
    /// 负责治疗方案的生成、查询和管理功能
    /// </summary>
    public class TreatmentDomain
    {
        // 依赖注入的服务
        private readonly TcmAiDiagnosisContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DifyApiOptions _difyApiOptions;
        private readonly ILogger<TreatmentDomain> _logger;
        private readonly SyndromeDomain _syndromeDomain;
        private readonly VisitDomain _visitDomain;
        private readonly TreatmentDataValidator _treatmentDataValidator;
        private readonly RetryPolicyService _retryPolicyService;
        
        // 公共方法
        public async Task<TreatmentDto?> GetLatestTreatmentBySyndromeIdAsync(int syndromeId);
        public async Task GenerateAndSaveAiTreatmentAsync(int syndromeId);
        
        // 私有方法（核心业务逻辑）
        private async Task<Dictionary<string, object>> PrepareDifyInputsAsync(Syndrome syndrome, Visit visit);
        private async Task<string> CallDifyTreatmentApiAsync(Dictionary<string, object> inputs);
        private async Task UpdateTreatmentFromApiResponseAsync(Treatment treatment, string apiResponse);
        private async Task ParseAndUpdateTreatmentDataAsync(Treatment treatment, JsonElement treatmentData);
        
        // 数据解析方法（九大治疗模块）
        private async Task ParsePrescriptionsAsync(Treatment treatment, JsonElement prescriptionsElement);
        private async Task ParseHerbalWarningsAsync(Treatment treatment, JsonElement warningsElement);
        private async Task ParseDietaryTherapiesAsync(Treatment treatment, JsonElement therapiesElement);
        private async Task ParseAcupunctureAsync(Treatment treatment, JsonElement acupunctureElement);
        private async Task ParseMoxibustionAsync(Treatment treatment, JsonElement moxibustionElement);
        private async Task ParseCuppingAsync(Treatment treatment, JsonElement cuppingElement);
        private async Task ParseLifestyleAdviceAsync(Treatment treatment, JsonElement lifestyleElement);
        private async Task ParseDietaryAdviceAsync(Treatment treatment, JsonElement dietaryElement);
        private async Task ParseFollowUpAdviceAsync(Treatment treatment, JsonElement followUpElement);
        
        // 工具方法
        private static string GetStringProperty(JsonElement element, string propertyName);
    }
}
```

## 设计理念

### 1. 高性能并发控制
- **分布式锁机制**：采用数据库唯一约束实现分布式锁，通过创建占位符记录防止重复生成
- **先检查后锁定策略**：在获取锁之前先快速检查是否已存在治疗方案，避免不必要的锁竞争
- **异步处理模式**：使用异步方法处理耗时的API调用和数据库操作，提高系统并发能力
- **资源优化**：避免重复的API调用和数据库操作，减少系统资源消耗

### 2. 完整的错误处理和恢复机制
- **多层次异常处理**：在方法调用、API请求、数据解析等各个层面实现异常捕获和处理
- **自动恢复机制**：生成失败时自动清理占位符记录，允许重新生成
- **详细日志记录**：记录关键操作的执行过程和异常信息，便于问题诊断和性能监控
- **优雅降级**：在部分数据解析失败时，仍能保存成功解析的部分，避免全盘失败

### 3. 数据验证与安全保障
- **多层数据验证**：通过 `TreatmentDataValidator` 实现输入数据、API响应和实体数据的全面验证
- **JSON安全解析**：使用 `System.Text.Json` 进行安全的JSON解析，防止恶意数据注入
- **业务规则验证**：确保生成的治疗方案符合中医诊疗的业务规则和数据完整性要求
- **数据完整性保护**：通过事务机制确保数据的一致性和完整性

### 4. 可靠的外部API集成
- **重试机制**：通过 `RetryPolicyService` 实现智能重试，处理网络波动和临时故障
- **超时控制**：设置合理的HTTP请求超时时间，避免长时间等待
- **连接池管理**：使用 `IHttpClientFactory` 管理HTTP连接，提高性能和资源利用率
- **API响应验证**：验证Dify API响应的格式和内容，确保数据质量

### 5. 模块化数据处理架构
- **九大治疗模块**：按照中医治疗的九个核心模块（中药处方、针灸、艾灸、拔罐、食疗、生活方式建议、饮食建议、随访建议、中药安全警告）进行模块化处理
- **独立解析逻辑**：每个治疗模块都有独立的解析方法，便于维护和扩展
- **关联数据管理**：正确处理主表和从表的关联关系，确保数据结构的完整性
- **灵活的数据映射**：支持JSON字段到实体属性的灵活映射，适应API响应格式的变化

## 构造函数和依赖注入

### 构造函数签名

```csharp
public TreatmentDomain(
    TcmAiDiagnosisContext context,
    IMapper mapper,
    IHttpClientFactory httpClientFactory,
    IOptions<DifyApiOptions> difyApiOptions,
    ILogger<TreatmentDomain> logger,
    SyndromeDomain syndromeDomain,
    VisitDomain visitDomain,
    TreatmentDataValidator treatmentDataValidator,
    RetryPolicyService retryPolicyService)
```

### 依赖服务详细说明

#### 1. 数据访问层依赖

**`TcmAiDiagnosisContext _context`**
- **作用**：Entity Framework Core 数据库上下文
- **用途**：执行所有数据库操作，包括查询、插入、更新和删除治疗方案相关数据
- **关键操作**：
  - 查询现有治疗方案
  - 创建占位符记录实现分布式锁
  - 保存完整的治疗方案数据
  - 管理九大治疗模块的关联数据

**`IMapper _mapper`**
- **作用**：AutoMapper 对象映射器
- **用途**：实现实体对象与DTO对象之间的自动映射
- **映射场景**：
  - `Treatment` 实体到 `TreatmentDto` 的转换
  - 包含所有关联数据的复杂对象映射
  - 确保数据传输对象的完整性和一致性

#### 2. 外部服务集成依赖

**`IHttpClientFactory _httpClientFactory`**
- **作用**：HTTP客户端工厂，管理HTTP连接池
- **用途**：创建和管理与Dify API通信的HTTP客户端
- **优势**：
  - 连接池管理，提高性能
  - 自动处理连接生命周期
  - 支持配置超时和重试策略
  - 避免Socket耗尽问题

**`DifyApiOptions _difyApiOptions`**
- **作用**：Dify API配置选项类
- **配置内容**：
  ```csharp
  public class DifyApiOptions
  {
      public string BaseUrl { get; set; }                    // API基础URL
      public string TreatmentWorkflowApiKey { get; set; }    // 工作流API密钥
      public string TreatmentEndpoint { get; set; }          // 治疗方案端点
      public string ResponseMode { get; set; }               // 响应模式（blocking）
      public string User { get; set; }                       // 用户标识
      public int TimeoutSeconds { get; set; }                // 超时时间（秒）
  }
  ```
- **注入方式**：通过 `IOptions<DifyApiOptions>` 模式注入，支持配置热更新

#### 3. 日志和监控依赖

**`ILogger<TreatmentDomain> _logger`**
- **作用**：结构化日志记录器
- **日志级别使用**：
  - `LogInformation`：记录正常业务流程和关键操作
  - `LogWarning`：记录可恢复的异常和性能警告
  - `LogError`：记录严重错误和异常情况
- **日志内容**：
  - API调用的请求和响应信息
  - 数据解析过程中的异常
  - 并发锁的获取和释放状态
  - 性能监控数据

#### 4. 业务逻辑依赖

**`SyndromeDomain _syndromeDomain`**
- **作用**：证候业务逻辑处理类
- **用途**：
  - 验证证候数据的有效性
  - 获取证候的详细信息
  - 确保证候与治疗方案的关联正确性

**`VisitDomain _visitDomain`**
- **作用**：就诊业务逻辑处理类
- **用途**：
  - 获取与证候关联的就诊信息
  - 验证就诊数据的完整性
  - 提供患者基本信息用于治疗方案生成

#### 5. 数据验证和服务依赖

**`TreatmentDataValidator _treatmentDataValidator`**
- **作用**：治疗方案数据验证器
- **验证范围**：
  - 输入数据格式和完整性验证
  - API响应数据的结构验证
  - 治疗方案实体的业务规则验证
  - 九大治疗模块数据的专业性验证

**`RetryPolicyService _retryPolicyService`**
- **作用**：重试策略服务
- **重试配置**：
  - 最大重试次数：3次
  - 基础延迟时间：1000毫秒
  - 指数退避算法：每次重试延迟时间递增
  - 可重试异常类型：网络异常、超时异常、服务器5xx错误
- **使用场景**：
  - Dify API调用失败时的自动重试
  - 数据库连接异常的重试处理
  - 网络波动导致的临时故障恢复

### 依赖注入配置示例

```csharp
// Program.cs 或 Startup.cs 中的服务注册
services.AddScoped<TreatmentDomain>();
services.AddScoped<SyndromeDomain>();
services.AddScoped<VisitDomain>();
services.AddScoped<TreatmentDataValidator>();
services.AddScoped<RetryPolicyService>();

// HTTP客户端工厂注册
services.AddHttpClient();

// 配置选项注册
services.Configure<DifyApiOptions>(configuration.GetSection("DifyApi"));

// AutoMapper注册
services.AddAutoMapper(typeof(TreatmentMappingProfile));

// Entity Framework注册
services.AddDbContext<TcmAiDiagnosisContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
```

## 核心功能

### 1. 获取治疗方案 (`GetLatestTreatmentBySyndromeIdAsync`)

```csharp
/// <summary>
/// 获取指定证候ID的最新治疗方案
/// </summary>
/// <param name="syndromeId">证候ID</param>
/// <returns>治疗方案详情DTO，如果不存在则返回null</returns>
/// <exception cref="ArgumentException">当证候ID无效时抛出</exception>
public async Task<TreatmentDto?> GetLatestTreatmentBySyndromeIdAsync(int syndromeId)
```

**实现逻辑：**
1. **参数验证**：检查 `syndromeId` 是否大于0
2. **数据库查询**：使用EF Core查询最新的治疗方案
3. **关联数据加载**：通过 `Include` 方法预加载所有九大治疗模块的关联数据
4. **状态过滤**：只返回状态为 `Generated`（已生成）的治疗方案
5. **对象映射**：使用AutoMapper将实体转换为DTO

**查询优化：**
```csharp
var treatment = await _context.Treatments
    .Include(t => t.Prescriptions).ThenInclude(p => p.Herbs)
    .Include(t => t.HerbalWarnings).ThenInclude(hw => hw.AffectedMedications)
    .Include(t => t.DietaryTherapies).ThenInclude(dt => dt.Ingredients)
    .Include(t => t.AcupuncturePoints)
    .Include(t => t.MoxibustionPoints)
    .Include(t => t.CuppingPoints)
    .Include(t => t.LifestyleAdvice)
    .Include(t => t.DietaryAdvice).ThenInclude(da => da.RecommendedFoods)
    .Include(t => t.DietaryAdvice).ThenInclude(da => da.AvoidedFoods)
    .Include(t => t.FollowUpAdvice).ThenInclude(fa => fa.MonitoringIndicators)
    .Where(t => t.SyndromeId == syndromeId && t.Status == TreatmentStatusEnum.Generated)
    .OrderByDescending(t => t.CreatedAt)
    .FirstOrDefaultAsync();
```

**使用场景：**
- 治疗方案详情页面的数据加载
- 检查指定证候是否已有治疗方案
- 前端轮询检查治疗方案生成状态
- 医生查看完整的治疗建议

### 2. 异步生成治疗方案 (`GenerateAndSaveAiTreatmentAsync`)

```csharp
/// <summary>
/// 异步生成并保存AI治疗方案 - 核心业务方法
/// 实现高性能并发控制，采用"先检查，后锁定"(Check-Then-Lock)策略
/// </summary>
/// <param name="syndromeId">证候ID</param>
/// <returns>异步任务</returns>
/// <exception cref="ArgumentException">当证候ID无效时抛出</exception>
/// <exception cref="InvalidOperationException">当证候或就诊数据无效时抛出</exception>
/// <exception cref="ConcurrencyLockException">当并发锁竞争失败时抛出</exception>
/// <exception cref="TreatmentGenerationFailedException">当治疗方案生成失败时抛出</exception>
public async Task GenerateAndSaveAiTreatmentAsync(int syndromeId)
```

**详细执行流程：**

#### 第一阶段：快速检查和参数验证
1. **参数验证**：
   ```csharp
   if (syndromeId <= 0)
       throw new ArgumentException("证候ID必须大于0", nameof(syndromeId));
   ```

2. **快速存在性检查**：
   ```csharp
   var existingTreatment = await _context.Treatments
       .Where(t => t.SyndromeId == syndromeId && t.Status == TreatmentStatusEnum.Generated)
       .FirstOrDefaultAsync();
   
   if (existingTreatment != null)
   {
       _logger.LogInformation("证候 {SyndromeId} 的治疗方案已存在，跳过生成", syndromeId);
       return;
   }
   ```

#### 第二阶段：数据验证和准备
3. **证候数据验证**：
   ```csharp
   var syndrome = await _context.Syndromes
       .Include(s => s.Visit)
       .ThenInclude(v => v.Patient)
       .FirstOrDefaultAsync(s => s.Id == syndromeId);
   
   if (syndrome == null)
       throw new InvalidOperationException($"未找到ID为 {syndromeId} 的证候");
   ```

4. **关联数据完整性检查**：
   - 验证就诊信息是否存在
   - 验证患者信息是否完整
   - 检查证候诊断结果是否有效

#### 第三阶段：分布式锁获取
5. **创建占位符记录**（实现分布式锁）：
   ```csharp
   var placeholderTreatment = new Treatment
   {
       SyndromeId = syndromeId,
       Status = TreatmentStatusEnum.Generating,
       CreatedAt = DateTime.UtcNow,
       UpdatedAt = DateTime.UtcNow
   };
   
   try
   {
       _context.Treatments.Add(placeholderTreatment);
       await _context.SaveChangesAsync();
       _logger.LogInformation("成功获取证候 {SyndromeId} 的生成锁", syndromeId);
   }
   catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
   {
       throw new ConcurrencyLockException($"证候 {syndromeId} 的治疗方案正在生成中", ex);
   }
   ```

#### 第四阶段：AI治疗方案生成
6. **准备API输入数据**：
   ```csharp
   var inputs = await PrepareDifyInputsAsync(syndrome, syndrome.Visit);
   ```

7. **调用Dify API**：
   ```csharp
   var apiResponse = await CallDifyTreatmentApiAsync(inputs);
   ```

8. **解析和保存治疗方案**：
   ```csharp
   await UpdateTreatmentFromApiResponseAsync(placeholderTreatment, apiResponse);
   ```

#### 第五阶段：状态更新和清理
9. **更新治疗方案状态**：
   ```csharp
   placeholderTreatment.Status = TreatmentStatusEnum.Generated;
   placeholderTreatment.UpdatedAt = DateTime.UtcNow;
   await _context.SaveChangesAsync();
   ```

10. **异常处理和清理**：
    ```csharp
    catch (Exception ex)
    {
        // 清理占位符记录，允许重新生成
        _context.Treatments.Remove(placeholderTreatment);
        await _context.SaveChangesAsync();
        
        _logger.LogError(ex, "证候 {SyndromeId} 的治疗方案生成失败", syndromeId);
        throw new TreatmentGenerationFailedException($"治疗方案生成失败: {ex.Message}", ex);
    }
    ```

**并发控制机制详解：**

- **分布式锁实现**：利用数据库的唯一约束（`SyndromeId` 字段）实现分布式锁
- **占位符模式**：创建状态为 `Generating` 的记录作为锁标识
- **锁竞争处理**：当多个请求同时尝试生成时，只有一个能成功创建占位符
- **自动清理机制**：生成失败时自动删除占位符，允许重新尝试
- **超时保护**：通过API超时设置防止长时间占用锁

**性能优化特性：**

- **先检查后锁定**：避免不必要的锁竞争
- **异步处理**：不阻塞调用线程
- **批量数据加载**：使用 `Include` 预加载关联数据
- **连接池复用**：通过 `IHttpClientFactory` 管理HTTP连接

**错误恢复机制：**

- **网络异常**：通过重试策略自动恢复
- **API异常**：记录详细错误信息并清理资源
- **数据库异常**：回滚事务并释放锁
- **解析异常**：保存部分成功的数据，记录失败的模块

## 私有方法实现详解

### 1. API输入数据准备 (`PrepareDifyInputsAsync`)

```csharp
/// <summary>
/// 准备Dify API调用所需的输入数据
/// </summary>
/// <param name="syndrome">证候实体</param>
/// <param name="visit">就诊实体</param>
/// <returns>API输入数据字典</returns>
private async Task<Dictionary<string, object>> PrepareDifyInputsAsync(Syndrome syndrome, Visit visit)
```

**功能说明：**
- 构建符合Dify API要求的输入数据结构
- 整合证候、患者和就诊信息
- 确保数据格式符合AI模型的输入要求

**数据结构组织：**
```csharp
var inputs = new Dictionary<string, object>
{
    // 患者基本信息
    ["patient_name"] = visit.Patient.Name,
    ["patient_age"] = visit.Patient.Age,
    ["patient_gender"] = visit.Patient.Gender,
    
    // 就诊信息
    ["chief_complaint"] = visit.ChiefComplaint,
    ["present_illness"] = visit.PresentIllness,
    ["past_history"] = visit.PastHistory,
    ["physical_examination"] = visit.PhysicalExamination,
    
    // 证候诊断结果
    ["syndrome_name"] = syndrome.Name,
    ["syndrome_description"] = syndrome.Description,
    ["syndrome_symptoms"] = syndrome.Symptoms,
    ["syndrome_tongue"] = syndrome.TongueCondition,
    ["syndrome_pulse"] = syndrome.PulseCondition,
    
    // 诊断结论
    ["diagnosis_conclusion"] = syndrome.DiagnosisConclusion,
    ["treatment_principle"] = syndrome.TreatmentPrinciple
};
```

### 2. Dify API调用 (`CallDifyTreatmentApiAsync`)

```csharp
/// <summary>
/// 调用Dify治疗方案生成API
/// </summary>
/// <param name="inputs">API输入数据</param>
/// <returns>API响应JSON字符串</returns>
private async Task<string> CallDifyTreatmentApiAsync(Dictionary<string, object> inputs)
```

**实现特性：**
- **重试机制**：通过 `RetryPolicyService` 实现智能重试
- **超时控制**：设置HTTP请求超时时间
- **错误分类**：区分可重试和不可重试的错误
- **详细日志**：记录请求和响应的详细信息

**HTTP请求构建：**
```csharp
var httpClient = _httpClientFactory.CreateClient();
httpClient.Timeout = TimeSpan.FromSeconds(_difyApiOptions.TimeoutSeconds);

var requestBody = new
{
    inputs = inputs,
    response_mode = _difyApiOptions.ResponseMode,
    user = _difyApiOptions.User
};

var jsonContent = JsonSerializer.Serialize(requestBody);
var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
{
    Content = content
};
request.Headers.Add("Authorization", $"Bearer {_difyApiOptions.TreatmentWorkflowApiKey}");
```

**重试策略配置：**
- **最大重试次数**：3次
- **基础延迟**：1000毫秒
- **指数退避**：每次重试延迟时间翻倍
- **可重试异常**：`HttpRequestException`、`TaskCanceledException`、HTTP 5xx错误

### 3. API响应处理 (`UpdateTreatmentFromApiResponseAsync`)

```csharp
/// <summary>
/// 解析API响应并更新治疗方案实体
/// </summary>
/// <param name="treatment">治疗方案实体</param>
/// <param name="apiResponse">API响应JSON字符串</param>
private async Task UpdateTreatmentFromApiResponseAsync(Treatment treatment, string apiResponse)
```

**响应解析流程：**
1. **JSON格式验证**：验证响应是否为有效JSON
2. **响应结构检查**：验证必需字段是否存在
3. **数据提取**：提取治疗方案数据部分
4. **模块化解析**：调用九大治疗模块的解析方法

**JSON结构处理：**
```csharp
var jsonDocument = JsonDocument.Parse(apiResponse);
var root = jsonDocument.RootElement;

// 检查响应状态
if (!root.TryGetProperty("data", out var dataElement))
    throw new ApiResponseParseException("API响应缺少data字段");

if (!dataElement.TryGetProperty("outputs", out var outputsElement))
    throw new ApiResponseParseException("API响应缺少outputs字段");

if (!outputsElement.TryGetProperty("treatment_plan", out var treatmentElement))
    throw new ApiResponseParseException("API响应缺少treatment_plan字段");

// 更新基本信息
treatment.GeneratedContent = treatmentElement.GetRawText();
treatment.ApiResponseRaw = apiResponse;

// 解析九大治疗模块
await ParseAndUpdateTreatmentDataAsync(treatment, treatmentElement);
```

### 4. 治疗数据模块化解析 (`ParseAndUpdateTreatmentDataAsync`)

```csharp
/// <summary>
/// 解析和更新治疗数据的九大模块
/// </summary>
/// <param name="treatment">治疗方案实体</param>
/// <param name="treatmentData">治疗数据JSON元素</param>
private async Task ParseAndUpdateTreatmentDataAsync(Treatment treatment, JsonElement treatmentData)
```

**九大治疗模块解析：**

#### 4.1 中药处方解析 (`ParsePrescriptionsAsync`)
```csharp
private async Task ParsePrescriptionsAsync(Treatment treatment, JsonElement prescriptionsElement)
{
    foreach (var prescriptionElement in prescriptionsElement.EnumerateArray())
    {
        var prescription = new Prescription
        {
            TreatmentId = treatment.Id,
            Name = GetStringProperty(prescriptionElement, "name"),
            Composition = GetStringProperty(prescriptionElement, "composition"),
            Dosage = GetStringProperty(prescriptionElement, "dosage"),
            Usage = GetStringProperty(prescriptionElement, "usage"),
            Efficacy = GetStringProperty(prescriptionElement, "efficacy"),
            Herbs = new List<Herb>()
        };

        // 解析单味药材
        if (prescriptionElement.TryGetProperty("herbs", out var herbsElement))
        {
            foreach (var herbElement in herbsElement.EnumerateArray())
            {
                var herb = new Herb
                {
                    Name = GetStringProperty(herbElement, "name"),
                    Dosage = GetStringProperty(herbElement, "dosage"),
                    Usage = GetStringProperty(herbElement, "usage"),
                    Efficacy = GetStringProperty(herbElement, "efficacy")
                };
                prescription.Herbs.Add(herb);
            }
        }

        treatment.Prescriptions.Add(prescription);
    }
}
```

#### 4.2 中药安全警告解析 (`ParseHerbalWarningsAsync`)
```csharp
private async Task ParseHerbalWarningsAsync(Treatment treatment, JsonElement warningsElement)
{
    foreach (var warningElement in warningsElement.EnumerateArray())
    {
        var warning = new HerbalWarning
        {
            TreatmentId = treatment.Id,
            HerbName = GetStringProperty(warningElement, "herb_name"),
            WarningType = GetStringProperty(warningElement, "warning_type"),
            Description = GetStringProperty(warningElement, "description"),
            Severity = GetStringProperty(warningElement, "severity"),
            AffectedMedications = new List<AffectedMedication>()
        };

        // 解析影响的药物
        if (warningElement.TryGetProperty("affected_medications", out var medicationsElement))
        {
            foreach (var medicationElement in medicationsElement.EnumerateArray())
            {
                var medication = new AffectedMedication
                {
                    MedicationName = GetStringProperty(medicationElement, "medication_name"),
                    InteractionType = GetStringProperty(medicationElement, "interaction_type"),
                    RiskLevel = GetStringProperty(medicationElement, "risk_level")
                };
                warning.AffectedMedications.Add(medication);
            }
        }

        treatment.HerbalWarnings.Add(warning);
    }
}
```

#### 4.3 食疗方案解析 (`ParseDietaryTherapiesAsync`)
```csharp
private async Task ParseDietaryTherapiesAsync(Treatment treatment, JsonElement therapiesElement)
{
    foreach (var therapyElement in therapiesElement.EnumerateArray())
    {
        var therapy = new DietaryTherapy
        {
            TreatmentId = treatment.Id,
            Name = GetStringProperty(therapyElement, "name"),
            Ingredients = GetStringProperty(therapyElement, "ingredients"),
            Preparation = GetStringProperty(therapyElement, "preparation"),
            Usage = GetStringProperty(therapyElement, "usage"),
            Efficacy = GetStringProperty(therapyElement, "efficacy"),
            Ingredients = new List<DietaryIngredient>()
        };

        // 解析食材成分
        if (therapyElement.TryGetProperty("ingredient_list", out var ingredientsElement))
        {
            foreach (var ingredientElement in ingredientsElement.EnumerateArray())
            {
                var ingredient = new DietaryIngredient
                {
                    Name = GetStringProperty(ingredientElement, "name"),
                    Amount = GetStringProperty(ingredientElement, "amount"),
                    Properties = GetStringProperty(ingredientElement, "properties"),
                    Benefits = GetStringProperty(ingredientElement, "benefits")
                };
                therapy.Ingredients.Add(ingredient);
            }
        }

        treatment.DietaryTherapies.Add(therapy);
    }
}
```

#### 4.4 针灸穴位解析 (`ParseAcupunctureAsync`)
```csharp
private async Task ParseAcupunctureAsync(Treatment treatment, JsonElement acupunctureElement)
{
    foreach (var pointElement in acupunctureElement.EnumerateArray())
    {
        var acupuncturePoint = new AcupuncturePoint
        {
            TreatmentId = treatment.Id,
            Name = GetStringProperty(pointElement, "name"),
            Location = GetStringProperty(pointElement, "location"),
            Method = GetStringProperty(pointElement, "method"),
            Depth = GetStringProperty(pointElement, "depth"),
            Duration = GetStringProperty(pointElement, "duration"),
            Frequency = GetStringProperty(pointElement, "frequency"),
            Efficacy = GetStringProperty(pointElement, "efficacy"),
            Precautions = GetStringProperty(pointElement, "precautions")
        };

        treatment.AcupuncturePoints.Add(acupuncturePoint);
    }
}
```

#### 4.5 艾灸穴位解析 (`ParseMoxibustionAsync`)
```csharp
private async Task ParseMoxibustionAsync(Treatment treatment, JsonElement moxibustionElement)
{
    foreach (var pointElement in moxibustionElement.EnumerateArray())
    {
        var moxibustionPoint = new MoxibustionPoint
        {
            TreatmentId = treatment.Id,
            Name = GetStringProperty(pointElement, "name"),
            Location = GetStringProperty(pointElement, "location"),
            Method = GetStringProperty(pointElement, "method"),
            Duration = GetStringProperty(pointElement, "duration"),
            Temperature = GetStringProperty(pointElement, "temperature"),
            Frequency = GetStringProperty(pointElement, "frequency"),
            Efficacy = GetStringProperty(pointElement, "efficacy"),
            Precautions = GetStringProperty(pointElement, "precautions")
        };

        treatment.MoxibustionPoints.Add(moxibustionPoint);
    }
}
```

#### 4.6 拔罐部位解析 (`ParseCuppingAsync`)
```csharp
private async Task ParseCuppingAsync(Treatment treatment, JsonElement cuppingElement)
{
    foreach (var pointElement in cuppingElement.EnumerateArray())
    {
        var cuppingPoint = new CuppingPoint
        {
            TreatmentId = treatment.Id,
            Name = GetStringProperty(pointElement, "name"),
            Location = GetStringProperty(pointElement, "location"),
            Method = GetStringProperty(pointElement, "method"),
            Duration = GetStringProperty(pointElement, "duration"),
            Pressure = GetStringProperty(pointElement, "pressure"),
            Frequency = GetStringProperty(pointElement, "frequency"),
            Efficacy = GetStringProperty(pointElement, "efficacy"),
            Precautions = GetStringProperty(pointElement, "precautions")
        };

        treatment.CuppingPoints.Add(cuppingPoint);
    }
}
```

#### 4.7 生活方式建议解析 (`ParseLifestyleAdviceAsync`)
```csharp
private async Task ParseLifestyleAdviceAsync(Treatment treatment, JsonElement lifestyleElement)
{
    foreach (var adviceElement in lifestyleElement.EnumerateArray())
    {
        var lifestyle = new LifestyleAdvice
        {
            TreatmentId = treatment.Id,
            Category = GetStringProperty(adviceElement, "category"),
            Title = GetStringProperty(adviceElement, "title"),
            Description = GetStringProperty(adviceElement, "description"),
            Frequency = GetStringProperty(adviceElement, "frequency"),
            Duration = GetStringProperty(adviceElement, "duration"),
            Benefits = GetStringProperty(adviceElement, "benefits"),
            Precautions = GetStringProperty(adviceElement, "precautions")
        };

        treatment.LifestyleAdvice.Add(lifestyle);
    }
}
```

#### 4.8 饮食建议解析 (`ParseDietaryAdviceAsync`)
```csharp
private async Task ParseDietaryAdviceAsync(Treatment treatment, JsonElement dietaryElement)
{
    foreach (var adviceElement in dietaryElement.EnumerateArray())
    {
        var dietary = new DietaryAdvice
        {
            TreatmentId = treatment.Id,
            Category = GetStringProperty(adviceElement, "category"),
            GeneralPrinciples = GetStringProperty(adviceElement, "general_principles"),
            RecommendedFoods = new List<RecommendedFood>(),
            AvoidedFoods = new List<AvoidedFood>()
        };

        // 解析推荐食物
        if (adviceElement.TryGetProperty("recommended_foods", out var recommendedElement))
        {
            foreach (var foodElement in recommendedElement.EnumerateArray())
            {
                var food = new RecommendedFood
                {
                    Name = GetStringProperty(foodElement, "name"),
                    Properties = GetStringProperty(foodElement, "properties"),
                    Benefits = GetStringProperty(foodElement, "benefits"),
                    Usage = GetStringProperty(foodElement, "usage")
                };
                dietary.RecommendedFoods.Add(food);
            }
        }

        // 解析禁忌食物
        if (adviceElement.TryGetProperty("avoided_foods", out var avoidedElement))
        {
            foreach (var foodElement in avoidedElement.EnumerateArray())
            {
                var food = new AvoidedFood
                {
                    Name = GetStringProperty(foodElement, "name"),
                    Reason = GetStringProperty(foodElement, "reason"),
                    Severity = GetStringProperty(foodElement, "severity")
                };
                dietary.AvoidedFoods.Add(food);
            }
        }

        treatment.DietaryAdvice.Add(dietary);
    }
}
```

#### 4.9 随访建议解析 (`ParseFollowUpAdviceAsync`)
```csharp
private async Task ParseFollowUpAdviceAsync(Treatment treatment, JsonElement followUpElement)
{
    foreach (var adviceElement in followUpElement.EnumerateArray())
    {
        var followUp = new FollowUpAdvice
        {
            TreatmentId = treatment.Id,
            TimePoint = GetStringProperty(adviceElement, "time_point"),
            Content = GetStringProperty(adviceElement, "content"),
            Purpose = GetStringProperty(adviceElement, "purpose"),
            MonitoringIndicators = new List<MonitoringIndicator>()
        };

        // 解析监测指标
        if (adviceElement.TryGetProperty("monitoring_indicators", out var indicatorsElement))
        {
            foreach (var indicatorElement in indicatorsElement.EnumerateArray())
            {
                var indicator = new MonitoringIndicator
                {
                    Name = GetStringProperty(indicatorElement, "name"),
                    NormalRange = GetStringProperty(indicatorElement, "normal_range"),
                    MonitoringMethod = GetStringProperty(indicatorElement, "monitoring_method"),
                    Frequency = GetStringProperty(indicatorElement, "frequency")
                };
                followUp.MonitoringIndicators.Add(indicator);
            }
        }

        treatment.FollowUpAdvice.Add(followUp);
    }
}
```

### 5. 工具方法 (`GetStringProperty`)

```csharp
/// <summary>
/// 安全获取JSON元素的字符串属性值
/// </summary>
/// <param name="element">JSON元素</param>
/// <param name="propertyName">属性名称</param>
/// <returns>属性值，如果不存在则返回空字符串</returns>
private static string GetStringProperty(JsonElement element, string propertyName)
{
    if (element.TryGetProperty(propertyName, out var property))
    {
        return property.ValueKind == JsonValueKind.String ? property.GetString() ?? "" : "";
    }
    return "";
}
```

**安全特性：**
- **空值处理**：防止空引用异常
- **类型检查**：确保属性值为字符串类型
- **默认值**：属性不存在时返回空字符串
- **异常安全**：不会因为JSON格式问题而抛出异常

## 数据验证体系

### 1. 输入数据验证
- **证候数据完整性检查**：验证证候实体的必需字段
- **就诊信息有效性验证**：确保就诊数据的完整性和一致性
- **患者数据关联性检查**：验证患者与就诊的关联关系
- **业务规则验证**：确保数据符合中医诊疗的业务规则

### 2. API数据验证
- **请求参数格式验证**：检查发送给Dify API的数据格式
- **字段长度和内容检查**：防止超长字段导致的API调用失败
- **必需字段存在性验证**：确保所有必需的输入字段都存在
- **数据类型一致性检查**：验证数据类型符合API要求

### 3. 响应数据验证
- **JSON格式有效性检查**：验证API响应是否为有效JSON
- **数据结构完整性验证**：检查响应数据的结构是否符合预期
- **业务数据合理性检查**：验证治疗方案数据的合理性
- **字段值范围验证**：检查数值字段是否在合理范围内

### 4. 实体数据验证
- **治疗方案内容验证**：确保生成的治疗方案内容完整
- **关联数据一致性检查**：验证主从表数据的一致性
- **业务规则符合性验证**：确保数据符合中医治疗的专业要求
- **数据完整性保护**：通过约束和验证确保数据质量

## 异常处理体系

### 1. 自定义异常类型

#### 1.1 治疗方案生成异常 (`TreatmentGenerationException`)
```csharp
/// <summary>
/// 治疗方案生成过程中的异常
/// </summary>
public class TreatmentGenerationException : Exception
{
    public int? SyndromeId { get; }
    public int? VisitId { get; }
    public string? Stage { get; }
    
    public TreatmentGenerationException(string message, int? syndromeId = null, int? visitId = null, string? stage = null) 
        : base(message)
    {
        SyndromeId = syndromeId;
        VisitId = visitId;
        Stage = stage;
    }
    
    public TreatmentGenerationException(string message, Exception innerException, int? syndromeId = null, int? visitId = null, string? stage = null) 
        : base(message, innerException)
    {
        SyndromeId = syndromeId;
        VisitId = visitId;
        Stage = stage;
    }
}
```

#### 1.2 API调用异常 (`ApiCallException`)
```csharp
/// <summary>
/// Dify API调用异常
/// </summary>
public class ApiCallException : Exception
{
    public string? ApiEndpoint { get; }
    public int? HttpStatusCode { get; }
    public string? RequestBody { get; }
    public string? ResponseBody { get; }
    
    public ApiCallException(string message, string? apiEndpoint = null, int? httpStatusCode = null) 
        : base(message)
    {
        ApiEndpoint = apiEndpoint;
        HttpStatusCode = httpStatusCode;
    }
    
    public ApiCallException(string message, Exception innerException, string? apiEndpoint = null, int? httpStatusCode = null, string? requestBody = null, string? responseBody = null) 
        : base(message, innerException)
    {
        ApiEndpoint = apiEndpoint;
        HttpStatusCode = httpStatusCode;
        RequestBody = requestBody;
        ResponseBody = responseBody;
    }
}
```

#### 1.3 API响应解析异常 (`ApiResponseParseException`)
```csharp
/// <summary>
/// API响应解析异常
/// </summary>
public class ApiResponseParseException : Exception
{
    public string? ResponseContent { get; }
    public string? ParseStage { get; }
    
    public ApiResponseParseException(string message, string? responseContent = null, string? parseStage = null) 
        : base(message)
    {
        ResponseContent = responseContent;
        ParseStage = parseStage;
    }
    
    public ApiResponseParseException(string message, Exception innerException, string? responseContent = null, string? parseStage = null) 
        : base(message, innerException)
    {
        ResponseContent = responseContent;
        ParseStage = parseStage;
    }
}
```

#### 1.4 数据验证异常 (`DataValidationException`)
```csharp
/// <summary>
/// 数据验证异常
/// </summary>
public class DataValidationException : Exception
{
    public string? FieldName { get; }
    public object? FieldValue { get; }
    public string? ValidationRule { get; }
    
    public DataValidationException(string message, string? fieldName = null, object? fieldValue = null, string? validationRule = null) 
        : base(message)
    {
        FieldName = fieldName;
        FieldValue = fieldValue;
        ValidationRule = validationRule;
    }
}
```

#### 1.5 并发控制异常 (`ConcurrencyException`)
```csharp
/// <summary>
/// 并发控制异常
/// </summary>
public class ConcurrencyException : Exception
{
    public int? SyndromeId { get; }
    public string? LockKey { get; }
    public TimeSpan? WaitTime { get; }
    
    public ConcurrencyException(string message, int? syndromeId = null, string? lockKey = null, TimeSpan? waitTime = null) 
        : base(message)
    {
        SyndromeId = syndromeId;
        LockKey = lockKey;
        WaitTime = waitTime;
    }
}
```

### 2. 异常处理策略

#### 2.1 分层异常处理
```csharp
public async Task<Treatment> GenerateAndSaveAiTreatmentAsync(int syndromeId, int visitId)
{
    try
    {
        // 业务逻辑层异常处理
        var treatment = await InternalGenerateAndSaveAiTreatmentAsync(syndromeId, visitId);
        return treatment;
    }
    catch (ApiCallException ex)
    {
        // API层异常转换为业务异常
        _logger.LogError(ex, "API调用失败: {ApiEndpoint}, 状态码: {StatusCode}", ex.ApiEndpoint, ex.HttpStatusCode);
        throw new TreatmentGenerationException($"治疗方案生成失败：API调用异常 - {ex.Message}", ex, syndromeId, visitId, "API调用");
    }
    catch (ApiResponseParseException ex)
    {
        // 解析异常转换为业务异常
        _logger.LogError(ex, "API响应解析失败: {ParseStage}", ex.ParseStage);
        throw new TreatmentGenerationException($"治疗方案生成失败：响应解析异常 - {ex.Message}", ex, syndromeId, visitId, "响应解析");
    }
    catch (DataValidationException ex)
    {
        // 数据验证异常
        _logger.LogError(ex, "数据验证失败: {FieldName} = {FieldValue}, 规则: {ValidationRule}", ex.FieldName, ex.FieldValue, ex.ValidationRule);
        throw new TreatmentGenerationException($"治疗方案生成失败：数据验证异常 - {ex.Message}", ex, syndromeId, visitId, "数据验证");
    }
    catch (ConcurrencyException ex)
    {
        // 并发控制异常
        _logger.LogWarning(ex, "并发控制异常: {LockKey}, 等待时间: {WaitTime}", ex.LockKey, ex.WaitTime);
        throw; // 直接抛出，由上层处理重试逻辑
    }
    catch (Exception ex)
    {
        // 未预期异常
        _logger.LogError(ex, "治疗方案生成过程中发生未预期异常: SyndromeId={SyndromeId}, VisitId={VisitId}", syndromeId, visitId);
        throw new TreatmentGenerationException($"治疗方案生成失败：系统异常 - {ex.Message}", ex, syndromeId, visitId, "系统异常");
    }
}
```

#### 2.2 API调用异常处理
```csharp
private async Task<string> CallDifyTreatmentApiAsync(Dictionary<string, object> inputs)
{
    var requestUrl = $"{_difyApiOptions.BaseUrl}/workflows/run";
    var requestBody = JsonSerializer.Serialize(new { inputs });
    
    try
    {
        return await _retryPolicyService.ExecuteAsync(async () =>
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(_difyApiOptions.TimeoutSeconds);
                
                var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl) { Content = content };
                request.Headers.Add("Authorization", $"Bearer {_difyApiOptions.TreatmentWorkflowApiKey}");
                
                var response = await httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new ApiCallException(
                        $"API调用失败: {response.StatusCode} - {response.ReasonPhrase}",
                        apiEndpoint: requestUrl,
                        httpStatusCode: (int)response.StatusCode,
                        requestBody: requestBody,
                        responseBody: responseContent
                    );
                }
                
                return responseContent;
            }
            catch (HttpRequestException ex)
            {
                throw new ApiCallException("HTTP请求异常", ex, requestUrl, requestBody: requestBody);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new ApiCallException("API调用超时", ex, requestUrl, requestBody: requestBody);
            }
        });
    }
    catch (ApiCallException)
    {
        throw; // 直接抛出API异常
    }
    catch (Exception ex)
    {
        throw new ApiCallException("API调用过程中发生未预期异常", ex, requestUrl, requestBody: requestBody);
    }
}
```

#### 2.3 数据解析异常处理
```csharp
private async Task UpdateTreatmentFromApiResponseAsync(Treatment treatment, string apiResponse)
{
    try
    {
        var jsonDocument = JsonDocument.Parse(apiResponse);
        var root = jsonDocument.RootElement;
        
        // 验证响应结构
        if (!root.TryGetProperty("data", out var dataElement))
            throw new ApiResponseParseException("API响应缺少data字段", apiResponse, "结构验证");
            
        if (!dataElement.TryGetProperty("outputs", out var outputsElement))
            throw new ApiResponseParseException("API响应缺少outputs字段", apiResponse, "结构验证");
            
        if (!outputsElement.TryGetProperty("treatment_plan", out var treatmentElement))
            throw new ApiResponseParseException("API响应缺少treatment_plan字段", apiResponse, "结构验证");
        
        // 更新基本信息
        treatment.GeneratedContent = treatmentElement.GetRawText();
        treatment.ApiResponseRaw = apiResponse;
        
        // 解析治疗数据
        await ParseAndUpdateTreatmentDataAsync(treatment, treatmentElement);
    }
    catch (JsonException ex)
    {
        throw new ApiResponseParseException("JSON格式无效", ex, apiResponse, "JSON解析");
    }
    catch (ApiResponseParseException)
    {
        throw; // 直接抛出解析异常
    }
    catch (Exception ex)
    {
        throw new ApiResponseParseException("响应解析过程中发生未预期异常", ex, apiResponse, "未知阶段");
    }
}
```

### 3. 错误恢复机制

#### 3.1 重试机制
- **智能重试**：通过 `RetryPolicyService` 实现指数退避重试
- **重试条件**：仅对临时性错误（网络异常、超时、5xx错误）进行重试
- **重试限制**：最大重试3次，避免无限重试
- **重试间隔**：基础延迟1000ms，每次重试延迟时间翻倍

#### 3.2 资源清理机制
```csharp
private async Task<Treatment> InternalGenerateAndSaveAiTreatmentAsync(int syndromeId, int visitId)
{
    Treatment? placeholderTreatment = null;
    
    try
    {
        // 创建占位符治疗方案
        placeholderTreatment = await CreatePlaceholderTreatmentAsync(syndromeId, visitId);
        
        // 生成治疗方案
        var apiResponse = await CallDifyTreatmentApiAsync(inputs);
        await UpdateTreatmentFromApiResponseAsync(placeholderTreatment, apiResponse);
        
        // 更新状态为成功
        placeholderTreatment.Status = TreatmentStatusEnum.Generated;
        placeholderTreatment.GeneratedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        return placeholderTreatment;
    }
    catch (Exception)
    {
        // 异常时清理占位符记录
        if (placeholderTreatment != null)
        {
            try
            {
                _context.Treatments.Remove(placeholderTreatment);
                await _context.SaveChangesAsync();
                _logger.LogInformation("已清理占位符治疗方案记录: TreatmentId={TreatmentId}", placeholderTreatment.Id);
            }
            catch (Exception cleanupEx)
            {
                _logger.LogError(cleanupEx, "清理占位符治疗方案记录失败: TreatmentId={TreatmentId}", placeholderTreatment.Id);
            }
        }
        
        throw; // 重新抛出原始异常
    }
}
```

#### 3.3 事务回滚机制
- **数据库事务**：使用Entity Framework的事务机制确保数据一致性
- **自动回滚**：异常发生时自动回滚未提交的数据变更
- **部分成功处理**：在数据解析阶段，保存成功解析的部分，记录失败的模块

#### 3.4 状态恢复机制
```csharp
private async Task<Treatment> CreatePlaceholderTreatmentAsync(int syndromeId, int visitId)
{
    var treatment = new Treatment
    {
        SyndromeId = syndromeId,
        VisitId = visitId,
        Status = TreatmentStatusEnum.Generating, // 生成中状态
        CreatedAt = DateTime.UtcNow,
        GeneratedContent = "", // 占位符内容
        // 初始化所有集合属性
        Prescriptions = new List<Prescription>(),
        HerbalWarnings = new List<HerbalWarning>(),
        DietaryTherapies = new List<DietaryTherapy>(),
        AcupuncturePoints = new List<AcupuncturePoint>(),
        MoxibustionPoints = new List<MoxibustionPoint>(),
        CuppingPoints = new List<CuppingPoint>(),
        LifestyleAdvice = new List<LifestyleAdvice>(),
        DietaryAdvice = new List<DietaryAdvice>(),
        FollowUpAdvice = new List<FollowUpAdvice>()
    };
    
    _context.Treatments.Add(treatment);
    await _context.SaveChangesAsync();
    
    return treatment;
}
```

### 4. 异常监控和日志记录

#### 4.1 结构化日志记录
```csharp
// 业务操作日志
_logger.LogInformation("开始生成治疗方案: SyndromeId={SyndromeId}, VisitId={VisitId}", syndromeId, visitId);

// 性能监控日志
_logger.LogInformation("治疗方案生成完成: SyndromeId={SyndromeId}, 耗时={ElapsedMs}ms", syndromeId, stopwatch.ElapsedMilliseconds);

// 异常详情日志
_logger.LogError(ex, "API调用失败: {ApiEndpoint}, 状态码: {StatusCode}, 请求体: {RequestBody}", 
    ex.ApiEndpoint, ex.HttpStatusCode, ex.RequestBody);

// 业务指标日志
_logger.LogWarning("检测到重复生成请求: SyndromeId={SyndromeId}, 现有状态={Status}", syndromeId, existingTreatment.Status);
```

#### 4.2 关键指标监控
- **成功率监控**：记录治疗方案生成的成功率
- **性能监控**：记录各阶段的执行时间
- **错误分类统计**：统计不同类型异常的发生频率
- **API调用监控**：监控Dify API的调用成功率和响应时间

## API集成机制

### 1. 重试策略

```csharp
// 重试配置
maxRetries: 3              // 最大重试次数
baseDelayMs: 1000         // 基础延迟时间
指数退避算法               // 延迟时间递增
```

### 2. 超时控制

```csharp
// 超时配置
TimeoutSeconds: 60        // 请求超时时间
连接超时控制              // HTTP连接超时
读取超时控制              // 响应读取超时
```

### 3. 错误分类处理

- **可重试错误**：网络错误、服务器错误、超时
- **不可重试错误**：认证错误、参数错误、权限错误
- **业务错误**：数据格式错误、业务规则违反

## 数据解析机制

### 1. JSON Schema 映射

根据 `输入输出.md` 文档定义的JSON Schema，实现九大治疗模块的数据解析：

- **中药处方** (`herbal_prescriptions`)
- **中药安全警告** (`herbal_warnings`)
- **食疗方案** (`dietary_therapies`)
- **针灸穴位** (`acupuncture_points`)
- **艾灸穴位** (`moxibustion_points`)
- **拔罐部位** (`cupping_points`)
- **生活方式建议** (`lifestyle_advice`)
- **饮食建议** (`dietary_advice`)
- **随访建议** (`follow_up_advice`)

### 2. 数据转换规则

- **字段映射**：支持驼峰和下划线命名
- **类型转换**：自动处理字符串和数值类型
- **默认值处理**：为缺失字段提供合理默认值
- **关联数据**：正确处理主从表关系

## 配置管理

### 1. Dify API 配置

```json
{
  "DifyApi": {
    "BaseUrl": "http://192.168.88.253:18001",
    "TreatmentWorkflowApiKey": "app-TreatmentWorkflowDefault123",
    "TreatmentEndpoint": "/v1/workflows/run",
    "ResponseMode": "blocking",
    "User": "tcm-diagnosis-system",
    "TimeoutSeconds": 60
  }
}
```

### 2. 配置验证

- 启动时验证配置完整性
- 运行时检查配置有效性
- 配置变更时自动重新验证

## 性能优化

### 1. 数据库优化

- **条件唯一索引**：`Treatments` 表的 `SyndromeId` 字段
- **查询优化**：使用 `Include` 预加载关联数据
- **连接池管理**：合理配置数据库连接池

### 2. 内存优化

- **对象生命周期管理**：及时释放不需要的对象
- **大对象处理**：分批处理大量数据
- **缓存策略**：合理使用内存缓存

### 3. 网络优化

- **连接复用**：使用 `HttpClientFactory`
- **压缩传输**：启用HTTP压缩
- **并发控制**：限制同时进行的API调用数量

## 监控和日志

### 1. 日志级别

- **Information**：正常业务流程记录
- **Warning**：可恢复的错误和异常情况
- **Error**：严重错误和异常，需要人工干预

### 2. 关键指标监控

- **API调用成功率**：监控Dify API调用成功率
- **响应时间**：监控API调用和数据库操作耗时
- **并发锁竞争**：监控锁竞争频率和持续时间
- **错误率**：监控各类异常的发生频率

### 3. 业务指标监控

- **治疗方案生成数量**：统计每日生成的方案数量
- **生成成功率**：监控方案生成的成功率
- **平均生成时间**：监控从开始到完成的平均时间

## 使用示例

### 1. 依赖注入配置

```csharp
// Program.cs - 服务注册
var builder = WebApplication.CreateBuilder(args);

// 注册数据库上下文
builder.Services.AddDbContext<TcmAiDiagnosisContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 注册AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// 注册HTTP客户端工厂
builder.Services.AddHttpClient();

// 注册配置选项
builder.Services.Configure<DifyApiOptions>(builder.Configuration.GetSection("DifyApi"));

// 注册日志服务
builder.Services.AddLogging();

// 注册业务领域服务
builder.Services.AddScoped<TreatmentDomain>();
builder.Services.AddScoped<SyndromeDomain>();
builder.Services.AddScoped<VisitDomain>();

// 注册数据验证服务
builder.Services.AddScoped<TreatmentDataValidator>();

// 注册重试策略服务
builder.Services.AddScoped<RetryPolicyService>();

var app = builder.Build();
```

### 2. 基本使用示例

#### 2.1 检查现有治疗方案
```csharp
public class TreatmentController : Controller
{
    private readonly TreatmentDomain _treatmentDomain;
    private readonly ILogger<TreatmentController> _logger;
    
    public TreatmentController(TreatmentDomain treatmentDomain, ILogger<TreatmentController> logger)
    {
        _treatmentDomain = treatmentDomain;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetTreatment(int syndromeId)
    {
        try
        {
            // 检查是否已有治疗方案
            var existingTreatment = await _treatmentDomain.GetLatestTreatmentBySyndromeIdAsync(syndromeId);
            
            if (existingTreatment != null)
            {
                return Json(new { 
                    success = true, 
                    treatment = existingTreatment,
                    message = "治疗方案已存在" 
                });
            }
            
            return Json(new { 
                success = false, 
                message = "暂无治疗方案，请先生成" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取治疗方案失败: SyndromeId={SyndromeId}", syndromeId);
            return Json(new { 
                success = false, 
                message = "获取治疗方案失败" 
            });
        }
    }
}
```

#### 2.2 异步生成治疗方案
```csharp
[HttpPost]
public async Task<IActionResult> GenerateTreatment(int syndromeId, int visitId)
{
    try
    {
        // 检查是否已有治疗方案
        var existingTreatment = await _treatmentDomain.GetLatestTreatmentBySyndromeIdAsync(syndromeId);
        
        if (existingTreatment != null)
        {
            return Json(new { 
                success = true, 
                treatment = existingTreatment,
                message = "治疗方案已存在" 
            });
        }
        
        // 异步生成治疗方案（不等待完成）
        _ = Task.Run(async () =>
        {
            try
            {
                await _treatmentDomain.GenerateAndSaveAiTreatmentAsync(syndromeId, visitId);
                _logger.LogInformation("治疗方案生成任务已启动: SyndromeId={SyndromeId}, VisitId={VisitId}", syndromeId, visitId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "后台治疗方案生成失败: SyndromeId={SyndromeId}, VisitId={VisitId}", syndromeId, visitId);
            }
        });
        
        return Json(new { 
            success = true, 
            message = "治疗方案生成任务已启动，请稍后查询结果" 
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "启动治疗方案生成任务失败: SyndromeId={SyndromeId}, VisitId={VisitId}", syndromeId, visitId);
        return Json(new { 
            success = false, 
            message = "启动治疗方案生成任务失败" 
        });
    }
}
```

#### 2.3 同步生成治疗方案（适用于测试环境）
```csharp
[HttpPost]
public async Task<IActionResult> GenerateTreatmentSync(int syndromeId, int visitId)
{
    try
    {
        var treatment = await _treatmentDomain.GenerateAndSaveAiTreatmentAsync(syndromeId, visitId);
        
        return Json(new { 
            success = true, 
            treatment = treatment,
            message = "治疗方案生成成功" 
        });
    }
    catch (TreatmentGenerationException ex)
    {
        _logger.LogError(ex, "治疗方案生成失败: {Stage}, SyndromeId={SyndromeId}, VisitId={VisitId}", 
            ex.Stage, ex.SyndromeId, ex.VisitId);
        
        return Json(new { 
            success = false, 
            message = $"治疗方案生成失败: {ex.Message}",
            stage = ex.Stage
        });
    }
    catch (ConcurrencyException ex)
    {
        _logger.LogWarning(ex, "并发生成冲突: SyndromeId={SyndromeId}, LockKey={LockKey}", 
            ex.SyndromeId, ex.LockKey);
        
        return Json(new { 
            success = false, 
            message = "当前证候正在生成治疗方案，请稍后再试",
            retryAfter = 30 // 建议30秒后重试
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "治疗方案生成过程中发生未预期异常: SyndromeId={SyndromeId}, VisitId={VisitId}", 
            syndromeId, visitId);
        
        return Json(new { 
            success = false, 
            message = "系统异常，请联系管理员" 
        });
    }
}
```

### 3. 高级使用示例

#### 3.1 批量生成治疗方案
```csharp
public async Task<List<Treatment>> BatchGenerateTreatments(List<(int syndromeId, int visitId)> requests)
{
    var results = new List<Treatment>();
    var semaphore = new SemaphoreSlim(3, 3); // 限制并发数为3
    
    var tasks = requests.Select(async request =>
    {
        await semaphore.WaitAsync();
        try
        {
            var treatment = await _treatmentDomain.GenerateAndSaveAiTreatmentAsync(request.syndromeId, request.visitId);
            return treatment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量生成治疗方案失败: SyndromeId={SyndromeId}, VisitId={VisitId}", 
                request.syndromeId, request.visitId);
            return null;
        }
        finally
        {
            semaphore.Release();
        }
    });
    
    var treatments = await Task.WhenAll(tasks);
    return treatments.Where(t => t != null).ToList();
}
```

#### 3.2 带重试机制的生成
```csharp
public async Task<Treatment?> GenerateTreatmentWithRetry(int syndromeId, int visitId, int maxRetries = 3)
{
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            var treatment = await _treatmentDomain.GenerateAndSaveAiTreatmentAsync(syndromeId, visitId);
            _logger.LogInformation("治疗方案生成成功: 尝试次数={Attempt}, SyndromeId={SyndromeId}", 
                attempt, syndromeId);
            return treatment;
        }
        catch (ConcurrencyException ex)
        {
            _logger.LogWarning("并发冲突，尝试次数={Attempt}: {Message}", attempt, ex.Message);
            
            if (attempt < maxRetries)
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt)); // 指数退避
                await Task.Delay(delay);
                continue;
            }
            
            throw;
        }
        catch (TreatmentGenerationException ex) when (ex.Stage == "API调用")
        {
            _logger.LogWarning("API调用失败，尝试次数={Attempt}: {Message}", attempt, ex.Message);
            
            if (attempt < maxRetries)
            {
                var delay = TimeSpan.FromSeconds(5 * attempt); // 线性退避
                await Task.Delay(delay);
                continue;
            }
            
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "治疗方案生成失败，尝试次数={Attempt}", attempt);
            throw;
        }
    }
    
    return null;
}
```

### 4. 错误处理最佳实践

#### 4.1 分类错误处理
```csharp
public async Task<ApiResult<Treatment>> SafeGenerateTreatment(int syndromeId, int visitId)
{
    try
    {
        var treatment = await _treatmentDomain.GenerateAndSaveAiTreatmentAsync(syndromeId, visitId);
        return ApiResult<Treatment>.Success(treatment, "治疗方案生成成功");
    }
    catch (TreatmentGenerationException ex)
    {
        // 业务异常 - 用户可理解的错误
        return ApiResult<Treatment>.Failure($"治疗方案生成失败: {ex.Message}", "TREATMENT_GENERATION_FAILED");
    }
    catch (ConcurrencyException ex)
    {
        // 并发异常 - 建议重试
        return ApiResult<Treatment>.Failure("当前证候正在生成治疗方案，请稍后再试", "CONCURRENT_GENERATION", retryAfter: 30);
    }
    catch (ApiCallException ex)
    {
        // API异常 - 系统问题
        _logger.LogError(ex, "API调用异常: {ApiEndpoint}, 状态码: {StatusCode}", ex.ApiEndpoint, ex.HttpStatusCode);
        return ApiResult<Treatment>.Failure("AI服务暂时不可用，请稍后再试", "API_UNAVAILABLE");
    }
    catch (Exception ex)
    {
        // 未知异常 - 系统错误
        _logger.LogError(ex, "治疗方案生成过程中发生未预期异常");
        return ApiResult<Treatment>.Failure("系统异常，请联系管理员", "SYSTEM_ERROR");
    }
}

public class ApiResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = "";
    public string? ErrorCode { get; set; }
    public int? RetryAfter { get; set; }
    
    public static ApiResult<T> Success(T data, string message = "操作成功")
    {
        return new ApiResult<T> { Success = true, Data = data, Message = message };
    }
    
    public static ApiResult<T> Failure(string message, string? errorCode = null, int? retryAfter = null)
    {
        return new ApiResult<T> 
        { 
            Success = false, 
            Message = message, 
            ErrorCode = errorCode,
            RetryAfter = retryAfter
        };
    }
}
```

### 5. 监控和日志记录

#### 5.1 性能监控
```csharp
public async Task<Treatment> GenerateTreatmentWithMonitoring(int syndromeId, int visitId)
{
    var stopwatch = Stopwatch.StartNew();
    var operationId = Guid.NewGuid().ToString("N")[..8];
    
    _logger.LogInformation("开始生成治疗方案: OperationId={OperationId}, SyndromeId={SyndromeId}, VisitId={VisitId}", 
        operationId, syndromeId, visitId);
    
    try
    {
        var treatment = await _treatmentDomain.GenerateAndSaveAiTreatmentAsync(syndromeId, visitId);
        
        stopwatch.Stop();
        _logger.LogInformation("治疗方案生成成功: OperationId={OperationId}, 耗时={ElapsedMs}ms, TreatmentId={TreatmentId}", 
            operationId, stopwatch.ElapsedMilliseconds, treatment.Id);
        
        // 记录业务指标
        RecordBusinessMetrics("treatment_generation_success", stopwatch.ElapsedMilliseconds);
        
        return treatment;
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        _logger.LogError(ex, "治疗方案生成失败: OperationId={OperationId}, 耗时={ElapsedMs}ms", 
            operationId, stopwatch.ElapsedMilliseconds);
        
        // 记录失败指标
        RecordBusinessMetrics("treatment_generation_failure", stopwatch.ElapsedMilliseconds, ex.GetType().Name);
        
        throw;
    }
}

private void RecordBusinessMetrics(string eventName, long elapsedMs, string? errorType = null)
{
    // 这里可以集成到监控系统，如Prometheus、Application Insights等
    _logger.LogInformation("业务指标: Event={EventName}, Duration={Duration}ms, ErrorType={ErrorType}", 
        eventName, elapsedMs, errorType);
}
```

### 6. 配置管理示例

#### 6.1 appsettings.json 配置
```json
{
  "DifyApi": {
    "BaseUrl": "https://api.dify.ai/v1",
    "TreatmentWorkflowApiKey": "your-api-key-here",
    "TimeoutSeconds": 120,
    "ResponseMode": "blocking",
    "User": "tcm-diagnosis-system",
    "MaxRetries": 3,
    "BaseDelayMs": 1000
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "TcmAiDiagnosis.Domain.TreatmentDomain": "Debug"
    }
  }
}
```

#### 6.2 配置验证
```csharp
public class DifyApiOptionsValidator : IValidateOptions<DifyApiOptions>
{
    public ValidateOptionsResult Validate(string name, DifyApiOptions options)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            errors.Add("DifyApi:BaseUrl 不能为空");
            
        if (string.IsNullOrWhiteSpace(options.TreatmentWorkflowApiKey))
            errors.Add("DifyApi:TreatmentWorkflowApiKey 不能为空");
            
        if (options.TimeoutSeconds <= 0)
            errors.Add("DifyApi:TimeoutSeconds 必须大于0");
        
        return errors.Any() 
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}

// 在Program.cs中注册验证器
builder.Services.AddSingleton<IValidateOptions<DifyApiOptions>, DifyApiOptionsValidator>();
```

## 最佳实践

### 1. 错误处理
- 始终使用 try-catch 包装业务逻辑
- 根据异常类型采取不同的处理策略
- 记录详细的错误信息和上下文

### 2. 性能优化
- 避免在循环中进行数据库操作
- 使用异步方法处理I/O密集型操作
- 合理设置超时时间和重试次数

### 3. 数据一致性
- 使用事务确保数据一致性
- 验证数据完整性和业务规则
- 实现幂等性，支持安全重试

### 4. 监控和维护
- 定期检查日志和监控指标
- 及时处理异常和性能问题
- 保持配置和依赖的更新

## 总结

`TreatmentDomain` 类实现了一个完整、可靠、高性能的智能治疗方案生成系统。通过合理的架构设计、完善的错误处理、严格的数据验证和可靠的API集成，确保了系统的稳定性和可维护性。

该实现严格遵循了开发计划文档的要求，实现了所有核心功能，并在此基础上增加了更多的安全性和可靠性保障。