using AngularDotNetChat.ApiService.Common;
using AngularDotNetChat.ApiService.Data;
using AngularDotNetChat.ApiService.DTOs;
using AngularDotNetChat.ApiService.Enums;
using AngularDotNetChat.ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace AngularDotNetChat.ApiService.Services;

public class DocumentService(
    AppDbContext db,
    IEmbeddingService embeddingService,
    IWebHostEnvironment env,
    ILogger<DocumentService> logger) : IDocumentService
{
    private static readonly string[] AllowedTypes =
    [
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/msword"
    ];

    public async Task<ServiceResponse<DocumentDto>> UploadAsync(IFormFile file, int userId)
    {
        if (!AllowedTypes.Contains(file.ContentType))
            return ServiceResponse<DocumentDto>.Fail("Only PDF, DOC, and DOCX files are supported.");

        var uploadsPath = Path.Combine(env.ContentRootPath, "Uploads");
        Directory.CreateDirectory(uploadsPath);

        var uniqueName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(uploadsPath, uniqueName);

        await using (var stream = File.Create(filePath))
            await file.CopyToAsync(stream);

        var document = new Document
        {
            FileName = file.FileName,
            FilePath = filePath,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            UploadedByUserId = userId,
            EmbeddingStatus = EmbeddingStatus.Pending
        };

        db.Documents.Add(document);
        await db.SaveChangesAsync();


        try
        {
            await embeddingService.EmbedDocumentAsync(document.DocumentId);
        }
        catch (Exception ex) { 
            logger.LogError(ex, "Background embedding failed for {DocId}", document.DocumentId);
            return ServiceResponse<DocumentDto>.Ok(ToDto(document), "Embedding failed. You can retry.");
        }


        return ServiceResponse<DocumentDto>.Ok(ToDto(document), "File uploaded and processed.You can now chat");
    }

    public async Task<ServiceResponse<List<DocumentDto>>> GetAllAsync(int userId)
    {
        var docs = await db.Documents
            .Where(d => d.UploadedByUserId == userId)
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => ToDto(d))
            .ToListAsync();

        return ServiceResponse<List<DocumentDto>>.Ok(docs);
    }

    public async Task<ServiceResponse<DocumentDto>> RetryEmbeddingAsync(int documentId, int userId)
    {
        var document = await db.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId && d.UploadedByUserId == userId);
        if (document is null)
            return ServiceResponse<DocumentDto>.Fail("Document not found.");

        if (document.EmbeddingStatus == EmbeddingStatus.Completed)
            return ServiceResponse<DocumentDto>.Fail("Embedding already completed. Delete and re-upload to regenerate.");

        document.EmbeddingStatus = EmbeddingStatus.Pending;
        document.EmbeddingErrorMessage = null;
        await db.SaveChangesAsync();

        //_ = Task.Run(async () =>
        //{
        //    try { await embeddingService.EmbedDocumentAsync(document.DocumentId); }
        //    catch (Exception ex) { logger.LogError(ex, "Retry embedding failed for {DocId}", document.DocumentId); }
        //});
        try { await embeddingService.EmbedDocumentAsync(document.DocumentId); }
        catch (Exception ex) { logger.LogError(ex, "Retry embedding failed for {DocId}", document.DocumentId); }
        return ServiceResponse<DocumentDto>.Ok(ToDto(document), "Embedding retry started.");
    }

    private static DocumentDto ToDto(Document d) => new(
        d.DocumentId,
        d.FileName,
        d.ContentType,
        d.FileSizeBytes,
        d.UploadedAt,
        d.EmbeddingStatus,
        d.EmbeddingErrorMessage);
}
