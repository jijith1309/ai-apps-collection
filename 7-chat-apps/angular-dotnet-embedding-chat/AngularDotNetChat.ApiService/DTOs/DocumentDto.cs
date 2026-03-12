using AngularDotNetChat.ApiService.Enums;

namespace AngularDotNetChat.ApiService.DTOs;

/// <summary>Document data transfer object returned to clients.</summary>
public record DocumentDto(
    int DocumentId,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    DateTime UploadedAt,
    EmbeddingStatus EmbeddingStatus,
    string? EmbeddingErrorMessage
);
