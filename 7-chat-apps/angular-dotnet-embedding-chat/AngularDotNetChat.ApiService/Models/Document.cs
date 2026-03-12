using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AngularDotNetChat.ApiService.Enums;

namespace AngularDotNetChat.ApiService.Models;

/// <summary>Represents an uploaded document and its embedding state.</summary>
public class Document
{
    [Key]
    public int DocumentId { get; set; }

    [Required, MaxLength(512)]
    public string FileName { get; set; } = string.Empty;

    [Required, MaxLength(1024)]
    public string FilePath { get; set; } = string.Empty;

    [Required, MaxLength(128)]
    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public int UploadedByUserId { get; set; }

    [ForeignKey(nameof(UploadedByUserId))]
    public User UploadedBy { get; set; } = null!;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public EmbeddingStatus EmbeddingStatus { get; set; } = EmbeddingStatus.Pending;

    public string? EmbeddingErrorMessage { get; set; }

    public ICollection<DocumentChunk> Chunks { get; set; } = [];
}
