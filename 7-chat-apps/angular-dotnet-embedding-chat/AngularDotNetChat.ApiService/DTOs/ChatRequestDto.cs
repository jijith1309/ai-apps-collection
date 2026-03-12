using System.ComponentModel.DataAnnotations;

namespace AngularDotNetChat.ApiService.DTOs;

/// <summary>Request payload for a chat query.</summary>
public record ChatRequestDto(
    [Required] string Query,
    int? DocumentId = null
);
