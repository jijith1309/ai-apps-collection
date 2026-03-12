namespace AngularDotNetChat.ApiService.Common;

/// <summary>Standard API response wrapper for all endpoints.</summary>
/// <typeparam name="T">The type of the response data payload.</typeparam>
public class ServiceResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    public static ServiceResponse<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public static ServiceResponse<T> Fail(string message) =>
        new() { Success = false, Message = message };
}
