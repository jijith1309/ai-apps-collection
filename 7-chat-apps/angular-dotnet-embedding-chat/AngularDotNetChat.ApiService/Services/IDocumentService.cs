using AngularDotNetChat.ApiService.Common;
using AngularDotNetChat.ApiService.DTOs;

namespace AngularDotNetChat.ApiService.Services;

/// <summary>Service contract for document management.</summary>
public interface IDocumentService
{
    Task<ServiceResponse<DocumentDto>> UploadAsync(IFormFile file, int userId);
    Task<ServiceResponse<List<DocumentDto>>> GetAllAsync(int userId);
    Task<ServiceResponse<DocumentDto>> RetryEmbeddingAsync(int documentId, int userId);
}
