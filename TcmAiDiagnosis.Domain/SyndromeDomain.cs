using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TcmAiDiagnosis.EFContext;
using TcmAiDiagnosis.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace TcmAiDiagnosis.Domain
{
    /// <summary>
    /// 证候诊断业务逻辑
    /// </summary>
    public class SyndromeDomain
    {
        private readonly TcmAiDiagnosisContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DifyApiOptions _difyApiOptions;
        private readonly IServiceProvider _serviceProvider;

        public SyndromeDomain(TcmAiDiagnosisContext context, IHttpClientFactory httpClientFactory, IOptions<DifyApiOptions> difyApiOptions, IServiceProvider serviceProvider)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _difyApiOptions = difyApiOptions.Value;
            _serviceProvider = serviceProvider;

            // 验证配置
            if (!_difyApiOptions.IsValid())
            {
                throw new InvalidOperationException("Dify API 配置无效，请检查 appsettings.json 中的 DifyApi 配置节");
            }
        }

        /// <summary>
        /// 调用Dify API获取证候概览
        /// </summary>
        /// <param name="patientDescription">患者描述</param>
        /// <param name="visitDescription">问诊描述</param>
        /// <param name="userPhoneNumber">当前登录用户的手机号</param>
        /// <returns>证候概览列表</returns>
        public async Task<List<SyndromeOverviewDto>> GetSyndromeOverviewAsync(string patientDescription, string visitDescription, string userPhoneNumber = null)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            
            try
            {
                // 配置 HttpClient 超时
                httpClient.Timeout = TimeSpan.FromSeconds(_difyApiOptions.TimeoutSeconds);
                
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

                // 设置请求头
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_difyApiOptions.OverviewWorkflowApiKey}");

                var response = await httpClient.PostAsync(_difyApiOptions.OverviewUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<DifyWorkflowResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    if (apiResponse?.Data?.Status == "succeeded" && apiResponse.Data.Outputs != null)
                    {
                        // 解析API返回的证候数据
                        return ParseSyndromeOverview(apiResponse.Data.Outputs);
                    }
                    else if (apiResponse?.Data?.Status == "failed")
                    {
                        throw new Exception($"Dify工作流执行失败: {apiResponse.Data.Error ?? "未知错误"}");
                    }
                    else
                    {
                        throw new Exception($"Dify工作流状态异常: {apiResponse?.Data?.Status ?? "未知状态"}");
                    }
                }
                else
                {
                    // 尝试解析错误响应
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<DifyErrorResponse>(responseContent);
                        throw new Exception($"Dify API调用失败 [{errorResponse?.Code}]: {errorResponse?.Message ?? responseContent}");
                    }
                    catch (JsonException)
                    {
                        throw new Exception($"Dify API调用失败 [HTTP {response.StatusCode}]: {responseContent}");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new Exception($"请求超时: {ex.Message}", ex);
            }
            catch (Exception ex) when (!(ex is Exception))
            {
                throw new Exception($"获取证候概览失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 调用Dify API获取证候详情
        /// </summary>
        /// <param name="syndromeName">证候名称</param>
        /// <param name="patientDescription">患者描述</param>
        /// <param name="visitDescription">问诊描述</param>
        /// <param name="userPhoneNumber">当前登录用户的手机号</param>
        /// <returns>证候详情</returns>
        public async Task<SyndromeDetailDto> GetSyndromeDetailAsync(string syndromeName, string patientDescription, string visitDescription, string userPhoneNumber = null)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            
            try
            {
                // 配置 HttpClient 超时
                httpClient.Timeout = TimeSpan.FromSeconds(_difyApiOptions.TimeoutSeconds);
                
                var requestData = new DifyWorkflowRequest
                {
                    Inputs = new Dictionary<string, object>
                    {
                        ["syndrome_name"] = syndromeName,
                        ["patient_description"] = $"{patientDescription}\n\n问诊信息：{visitDescription}"
                    },
                    ResponseMode = _difyApiOptions.ResponseMode,
                    User = string.IsNullOrEmpty(userPhoneNumber) ? _difyApiOptions.User : userPhoneNumber
                };

                var json = JsonSerializer.Serialize(requestData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                // 设置请求头
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_difyApiOptions.DetailWorkflowApiKey}");

                var response = await httpClient.PostAsync(_difyApiOptions.DetailUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<DifyWorkflowResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    if (apiResponse?.Data?.Status == "succeeded" && apiResponse.Data.Outputs != null)
                    {
                        // 解析API返回的证候详情数据
                        return ParseSyndromeDetail(apiResponse.Data.Outputs);
                    }
                    else if (apiResponse?.Data?.Status == "failed")
                    {
                        throw new Exception($"Dify工作流执行失败: {apiResponse.Data.Error ?? "未知错误"}");
                    }
                    else
                    {
                        throw new Exception($"Dify工作流状态异常: {apiResponse?.Data?.Status ?? "未知状态"}");
                    }
                }
                else
                {
                    // 尝试解析错误响应
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<DifyErrorResponse>(responseContent);
                        throw new Exception($"Dify API调用失败 [{errorResponse?.Code}]: {errorResponse?.Message ?? responseContent}");
                    }
                    catch (JsonException)
                    {
                        throw new Exception($"Dify API调用失败 [HTTP {response.StatusCode}]: {responseContent}");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"网络请求失败: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new Exception($"请求超时: {ex.Message}", ex);
            }
            catch (Exception ex) when (!(ex is Exception))
            {
                throw new Exception($"获取证候详情失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 保存证候概览到数据库
        /// </summary>
        /// <param name="visitId">就诊ID</param>
        /// <param name="syndromeOverviews">证候概览列表</param>
        /// <returns>保存的证候列表</returns>
        public async Task<List<Syndrome>> SaveSyndromeOverviewAsync(int visitId, List<SyndromeOverviewDto> syndromeOverviews)
        {
            var syndromes = new List<Syndrome>();
            var now = DateTime.Now;

            // 按置信度降序排序
            var sortedOverviews = syndromeOverviews.OrderByDescending(s => s.Confidence).ToList();

            foreach (var overview in sortedOverviews)
            {
                var syndrome = new Syndrome
                {
                    VisitId = visitId,
                    SyndromeName = overview.SyndromeName,
                    Confidence = overview.Confidence,
                    MainSymptoms = JsonSerializer.Serialize(overview.MainSymptoms),
                    CommonDiseases = JsonSerializer.Serialize(overview.CommonDiseases),
                    DetailStatus = 0, // 未获取详情
                    CreatedAt = now,
                    UpdatedAt = now
                };

                syndromes.Add(syndrome);
                _context.Syndromes.Add(syndrome);
            }

            await _context.SaveChangesAsync();
            return syndromes;
        }

        /// <summary>
        /// 更新证候详情
        /// </summary>
        /// <param name="syndromeId">证候ID</param>
        /// <param name="detail">证候详情</param>
        public async Task UpdateSyndromeDetailAsync(int syndromeId, SyndromeDetailDto detail)
        {
            var syndrome = await _context.Syndromes.FindAsync(syndromeId);
            if (syndrome != null)
            {
                syndrome.Description = detail.Description;
                syndrome.PathogenesisAnalysis = detail.PathogenesisAnalysis;
                syndrome.TreatmentPrinciple = detail.TreatmentPrinciple;
                syndrome.RecommendedFormulas = JsonSerializer.Serialize(detail.RecommendedFormulas);
                syndrome.DetailStatus = 2; // 已获取详情
                syndrome.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 标记证候详情获取状态
        /// </summary>
        /// <param name="syndromeId">证候ID</param>
        /// <param name="status">状态 (0-未获取, 1-获取中, 2-已获取, 3-获取失败)</param>
        public async Task UpdateSyndromeDetailStatusAsync(int syndromeId, int status)
        {
            var syndrome = await _context.Syndromes.FindAsync(syndromeId);
            if (syndrome != null)
            {
                syndrome.DetailStatus = status;
                syndrome.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 使用指定的 DbContext 标记证候详情获取状态（用于并发场景）
        /// </summary>
        /// <param name="context">指定的 DbContext 实例</param>
        /// <param name="syndromeId">证候ID</param>
        /// <param name="status">状态 (0-未获取, 1-获取中, 2-已获取, 3-获取失败)</param>
        private async Task UpdateSyndromeDetailStatusWithContextAsync(TcmAiDiagnosisContext context, int syndromeId, int status)
        {
            var syndrome = await context.Syndromes.FindAsync(syndromeId);
            if (syndrome != null)
            {
                syndrome.DetailStatus = status;
                syndrome.UpdatedAt = DateTime.Now;
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 使用指定的 DbContext 更新证候详情（用于并发场景）
        /// </summary>
        /// <param name="context">指定的 DbContext 实例</param>
        /// <param name="syndromeId">证候ID</param>
        /// <param name="detail">证候详情</param>
        private async Task UpdateSyndromeDetailWithContextAsync(TcmAiDiagnosisContext context, int syndromeId, SyndromeDetailDto detail)
        {
            var syndrome = await context.Syndromes.FindAsync(syndromeId);
            if (syndrome != null)
            {
                syndrome.Description = detail.Description;
                syndrome.PathogenesisAnalysis = detail.PathogenesisAnalysis;
                syndrome.TreatmentPrinciple = detail.TreatmentPrinciple;
                syndrome.RecommendedFormulas = JsonSerializer.Serialize(detail.RecommendedFormulas);
                
                // 保存新增的字段
                syndrome.SyndromeCategories = detail.SyndromeCategories?.Any() == true 
                    ? JsonSerializer.Serialize(detail.SyndromeCategories) 
                    : null;
                syndrome.RelatedOrgans = detail.RelatedOrgans?.Any() == true 
                    ? JsonSerializer.Serialize(detail.RelatedOrgans) 
                    : null;
                syndrome.DiagnosisInfo = detail.DiagnosisInfo != null 
                    ? JsonSerializer.Serialize(detail.DiagnosisInfo) 
                    : null;
                syndrome.TreatmentCareInfo = detail.TreatmentCareInfo != null 
                    ? JsonSerializer.Serialize(detail.TreatmentCareInfo) 
                    : null;
                
                syndrome.DetailStatus = 2; // 已获取详情
                syndrome.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 获取就诊的证候列表
        /// </summary>
        /// <param name="visitId">就诊ID</param>
        /// <returns>证候列表</returns>
        public async Task<List<Syndrome>> GetSyndromesByVisitIdAsync(int visitId)
        {
            return await _context.Syndromes
                .Where(s => s.VisitId == visitId)
                .OrderByDescending(s => s.Confidence)
                .ToListAsync();
        }

        /// <summary>
        /// 获取证候详情
        /// </summary>
        /// <param name="syndromeId">证候ID</param>
        /// <returns>证候详情</returns>
        public async Task<Syndrome?> GetSyndromeByIdAsync(int syndromeId)
        {
            return await _context.Syndromes.FindAsync(syndromeId);
        }



        /// <summary>
        /// 确认证候（标记为确证）
        /// </summary>
        /// <param name="syndromeId">证候ID</param>
        public async Task ConfirmSyndromeAsync(int syndromeId)
        {
            var syndrome = await _context.Syndromes.FindAsync(syndromeId);
            if (syndrome != null)
            {
                // 先取消同一就诊下其他证候的确证状态
                var otherSyndromes = await _context.Syndromes
                    .Where(s => s.VisitId == syndrome.VisitId && s.SyndromeId != syndromeId)
                    .ToListAsync();

                foreach (var other in otherSyndromes)
                {
                    other.IsConfirmed = false;
                    other.UpdatedAt = DateTime.Now;
                }

                // 标记当前证候为确证
                syndrome.IsConfirmed = true;
                syndrome.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 异步获取证候详情（多线程）
        /// </summary>
        /// <param name="syndromes">证候列表</param>
        /// <param name="patientDescription">患者描述</param>
        /// <param name="visitDescription">问诊描述</param>
        /// <param name="userPhoneNumber">当前登录用户的手机号</param>
        public async Task FetchSyndromeDetailsAsync(List<Syndrome> syndromes, string patientDescription, string visitDescription, string userPhoneNumber = null)
        {
            var tasks = syndromes.Select(async syndrome =>
            {
                // 为每个任务创建独立的作用域和 DbContext，避免并发冲突
                using var scope = _serviceProvider.CreateScope();
                var scopedContext = scope.ServiceProvider.GetRequiredService<TcmAiDiagnosisContext>();
                
                try
                {
                    // 标记为获取中
                    await UpdateSyndromeDetailStatusWithContextAsync(scopedContext, syndrome.SyndromeId, 1);

                    // 调用API获取详情
                    var detail = await GetSyndromeDetailAsync(syndrome.SyndromeName, patientDescription, visitDescription, userPhoneNumber);

                    // 更新详情
                    await UpdateSyndromeDetailWithContextAsync(scopedContext, syndrome.SyndromeId, detail);
                }
                catch (Exception ex)
                {
                    // 标记为获取失败
                    await UpdateSyndromeDetailStatusWithContextAsync(scopedContext, syndrome.SyndromeId, 3);
                    // 记录错误日志
                    Console.WriteLine($"获取证候详情失败: {syndrome.SyndromeName}, 错误: {ex.Message}");
                }
            });

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 解析证候概览数据
        /// </summary>
        /// <param name="outputs">Dify工作流输出数据</param>
        /// <returns>证候概览列表</returns>
        private List<SyndromeOverviewDto> ParseSyndromeOverview(Dictionary<string, object> outputs)
        {
            var syndromes = new List<SyndromeOverviewDto>();

            try
            {
                // 添加调试信息：输出所有可用的字段名
                Console.WriteLine("=== Dify工作流输出调试信息 ===");
                Console.WriteLine($"输出字段数量: {outputs.Count}");
                foreach (var kvp in outputs)
                {
                    var valueType = kvp.Value?.GetType().Name ?? "null";
                    var valuePreview = kvp.Value?.ToString();
                    if (valuePreview != null && valuePreview.Length > 200)
                    {
                        valuePreview = valuePreview.Substring(0, 200) + "...";
                    }
                    Console.WriteLine($"字段名: '{kvp.Key}', 类型: {valueType}, 值预览: {valuePreview}");
                }
                Console.WriteLine("=== 调试信息结束 ===");

                // 根据实际的Dify工作流输出格式进行解析
                // 这里假设输出包含一个名为"syndromes"或"result"的字段
                
                object? syndromesData = null;
                string? foundFieldName = null;
                
                // 尝试不同的可能字段名
                var possibleFieldNames = new[] { "syndromes", "result", "text", "output", "data", "response", "answer" };
                
                foreach (var fieldName in possibleFieldNames)
                {
                    if (outputs.TryGetValue(fieldName, out syndromesData))
                    {
                        foundFieldName = fieldName;
                        Console.WriteLine($"找到数据字段: '{fieldName}'");
                        break;
                    }
                }

                if (syndromesData != null)
                {
                    Console.WriteLine($"开始解析字段 '{foundFieldName}' 的数据...");
                    
                    // 如果是字符串，尝试解析为JSON
                    if (syndromesData is string jsonString)
                    {
                        Console.WriteLine($"数据类型: 字符串, 长度: {jsonString.Length}");
                        Console.WriteLine($"字符串内容预览: {(jsonString.Length > 500 ? jsonString.Substring(0, 500) + "..." : jsonString)}");
                        
                        try
                        {
                            var jsonDocument = JsonDocument.Parse(jsonString);
                            Console.WriteLine("成功解析为JSON");
                            syndromes = ParseSyndromesFromJson(jsonDocument.RootElement);
                        }
                        catch (JsonException ex)
                        {
                            Console.WriteLine($"JSON解析失败: {ex.Message}");
                            throw new InvalidOperationException($"Dify API返回的数据格式不正确，无法解析为JSON。原始数据: {(jsonString.Length > 200 ? jsonString.Substring(0, 200) + "..." : jsonString)}", ex);
                        }
                    }
                    // 如果是JsonElement
                    else if (syndromesData is JsonElement jsonElement)
                    {
                        Console.WriteLine($"数据类型: JsonElement, ValueKind: {jsonElement.ValueKind}");
                        syndromes = ParseSyndromesFromJson(jsonElement);
                    }
                    // 如果是其他对象类型，尝试序列化后再解析
                    else
                    {
                        Console.WriteLine($"数据类型: {syndromesData.GetType().Name}");
                        var serializedJson = JsonSerializer.Serialize(syndromesData);
                        Console.WriteLine($"序列化后的JSON: {(serializedJson.Length > 500 ? serializedJson.Substring(0, 500) + "..." : serializedJson)}");
                        var jsonDocument = JsonDocument.Parse(serializedJson);
                        syndromes = ParseSyndromesFromJson(jsonDocument.RootElement);
                    }
                }
                else
                {
                    Console.WriteLine("未找到任何可识别的数据字段");
                }

                // 如果没有找到数据，返回空列表
                if (!syndromes.Any())
                {
                    Console.WriteLine("警告: 未能从Dify工作流输出中解析到证候数据");
                    Console.WriteLine("请检查Dify工作流的输出格式是否正确");
                }
                else
                {
                    Console.WriteLine($"成功解析到 {syndromes.Count} 个证候");
                }

                return syndromes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析证候概览数据时发生错误: {ex.Message}");
                Console.WriteLine($"错误堆栈: {ex.StackTrace}");
                return new List<SyndromeOverviewDto>();
            }
        }

        /// <summary>
        /// 从JSON元素解析证候数据
        /// </summary>
        /// <param name="jsonElement">JSON元素</param>
        /// <returns>证候概览列表</returns>
        private List<SyndromeOverviewDto> ParseSyndromesFromJson(JsonElement jsonElement)
        {
            var syndromes = new List<SyndromeOverviewDto>();

            // 如果是数组
            if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in jsonElement.EnumerateArray())
                {
                    var syndrome = ParseSingleSyndromeFromJson(element);
                    if (syndrome != null)
                    {
                        syndromes.Add(syndrome);
                    }
                }
            }
            // 如果是对象，可能包含syndromes数组
            else if (jsonElement.ValueKind == JsonValueKind.Object)
            {
                if (jsonElement.TryGetProperty("syndromes", out var syndromesArray) ||
                    jsonElement.TryGetProperty("data", out syndromesArray) ||
                    jsonElement.TryGetProperty("results", out syndromesArray))
                {
                    if (syndromesArray.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var element in syndromesArray.EnumerateArray())
                        {
                            var syndrome = ParseSingleSyndromeFromJson(element);
                            if (syndrome != null)
                            {
                                syndromes.Add(syndrome);
                            }
                        }
                    }
                }
                else
                {
                    // 尝试将整个对象作为单个证候解析
                    var syndrome = ParseSingleSyndromeFromJson(jsonElement);
                    if (syndrome != null)
                    {
                        syndromes.Add(syndrome);
                    }
                }
            }

            return syndromes;
        }

        /// <summary>
        /// 从JSON元素解析单个证候
        /// </summary>
        /// <param name="element">JSON元素</param>
        /// <returns>证候概览DTO</returns>
        private SyndromeOverviewDto? ParseSingleSyndromeFromJson(JsonElement element)
        {
            try
            {
                var syndrome = new SyndromeOverviewDto();

                // 解析证候名称 - 支持新的驼峰命名和旧的下划线命名
                if (element.TryGetProperty("syndromeName", out var nameElement) ||
                    element.TryGetProperty("syndrome_name", out nameElement) ||
                    element.TryGetProperty("name", out nameElement))
                {
                    syndrome.SyndromeName = nameElement.GetString() ?? "";
                }

                // 解析置信度
                if (element.TryGetProperty("confidence", out var confidenceElement))
                {
                    if (confidenceElement.ValueKind == JsonValueKind.Number)
                    {
                        syndrome.Confidence = confidenceElement.GetDecimal();
                    }
                    else if (confidenceElement.ValueKind == JsonValueKind.String)
                    {
                        if (decimal.TryParse(confidenceElement.GetString(), out var confidence))
                        {
                            syndrome.Confidence = confidence;
                        }
                    }
                }

                // 解析主要症状 - 支持新的驼峰命名和旧的下划线命名
                if (element.TryGetProperty("mainSymptoms", out var symptomsElement) ||
                    element.TryGetProperty("main_symptoms", out symptomsElement) ||
                    element.TryGetProperty("symptoms", out symptomsElement))
                {
                    syndrome.MainSymptoms = ParseStringArray(symptomsElement);
                }

                // 解析常见疾病 - 支持新的驼峰命名和旧的下划线命名
                if (element.TryGetProperty("commonDiseases", out var diseasesElement) ||
                    element.TryGetProperty("common_diseases", out diseasesElement) ||
                    element.TryGetProperty("diseases", out diseasesElement))
                {
                    syndrome.CommonDiseases = ParseStringArray(diseasesElement);
                }

                // 验证必要字段
                if (!string.IsNullOrWhiteSpace(syndrome.SyndromeName))
                {
                    return syndrome;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析单个证候时发生错误: {ex.Message}");
                return null;
            }
        }


        /// <summary>
        /// 解析字符串数组
        /// </summary>
        /// <param name="element">JSON元素</param>
        /// <returns>字符串列表</returns>
        private List<string> ParseStringArray(JsonElement element)
        {
            var result = new List<string>();

            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    var value = item.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        result.Add(value);
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.String)
            {
                var value = element.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    // 尝试按逗号分割
                    var items = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    result.AddRange(items.Select(item => item.Trim()));
                }
            }

            return result;
        }

        /// <summary>
        /// 解析证候详情数据
        /// </summary>
        /// <param name="outputs">Dify工作流输出数据</param>
        /// <returns>证候详情</returns>
        private SyndromeDetailDto ParseSyndromeDetail(Dictionary<string, object> outputs)
        {
            try
            {
                // 添加调试信息：输出所有可用的字段名
                Console.WriteLine("=== Dify证候详情工作流输出调试信息 ===");
                Console.WriteLine($"输出字段数量: {outputs.Count}");
                foreach (var kvp in outputs)
                {
                    var valueType = kvp.Value?.GetType().Name ?? "null";
                    var valuePreview = kvp.Value?.ToString();
                    if (valuePreview != null && valuePreview.Length > 200)
                    {
                        valuePreview = valuePreview.Substring(0, 200) + "...";
                    }
                    Console.WriteLine($"字段名: '{kvp.Key}', 类型: {valueType}, 值预览: {valuePreview}");
                }
                Console.WriteLine("=== 调试信息结束 ===");

                // 根据实际的Dify工作流输出格式进行解析
                object? detailData = null;
                string? foundFieldName = null;
                
                // 尝试不同的可能字段名
                var possibleFieldNames = new[] { "detail", "syndrome_detail", "result", "text", "output", "data", "response", "answer" };
                
                foreach (var fieldName in possibleFieldNames)
                {
                    if (outputs.TryGetValue(fieldName, out detailData))
                    {
                        foundFieldName = fieldName;
                        Console.WriteLine($"找到详情数据字段: '{fieldName}'");
                        break;
                    }
                }

                if (detailData != null)
                {
                    Console.WriteLine($"开始解析字段 '{foundFieldName}' 的详情数据...");
                    
                    // 如果是字符串，尝试解析为JSON
                    if (detailData is string jsonString)
                    {
                        Console.WriteLine($"数据类型: 字符串, 长度: {jsonString.Length}");
                        Console.WriteLine($"字符串内容预览: {(jsonString.Length > 500 ? jsonString.Substring(0, 500) + "..." : jsonString)}");
                        
                        try
                        {
                            var jsonDocument = JsonDocument.Parse(jsonString);
                            Console.WriteLine("成功解析为JSON");
                            return ParseSyndromeDetailFromJson(jsonDocument.RootElement);
                        }
                        catch (JsonException ex)
                        {
                            Console.WriteLine($"JSON解析失败: {ex.Message}");
                            throw new InvalidOperationException($"Dify API返回的证候详情数据格式不正确，无法解析为JSON。原始数据: {(jsonString.Length > 200 ? jsonString.Substring(0, 200) + "..." : jsonString)}", ex);
                        }
                    }
                    // 如果是JsonElement
                    else if (detailData is JsonElement jsonElement)
                    {
                        Console.WriteLine($"数据类型: JsonElement, ValueKind: {jsonElement.ValueKind}");
                        return ParseSyndromeDetailFromJson(jsonElement);
                    }
                    // 如果是其他对象类型，尝试序列化后再解析
                    else
                    {
                        Console.WriteLine($"数据类型: {detailData.GetType().Name}");
                        var serializedDetailJson = JsonSerializer.Serialize(detailData);
                        Console.WriteLine($"序列化后的JSON: {(serializedDetailJson.Length > 500 ? serializedDetailJson.Substring(0, 500) + "..." : serializedDetailJson)}");
                        var jsonDocument = JsonDocument.Parse(serializedDetailJson);
                        return ParseSyndromeDetailFromJson(jsonDocument.RootElement);
                    }
                }
                else
                {
                    Console.WriteLine("未找到任何可识别的详情数据字段");
                }

                // 如果没有找到数据，返回默认对象
                Console.WriteLine("警告: 未能从Dify工作流输出中解析到证候详情数据");
                Console.WriteLine("请检查Dify工作流的输出格式是否正确");
                return new SyndromeDetailDto { SyndromeName = "未知证候" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析证候详情数据时发生错误: {ex.Message}");
                Console.WriteLine($"错误堆栈: {ex.StackTrace}");
                return new SyndromeDetailDto { SyndromeName = "解析错误" };
            }
        }

        /// <summary>
        /// 从JSON元素解析证候详情
        /// </summary>
        /// <param name="jsonElement">JSON元素</param>
        /// <returns>证候详情DTO</returns>
        private SyndromeDetailDto ParseSyndromeDetailFromJson(JsonElement jsonElement)
        {
            var detail = new SyndromeDetailDto();

            try
            {
                // 解析证候名称 - 支持新的驼峰命名和旧的下划线命名
                if (jsonElement.TryGetProperty("syndromeName", out var nameElement) ||
                    jsonElement.TryGetProperty("syndrome_name", out nameElement) ||
                    jsonElement.TryGetProperty("name", out nameElement))
                {
                    detail.SyndromeName = nameElement.GetString() ?? "未知证候";
                }

                // 解析描述
                if (jsonElement.TryGetProperty("description", out var descriptionElement))
                {
                    detail.Description = descriptionElement.GetString() ?? "";
                }

                // 解析病因病机分析 - 支持新的驼峰命名和旧的下划线命名
                if (jsonElement.TryGetProperty("pathogenesisAnalysis", out var pathogenesisElement) ||
                    jsonElement.TryGetProperty("pathogenesis_analysis", out pathogenesisElement) ||
                    jsonElement.TryGetProperty("pathogenesis", out pathogenesisElement))
                {
                    detail.PathogenesisAnalysis = pathogenesisElement.GetString() ?? "";
                }

                // 解析治疗原则 - 支持新的驼峰命名和旧的下划线命名
                if (jsonElement.TryGetProperty("treatmentPrinciple", out var treatmentElement) ||
                    jsonElement.TryGetProperty("treatment_principle", out treatmentElement) ||
                    jsonElement.TryGetProperty("principle", out treatmentElement))
                {
                    detail.TreatmentPrinciple = treatmentElement.GetString() ?? "";
                }

                // 解析推荐方剂 - 支持新的驼峰命名和旧的下划线命名
                if (jsonElement.TryGetProperty("recommendedFormulas", out var formulasElement) ||
                    jsonElement.TryGetProperty("recommended_formulas", out formulasElement) ||
                    jsonElement.TryGetProperty("formulas", out formulasElement))
                {
                    detail.RecommendedFormulas = ParseStringArray(formulasElement);
                }

                // 解析主要症状 - 支持新的驼峰命名和旧的下划线命名
                if (jsonElement.TryGetProperty("mainSymptoms", out var mainSymptomsElement) ||
                    jsonElement.TryGetProperty("main_symptoms", out mainSymptomsElement) ||
                    jsonElement.TryGetProperty("symptoms", out mainSymptomsElement))
                {
                    detail.MainSymptoms = ParseStringArray(mainSymptomsElement);
                }

                // 解析常见疾病 - 支持新的驼峰命名和旧的下划线命名
                if (jsonElement.TryGetProperty("commonDiseases", out var diseasesElement) ||
                    jsonElement.TryGetProperty("common_diseases", out diseasesElement) ||
                    jsonElement.TryGetProperty("diseases", out diseasesElement))
                {
                    detail.CommonDiseases = ParseStringArray(diseasesElement);
                }

                // 解析置信度
                if (jsonElement.TryGetProperty("confidence", out var confidenceElement))
                {
                    if (confidenceElement.ValueKind == JsonValueKind.Number)
                    {
                        detail.Confidence = confidenceElement.GetDecimal();
                    }
                    else if (confidenceElement.ValueKind == JsonValueKind.String)
                    {
                        if (decimal.TryParse(confidenceElement.GetString(), out var confidenceValue))
                        {
                            detail.Confidence = confidenceValue;
                        }
                    }
                }

                // 解析证候分类
                if (jsonElement.TryGetProperty("syndromeCategories", out var categoriesElement) ||
                    jsonElement.TryGetProperty("syndrome_categories", out categoriesElement) ||
                    jsonElement.TryGetProperty("categories", out categoriesElement))
                {
                    detail.SyndromeCategories = ParseStringArray(categoriesElement);
                }

                // 解析归属脏腑
                if (jsonElement.TryGetProperty("relatedOrgans", out var organsElement) ||
                    jsonElement.TryGetProperty("related_organs", out organsElement) ||
                    jsonElement.TryGetProperty("organs", out organsElement))
                {
                    detail.RelatedOrgans = ParseStringArray(organsElement);
                }

                // 解析诊断信息
                if (jsonElement.TryGetProperty("diagnosisInfo", out var diagnosisInfoElement) ||
                    jsonElement.TryGetProperty("diagnosis_info", out diagnosisInfoElement))
                {
                    detail.DiagnosisInfo = ParseDiagnosisInfo(diagnosisInfoElement);
                }

                // 解析治疗与护理信息
                if (jsonElement.TryGetProperty("treatmentCareInfo", out var treatmentCareElement) ||
                    jsonElement.TryGetProperty("treatment_care_info", out treatmentCareElement))
                {
                    detail.TreatmentCareInfo = ParseTreatmentCareInfo(treatmentCareElement);
                }

                return detail;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"从JSON解析证候详情时发生错误: {ex.Message}");
                detail.SyndromeName = "解析错误";
                return detail;
            }
        }




        /// <summary>
        /// 解析诊断信息
        /// </summary>
        /// <param name="element">JSON元素</param>
        /// <returns>诊断信息</returns>
        private DiagnosisInfo ParseDiagnosisInfo(JsonElement element)
        {
            var diagnosisInfo = new DiagnosisInfo();

            try
            {
                // 解析诊断结论
                if (element.TryGetProperty("conclusion", out var conclusionElement))
                {
                    diagnosisInfo.Conclusion = ParseDiagnosisConclusion(conclusionElement);
                }

                // 解析诊断分析
                if (element.TryGetProperty("analysis", out var analysisElement))
                {
                    diagnosisInfo.Analysis = ParseDiagnosisAnalysis(analysisElement);
                }

                // 解析鉴别诊断
                if (element.TryGetProperty("differentialDiagnoses", out var differentialElement) ||
                    element.TryGetProperty("differential_diagnoses", out differentialElement))
                {
                    diagnosisInfo.DifferentialDiagnoses = ParseDifferentialDiagnoses(differentialElement);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析诊断信息时发生错误: {ex.Message}");
            }

            return diagnosisInfo;
        }

        /// <summary>
        /// 解析诊断结论
        /// </summary>
        /// <param name="element">JSON元素</param>
        /// <returns>诊断结论</returns>
        private DiagnosisConclusion ParseDiagnosisConclusion(JsonElement element)
        {
            var conclusion = new DiagnosisConclusion();

            try
            {
                if (element.TryGetProperty("primarySyndrome", out var primaryElement) ||
                    element.TryGetProperty("primary_syndrome", out primaryElement))
                {
                    conclusion.PrimarySyndrome = primaryElement.GetString() ?? "";
                }

                if (element.TryGetProperty("accompanyingSyndromes", out var accompanyingElement) ||
                    element.TryGetProperty("accompanying_syndromes", out accompanyingElement))
                {
                    conclusion.AccompanyingSyndromes = ParseStringArray(accompanyingElement);
                }

                if (element.TryGetProperty("constitutionType", out var constitutionElement) ||
                    element.TryGetProperty("constitution_type", out constitutionElement))
                {
                    conclusion.ConstitutionType = constitutionElement.GetString() ?? "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析诊断结论时发生错误: {ex.Message}");
            }

            return conclusion;
        }

        /// <summary>
        /// 解析诊断分析
        /// </summary>
        /// <param name="element">JSON元素</param>
        /// <returns>诊断分析</returns>
        private DiagnosisAnalysis ParseDiagnosisAnalysis(JsonElement element)
        {
            var analysis = new DiagnosisAnalysis();

            try
            {
                if (element.TryGetProperty("primaryBasis", out var primaryElement) ||
                    element.TryGetProperty("primary_basis", out primaryElement))
                {
                    analysis.PrimaryBasis = primaryElement.GetString() ?? "";
                }

                if (element.TryGetProperty("accompanyingBasis", out var accompanyingElement) ||
                    element.TryGetProperty("accompanying_basis", out accompanyingElement))
                {
                    analysis.AccompanyingBasis = accompanyingElement.GetString() ?? "";
                }

                if (element.TryGetProperty("constitutionInfluence", out var constitutionElement) ||
                    element.TryGetProperty("constitution_influence", out constitutionElement))
                {
                    analysis.ConstitutionInfluence = constitutionElement.GetString() ?? "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析诊断分析时发生错误: {ex.Message}");
            }

            return analysis;
        }

        /// <summary>
        /// 解析鉴别诊断
        /// </summary>
        /// <param name="element">JSON元素</param>
        /// <returns>鉴别诊断列表</returns>
        private List<DifferentialDiagnosis> ParseDifferentialDiagnoses(JsonElement element)
        {
            var differentialDiagnoses = new List<DifferentialDiagnosis>();

            try
            {
                if (element.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in element.EnumerateArray())
                    {
                        var differential = new DifferentialDiagnosis();

                        if (item.TryGetProperty("similarSyndrome", out var similarElement) ||
                            item.TryGetProperty("similar_syndrome", out similarElement))
                        {
                            differential.SimilarSyndrome = similarElement.GetString() ?? "";
                        }

                        if (item.TryGetProperty("keyDifference", out var differenceElement) ||
                            item.TryGetProperty("key_difference", out differenceElement))
                        {
                            differential.KeyDifference = differenceElement.GetString() ?? "";
                        }

                        if (!string.IsNullOrWhiteSpace(differential.SimilarSyndrome))
                        {
                            differentialDiagnoses.Add(differential);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析鉴别诊断时发生错误: {ex.Message}");
            }

            return differentialDiagnoses;
        }

        /// <summary>
        /// 解析治疗与护理信息
        /// </summary>
        /// <param name="element">JSON元素</param>
        /// <returns>治疗与护理信息</returns>
        private TreatmentCareInfo ParseTreatmentCareInfo(JsonElement element)
        {
            var treatmentCareInfo = new TreatmentCareInfo();

            try
            {
                // 解析治疗建议
                if (element.TryGetProperty("treatmentRecommendation", out var treatmentElement) ||
                    element.TryGetProperty("treatment_recommendation", out treatmentElement))
                {
                    treatmentCareInfo.TreatmentRecommendation = ParseTreatmentRecommendation(treatmentElement);
                }

                // 解析注意事项
                if (element.TryGetProperty("precautions", out var precautionsElement))
                {
                    treatmentCareInfo.Precautions = ParsePrecautions(precautionsElement);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析治疗与护理信息时发生错误: {ex.Message}");
            }

            return treatmentCareInfo;
        }

        /// <summary>
        /// 解析治疗建议
        /// </summary>
        /// <param name="element">JSON元素</param>
        /// <returns>治疗建议</returns>
        private TreatmentRecommendation ParseTreatmentRecommendation(JsonElement element)
        {
            var recommendation = new TreatmentRecommendation();

            try
            {
                if (element.TryGetProperty("treatmentPrinciple", out var principleElement) ||
                    element.TryGetProperty("treatment_principle", out principleElement))
                {
                    recommendation.TreatmentPrinciple = principleElement.GetString() ?? "";
                }

                if (element.TryGetProperty("formulaRecommendation", out var formulaElement) ||
                    element.TryGetProperty("formula_recommendation", out formulaElement))
                {
                    recommendation.FormulaRecommendation = formulaElement.GetString() ?? "";
                }

                if (element.TryGetProperty("dietaryAdvice", out var dietaryElement) ||
                    element.TryGetProperty("dietary_advice", out dietaryElement))
                {
                    recommendation.DietaryAdvice = dietaryElement.GetString() ?? "";
                }

                if (element.TryGetProperty("lifestyleAdvice", out var lifestyleElement) ||
                    element.TryGetProperty("lifestyle_advice", out lifestyleElement))
                {
                    recommendation.LifestyleAdvice = lifestyleElement.GetString() ?? "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析治疗建议时发生错误: {ex.Message}");
            }

            return recommendation;
        }

        /// <summary>
        /// 解析注意事项
        /// </summary>
        /// <param name="element">JSON元素</param>
        /// <returns>注意事项</returns>
        private Precautions ParsePrecautions(JsonElement element)
        {
            var precautions = new Precautions();

            try
            {
                if (element.TryGetProperty("medicationWarning", out var medicationElement) ||
                    element.TryGetProperty("medication_warning", out medicationElement))
                {
                    precautions.MedicationWarning = medicationElement.GetString() ?? "";
                }

                if (element.TryGetProperty("followUpAdvice", out var followUpElement) ||
                    element.TryGetProperty("follow_up_advice", out followUpElement))
                {
                    precautions.FollowUpAdvice = followUpElement.GetString() ?? "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析注意事项时发生错误: {ex.Message}");
            }

            return precautions;
        }
    }
}