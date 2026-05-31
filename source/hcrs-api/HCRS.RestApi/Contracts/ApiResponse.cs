namespace HCRS.RestApi.Contracts;

public class ApiResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? CorrelationId { get; set; } = null!;
}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }
}
