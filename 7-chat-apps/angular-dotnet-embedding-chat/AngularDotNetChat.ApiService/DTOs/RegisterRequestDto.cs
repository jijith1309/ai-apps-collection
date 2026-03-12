using System.ComponentModel.DataAnnotations;

namespace AngularDotNetChat.ApiService.DTOs;

/// <summary>Request body for user registration.</summary>
public record RegisterRequestDto(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password
);
