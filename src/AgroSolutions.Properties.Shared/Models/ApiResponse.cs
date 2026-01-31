namespace AgroSolutions.Properties.Shared.Models;

/// <summary>
/// Modelo de resposta padronizado para APIs
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> SuccessResponse(T data)
    {
        return new ApiResponse<T> { Success = true, Data = data };
    }

    public static ApiResponse<T> ErrorResponse(params string[] errors)
    {
        return new ApiResponse<T> { Success = false, Errors = [.. errors] };
    }
}
