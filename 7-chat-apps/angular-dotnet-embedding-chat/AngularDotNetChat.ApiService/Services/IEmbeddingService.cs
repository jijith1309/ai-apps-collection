namespace AngularDotNetChat.ApiService.Services;

/// <summary>Service contract for generating text embeddings.</summary>
public interface IEmbeddingService
{
    Task<float[]> GetEmbeddingAsync(string text, CancellationToken ct = default);
    Task EmbedDocumentAsync(int documentId, CancellationToken ct = default);
}
