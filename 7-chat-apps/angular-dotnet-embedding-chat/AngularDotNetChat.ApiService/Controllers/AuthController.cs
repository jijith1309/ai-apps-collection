using AngularDotNetChat.ApiService.Common;
using AngularDotNetChat.ApiService.DTOs;
using AngularDotNetChat.ApiService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AngularDotNetChat.ApiService.Controllers;

/// <summary>Handles user authentication — login and registration.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>Authenticates a user and returns a JWT bearer token.</summary>
    /// <param name="request">Email and password credentials.</param>
    /// <returns>JWT token and user info on success.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ServiceResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<LoginResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ServiceResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
    {
        var result = await authService.LoginAsync(request);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    /// <summary>Registers a new user account and returns a JWT bearer token.</summary>
    /// <param name="request">Email and password for the new account.</param>
    /// <returns>JWT token and user info for the new account.</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ServiceResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<LoginResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceResponse<LoginResponseDto>>> Register([FromBody] RegisterRequestDto request)
    {
        var result = await authService.RegisterAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
