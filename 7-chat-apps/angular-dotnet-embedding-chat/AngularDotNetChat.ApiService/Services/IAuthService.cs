using AngularDotNetChat.ApiService.Common;
using AngularDotNetChat.ApiService.DTOs;

namespace AngularDotNetChat.ApiService.Services;

/// <summary>Authentication service contract.</summary>
public interface IAuthService
{
    Task<ServiceResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request);
    Task<ServiceResponse<LoginResponseDto>> RegisterAsync(RegisterRequestDto request);
}
