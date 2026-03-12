namespace AngularDotNetChat.ApiService.DTOs;

/// <summary>Response payload returned after a successful login.</summary>
public record LoginResponseDto(
    string Token,
    string Email,
    int UserId,
    DateTime ExpiresAt
);
