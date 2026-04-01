namespace TcmAiDiagnosis.Web
{
    public class ApiResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        // 静态方法用于创建成功结果
        public static ApiResult SuccessResult(string message = "操作成功")
        {
            return new ApiResult { Success = true, Message = message };
        }

        // 静态方法用于创建失败结果
        public static ApiResult ErrorResult(string message)
        {
            return new ApiResult { Success = false, Message = message };
        }
    }

    public class ApiResult<T> : ApiResult
    {
        public T? Data { get; set; }

        // 静态方法用于创建成功结果（带数据）
        public static ApiResult<T> SuccessResult(T data, string message = "操作成功")
        {
            return new ApiResult<T> { Success = true, Message = message, Data = data };
        }

        // 静态方法用于创建失败结果
        public static new ApiResult<T> ErrorResult(string message)
        {
            return new ApiResult<T> { Success = false, Message = message };
        }
    }
}