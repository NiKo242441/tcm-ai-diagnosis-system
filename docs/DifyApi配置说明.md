# Dify工作流API配置说明

## 概述

本项目已将Dify API调用从硬编码方式迁移到配置文件管理，现在支持Dify工作流API。通过配置文件可以灵活管理API端点地址、密钥、工作流ID等参数，提高了系统的可维护性和安全性。

## 配置文件位置

- **生产环境配置**: `TcmAiDiagnosis.Web/appsettings.json`
- **开发环境配置**: `TcmAiDiagnosis.Web/appsettings.Development.json`

## 配置结构

### JSON配置格式

```json
{
  "DifyApi": {
    "BaseUrl": "https://api.dify.ai/v1",
    "ApiKey": "YOUR_DIFY_API_KEY",
    "OverviewWorkflowApiKey": "YOUR_OVERVIEW_WORKFLOW_API_KEY",
    "DetailWorkflowApiKey": "YOUR_DETAIL_WORKFLOW_API_KEY",
    "Endpoint": "/workflows/run",
    "ResponseMode": "blocking",
    "User": "tcm-diagnosis-system",
    "TimeoutSeconds": 30
  }
}
```

### 参数说明

| 参数名 | 类型 | 必填 | 说明 | 示例值 |
|--------|------|------|------|--------|
| `BaseUrl` | string | 是 | Dify API基础URL | `https://api.dify.ai/v1` |
| `ApiKey` | string | 是 | Dify基础API密钥（用于通用API访问） | `app-xxxxxxxxxxxxxxxx` |
| `OverviewWorkflowApiKey` | string | 是 | 证候概览工作流专用API密钥 | `app-overview-xxxxxxxx` |
| `DetailWorkflowApiKey` | string | 是 | 证候详情工作流专用API密钥 | `app-detail-xxxxxxxx` |
| `Endpoint` | string | 是 | API端点 | `/workflows/run` |
| `ResponseMode` | string | 是 | 响应模式 | `blocking` 或 `streaming` |
| `User` | string | 是 | 用户标识 | `tcm-diagnosis-system` |
| `TimeoutSeconds` | int | 是 | 请求超时时间（秒） | `30` |

## 环境配置示例

### 生产环境 (appsettings.json)

```json
{
  "DifyApi": {
    "BaseUrl": "https://api.dify.ai/v1",
    "ApiKey": "app-prod-your-api-key-here",
    "OverviewWorkflowApiKey": "app-prod-overview-workflow-key",
    "DetailWorkflowApiKey": "app-prod-detail-workflow-key",
    "OverviewEndpoint": "/workflows/run",
    "ResponseMode": "blocking",
    "User": "tcm-diagnosis-prod",
    "TimeoutSeconds": 30
  }
}
```

### 开发环境 (appsettings.Development.json)

```json
{
  "DifyApi": {
    "BaseUrl": "http://localhost:8000",
    "ApiKey": "app-dev-your-api-key-here",
    "OverviewWorkflowApiKey": "app-dev-overview-workflow-key",
    "DetailWorkflowApiKey": "app-dev-detail-workflow-key",
    "OverviewEndpoint": "/v1/workflows/run",
    "ResponseMode": "blocking",
    "User": "tcm-diagnosis-dev",
    "TimeoutSeconds": 60
  }
}
```

## 配置步骤

1. **获取工作流信息**
   - 登录Dify控制台
   - 创建或找到证候概览和详情工作流
   - 记录每个工作流的专用API密钥

2. **更新配置文件**
   - 修改对应环境的配置文件
   - 替换`YOUR_DIFY_API_KEY`为实际的基础API密钥
   - 替换`YOUR_OVERVIEW_WORKFLOW_API_KEY`为概览工作流的API密钥
   - 替换`YOUR_DETAIL_WORKFLOW_API_KEY`为详情工作流的API密钥

3. **验证配置**
   - 重启应用程序
   - 测试证候诊断功能

## 环境变量支持

系统支持通过环境变量覆盖配置文件设置：

```bash
# 设置基础API密钥
export DifyApi__ApiKey="your-api-key"

# 设置工作流专用API密钥
export DifyApi__OverviewWorkflowApiKey="your-overview-workflow-api-key"
export DifyApi__DetailWorkflowApiKey="your-detail-workflow-api-key"

# 设置基础URL
export DifyApi__BaseUrl="https://your-dify-instance.com/v1"

# 设置端点
export DifyApi__Endpoint="/workflows/run"
```

## 安全建议

1. **API密钥管理**
   - 不要将真实的API密钥提交到版本控制系统
   - 使用环境变量或密钥管理服务存储敏感信息
   - 定期轮换API密钥

2. **网络安全**
   - 在生产环境中使用HTTPS
   - 配置适当的防火墙规则
   - 考虑使用API网关进行额外的安全控制

## 配置验证

系统在启动时会自动验证配置的完整性：

- 检查必填字段是否存在
- 验证URL格式是否正确
- 确认超时时间是否合理

如果配置验证失败，应用程序将记录错误信息并可能拒绝启动。

## 工作流API请求格式

### 请求结构

```json
{
  "inputs": {
    "symptoms": "患者症状描述",
    "age": "患者年龄",
    "gender": "患者性别"
  },
  "response_mode": "blocking",
  "user": "tcm-diagnosis-system"
}
```

### 响应结构

#### 成功响应

```json
{
  "workflow_run_id": "run-id",
  "task_id": "task-id",
  "data": {
    "id": "data-id",
    "workflow_id": "workflow-id",
    "status": "succeeded",
    "outputs": {
      "result": "工作流输出结果"
    },
    "error": null,
    "elapsed_time": 1.234,
    "total_tokens": 100,
    "total_steps": 3,
    "created_at": 1234567890,
    "finished_at": 1234567891
  }
}
```

#### 错误响应

```json
{
  "code": "workflow_request_error",
  "message": "错误描述",
  "status": 400
}
```

## 代码实现

### 配置类 (DifyApiOptions.cs)

```csharp
public class DifyApiOptions
{
    public const string SectionName = "DifyApi";
    
    public string BaseUrl { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string OverviewWorkflowApiKey { get; set; } = "";
    public string DetailWorkflowApiKey { get; set; } = "";
    public string OverviewEndpoint { get; set; } = "/workflows/run";
    public string DetailEndpoint { get; set; } = "/workflows/run";
    public string ResponseMode { get; set; } = "blocking";
    public string User { get; set; } = "doctor";
    public int TimeoutSeconds { get; set; } = 30;
    
    // 构建完整URL的方法
    public string OverviewUrl => $"{BaseUrl.TrimEnd('/')}{OverviewEndpoint}";
    public string DetailUrl => $"{BaseUrl.TrimEnd('/')}{DetailEndpoint}";
    
    // 验证配置完整性
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(BaseUrl) &&
               !string.IsNullOrWhiteSpace(ApiKey) &&
               !string.IsNullOrWhiteSpace(OverviewWorkflowApiKey) &&
               !string.IsNullOrWhiteSpace(DetailWorkflowApiKey) &&
               !string.IsNullOrWhiteSpace(OverviewEndpoint) &&
               !string.IsNullOrWhiteSpace(DetailEndpoint) &&
               TimeoutSeconds > 0 &&
               (ResponseMode == "blocking" || ResponseMode == "streaming") &&
               !string.IsNullOrWhiteSpace(User);
    }
}
```

### 服务注册 (Program.cs)

```csharp
// 注册Dify API配置
builder.Services.Configure<DifyApiOptions>(
    builder.Configuration.GetSection(DifyApiOptions.SectionName));
```

### 使用示例 (SyndromeDomain.cs)

```csharp
public class SyndromeDomain
{
    private readonly HttpClient _httpClient;
    private readonly DifyApiOptions _difyApiOptions;
    
    public SyndromeDomain(HttpClient httpClient, IOptions<DifyApiOptions> difyApiOptions)
    {
        _httpClient = httpClient;
        _difyApiOptions = difyApiOptions.Value;
        
        // 验证配置
        if (!_difyApiOptions.IsValid())
        {
            throw new InvalidOperationException("Dify API配置无效");
        }
        
        // 设置超时时间
        _httpClient.Timeout = TimeSpan.FromSeconds(_difyApiOptions.TimeoutSeconds);
    }
    
    public async Task<List<SyndromeOverviewDto>> GetSyndromeOverviewAsync(string patientDescription, string visitDescription, string userPhoneNumber = null)
    {
        var requestData = new DifyWorkflowRequest
        {
            Inputs = new Dictionary<string, object>
            {
                ["patient_info"] = patientDescription,
                ["visit_info"] = visitDescription
            },
            ResponseMode = _difyApiOptions.ResponseMode,
            User = string.IsNullOrEmpty(userPhoneNumber) ? _difyApiOptions.User : userPhoneNumber
        };

        var json = JsonSerializer.Serialize(requestData, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // 设置请求头 - 使用概览工作流专用API密钥
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_difyApiOptions.OverviewWorkflowApiKey}");

        var response = await _httpClient.PostAsync(_difyApiOptions.OverviewUrl, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var apiResponse = JsonSerializer.Deserialize<DifyWorkflowResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (apiResponse?.Data?.Status == "succeeded" && apiResponse.Data.Outputs != null)
            {
                return ParseSyndromeOverview(apiResponse.Data.Outputs);
            }
            else
            {
                throw new Exception($"Dify工作流执行失败: {apiResponse?.Data?.Error ?? "未知错误"}");
            }
        }
        else
        {
            throw new Exception($"Dify API调用失败 [HTTP {response.StatusCode}]: {responseContent}");
        }
    }
    
    public async Task<SyndromeDetailDto> GetSyndromeDetailAsync(string syndromeId, string patientDescription, string userPhoneNumber = null)
    {
        var requestData = new DifyWorkflowRequest
        {
            Inputs = new Dictionary<string, object>
            {
                ["syndrome_id"] = syndromeId,
                ["patient_info"] = patientDescription
            },
            ResponseMode = _difyApiOptions.ResponseMode,
            User = string.IsNullOrEmpty(userPhoneNumber) ? _difyApiOptions.User : userPhoneNumber
        };

        var json = JsonSerializer.Serialize(requestData, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // 设置请求头 - 使用详情工作流专用API密钥
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_difyApiOptions.DetailWorkflowApiKey}");

        var response = await _httpClient.PostAsync(_difyApiOptions.DetailUrl, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var apiResponse = JsonSerializer.Deserialize<DifyWorkflowResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (apiResponse?.Data?.Status == "succeeded" && apiResponse.Data.Outputs != null)
            {
                return ParseSyndromeDetail(apiResponse.Data.Outputs);
            }
            else
            {
                throw new Exception($"Dify工作流执行失败: {apiResponse?.Data?.Error ?? "未知错误"}");
            }
        }
        else
        {
            throw new Exception($"Dify API调用失败 [HTTP {response.StatusCode}]: {responseContent}");
        }
    }
}
```

## 故障排除

### 常见问题

1. **配置验证失败**
   - 检查所有必填字段是否已填写
   - 确认URL格式正确
   - 验证工作流API密钥是否有效

2. **API调用失败**
   - 检查网络连接
   - 验证工作流专用API密钥是否正确
   - 确认工作流是否已发布并可访问

3. **超时错误**
   - 增加`TimeoutSeconds`配置值
   - 检查网络延迟
   - 优化工作流执行时间

### 日志信息

系统会记录以下关键信息：

- 配置加载状态
- API请求和响应
- 错误详情和堆栈跟踪

查看日志以获取详细的故障排除信息。

## 版本更新说明

### v3.0 - 独立工作流API密钥支持

- 移除工作流ID配置，改用独立的工作流API密钥
- 为概览和详情工作流分别配置专用API密钥
- 简化API调用流程，直接使用工作流端点
- 增强安全性，每个工作流使用独立的访问凭证

### v2.0 - 工作流API支持

- 迁移到Dify工作流API
- 支持工作流ID配置
- 增强的错误处理和响应解析
- 支持阻塞和流式响应模式

### v1.0 - 基础配置化

- 从硬编码迁移到配置文件
- 支持环境变量覆盖
- 基础的配置验证