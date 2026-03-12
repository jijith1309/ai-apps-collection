using System.ComponentModel.DataAnnotations;

namespace AngularDotNetChat.ApiService.DTOs;

/// <summary>Request body for login.</summary>
public record LoginRequestDto(
    [Required, EmailAddress] string Email,
    [Required] string Password
);
