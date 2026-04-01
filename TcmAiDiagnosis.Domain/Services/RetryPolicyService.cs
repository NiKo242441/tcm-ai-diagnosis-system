using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TcmAiDiagnosis.Domain.Exceptions;

namespace TcmAiDiagnosis.Domain.Services
{
    /// <summary>
    /// 重试策略服务
    /// 提供API调用的重试机制和超时处理功能
    /// </summary>
    public class RetryPolicyService
    {
        private readonly ILogger<RetryPolicyService> _logger;

        public RetryPolicyService(ILogger<RetryPolicyService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 执行带重试策略的HTTP请求
        /// </summary>
        /// <param name="httpRequestFunc">HTTP请求函数</param>
        /// <param name="maxRetries">最大重试次数</param>
        /// <param name="baseDelayMs">基础延迟时间（毫秒）</param>
        /// <param name="operationName">操作名称（用于日志）</param>
        /// <returns>HTTP响应内容</returns>
        /// <exception cref="DifyApiException">当所有重试都失败时抛出</exception>
        public async Task<string> ExecuteWithRetryAsync(
            Func<Task<HttpResponseMessage>> httpRequestFunc,
            int maxRetries = 3,
            int baseDelayMs = 1000,
            string operationName = "HTTP请求")
        {
            Exception lastException = null;
            
            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation("开始执行 {OperationName}，第 {Attempt}/{MaxAttempts} 次尝试", 
                        operationName, attempt + 1, maxRetries + 1);

                    using var response = await httpRequestFunc();
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("{OperationName} 成功完成，第 {Attempt} 次尝试", 
                            operationName, attempt + 1);
                        return responseContent;
                    }

                    // 检查是否应该重试
                    if (!ShouldRetry(response.StatusCode, attempt, maxRetries))
                    {
                        _logger.LogError("{OperationName} 失败，状态码: {StatusCode}，不进行重试", 
                            operationName, response.StatusCode);
                        throw new DifyApiException($"{operationName} 失败", (int)response.StatusCode, responseContent);
                    }

                    _logger.LogWarning("{OperationName} 失败，状态码: {StatusCode}，将在 {Delay}ms 后重试", 
                        operationName, response.StatusCode, CalculateDelay(attempt, baseDelayMs));

                    lastException = new DifyApiException($"{operationName} 失败", (int)response.StatusCode, responseContent);
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
                {
                    _logger.LogWarning("{OperationName} 超时，第 {Attempt} 次尝试", operationName, attempt + 1);
                    lastException = new DifyApiException($"{operationName} 超时", ex);

                    if (attempt >= maxRetries)
                    {
                        break;
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning(ex, "{OperationName} 网络错误，第 {Attempt} 次尝试", operationName, attempt + 1);
                    lastException = new DifyApiException($"{operationName} 网络错误", ex);

                    if (attempt >= maxRetries)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{OperationName} 发生未预期的错误，第 {Attempt} 次尝试", operationName, attempt + 1);
                    lastException = ex;

                    // 对于未预期的错误，不进行重试
                    break;
                }

                // 如果不是最后一次尝试，等待后重试
                if (attempt < maxRetries)
                {
                    var delay = CalculateDelay(attempt, baseDelayMs);
                    await Task.Delay(delay);
                }
            }

            _logger.LogError("{OperationName} 在 {MaxRetries} 次重试后仍然失败", operationName, maxRetries + 1);
            
            if (lastException is DifyApiException difyException)
            {
                throw difyException;
            }
            
            throw new DifyApiException($"{operationName} 在多次重试后仍然失败", lastException);
        }

        /// <summary>
        /// 判断是否应该重试
        /// </summary>
        /// <param name="statusCode">HTTP状态码</param>
        /// <param name="currentAttempt">当前尝试次数</param>
        /// <param name="maxRetries">最大重试次数</param>
        /// <returns>是否应该重试</returns>
        private bool ShouldRetry(HttpStatusCode statusCode, int currentAttempt, int maxRetries)
        {
            // 如果已达到最大重试次数，不再重试
            if (currentAttempt >= maxRetries)
            {
                return false;
            }

            // 根据HTTP状态码决定是否重试
            return statusCode switch
            {
                // 服务器错误 - 应该重试
                HttpStatusCode.InternalServerError => true,
                HttpStatusCode.BadGateway => true,
                HttpStatusCode.ServiceUnavailable => true,
                HttpStatusCode.GatewayTimeout => true,
                
                // 请求超时 - 应该重试
                HttpStatusCode.RequestTimeout => true,
                
                // 速率限制 - 应该重试
                HttpStatusCode.TooManyRequests => true,
                
                // 客户端错误 - 通常不应该重试
                HttpStatusCode.BadRequest => false,
                HttpStatusCode.Unauthorized => false,
                HttpStatusCode.Forbidden => false,
                HttpStatusCode.NotFound => false,
                HttpStatusCode.MethodNotAllowed => false,
                HttpStatusCode.UnprocessableEntity => false,
                
                // 其他状态码 - 默认不重试
                _ => false
            };
        }

        /// <summary>
        /// 计算延迟时间（指数退避）
        /// </summary>
        /// <param name="attempt">当前尝试次数（从0开始）</param>
        /// <param name="baseDelayMs">基础延迟时间（毫秒）</param>
        /// <returns>延迟时间（毫秒）</returns>
        private int CalculateDelay(int attempt, int baseDelayMs)
        {
            // 指数退避：baseDelay * 2^attempt，最大不超过30秒
            var delay = baseDelayMs * Math.Pow(2, attempt);
            return Math.Min((int)delay, 30000);
        }

        /// <summary>
        /// 执行带重试策略的异步操作
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="operation">要执行的操作</param>
        /// <param name="maxRetries">最大重试次数</param>
        /// <param name="baseDelayMs">基础延迟时间（毫秒）</param>
        /// <param name="operationName">操作名称（用于日志）</param>
        /// <returns>操作结果</returns>
        public async Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> operation,
            int maxRetries = 3,
            int baseDelayMs = 1000,
            string operationName = "异步操作")
        {
            Exception lastException = null;
            
            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation("开始执行 {OperationName}，第 {Attempt}/{MaxAttempts} 次尝试", 
                        operationName, attempt + 1, maxRetries + 1);

                    var result = await operation();
                    
                    _logger.LogInformation("{OperationName} 成功完成，第 {Attempt} 次尝试", 
                        operationName, attempt + 1);
                    
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "{OperationName} 失败，第 {Attempt} 次尝试", operationName, attempt + 1);
                    lastException = ex;

                    // 如果不是最后一次尝试，等待后重试
                    if (attempt < maxRetries)
                    {
                        var delay = CalculateDelay(attempt, baseDelayMs);
                        await Task.Delay(delay);
                    }
                }
            }

            _logger.LogError("{OperationName} 在 {MaxRetries} 次重试后仍然失败", operationName, maxRetries + 1);
            throw lastException ?? new InvalidOperationException($"{operationName} 失败");
        }

        /// <summary>
        /// 创建带超时的HTTP客户端
        /// </summary>
        /// <param name="httpClientFactory">HTTP客户端工厂</param>
        /// <param name="timeoutSeconds">超时时间（秒）</param>
        /// <returns>配置了超时的HTTP客户端</returns>
        public HttpClient CreateHttpClientWithTimeout(IHttpClientFactory httpClientFactory, int timeoutSeconds)
        {
            var httpClient = httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            
            _logger.LogInformation("创建HTTP客户端，超时时间: {Timeout}秒", timeoutSeconds);
            
            return httpClient;
        }
    }
}