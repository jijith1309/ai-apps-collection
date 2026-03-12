using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AngularDotNetChat.ApiService.Models;

/// <summary>Represents a text chunk of a document along with its embedding vector.</summary>
public class DocumentChunk
{
    [Key]
    public int ChunkId { get; set; }

    public int DocumentId { get; set; }

    [ForeignKey(nameof(DocumentId))]
    public Document Document { get; set; } = null!;

    public int ChunkIndex { get; set; }

    [Required]
    public string ChunkText { get; set; } = string.Empty;

    /// <summary>Embedding vector stored as JSON-serialized float array.</summary>
    public string EmbeddingJson { get; set; } = "[]";
}
