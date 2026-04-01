namespace TcmAiDiagnosis.Domain.Paged
{
    // 分页请求参数模型
    public class PagedRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchKeyword { get; set; }
    }
}