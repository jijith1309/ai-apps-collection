using System.Text.Json;
using AngularDotNetChat.ApiService.Data;
using AngularDotNetChat.ApiService.Enums;
using AngularDotNetChat.ApiService.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace AngularDotNetChat.ApiService.Services;

public class EmbeddingService(
    AppDbContext db,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    ILogger<EmbeddingService> logger) : IEmbeddingService
{
    private const int ChunkSize = 150;
    private const int ChunkOverlap = 20;

    public async Task<float[]> GetEmbeddingAsync(string text, CancellationToken ct = default)
    {
        var results = await embeddingGenerator.GenerateAsync([text], cancellationToken: ct);
        return results[0].Vector.ToArray();
    }

    public async Task EmbedDocumentAsync(int documentId, CancellationToken ct = default)
    {
       

        var document = await db.Documents.FindAsync([documentId], ct);
        if (document is null) return;

        try
        {
            document.EmbeddingStatus = EmbeddingStatus.Processing;
            await db.SaveChangesAsync(ct);

            var text = ExtractText(document.FilePath, document.ContentType);
            var chunks = ChunkText(text);

            var existingChunks = db.DocumentChunks.Where(c => c.DocumentId == documentId);
            db.DocumentChunks.RemoveRange(existingChunks);

            for (int i = 0; i < chunks.Count; i++)
            {
                var embedding = await GetEmbeddingAsync(chunks[i], ct);
                db.DocumentChunks.Add(new DocumentChunk
                {
                    DocumentId = documentId,
                    ChunkIndex = i,
                    ChunkText = chunks[i],
                    EmbeddingJson = JsonSerializer.Serialize(embedding)
                });
            }

            document.EmbeddingStatus = EmbeddingStatus.Completed;
            document.EmbeddingErrorMessage = null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to embed document {DocumentId}", documentId);
            document.EmbeddingStatus = EmbeddingStatus.Failed;
            document.EmbeddingErrorMessage = ex.Message;
        }

        await db.SaveChangesAsync(ct);
    }

    private static string ExtractText(string filePath, string contentType) => contentType switch
    {
        "application/pdf" => ExtractPdfText(filePath),
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ExtractDocxText(filePath),
        "application/msword" => ExtractDocxText(filePath),
        _ => File.ReadAllText(filePath)
    };

    private static string ExtractPdfText(string path)
    {
        using var doc = PdfDocument.Open(path);
        return string.Join("\n", doc.GetPages().Select(p => p.Text));
    }

    private static string ExtractDocxText(string path)
    {
        using var doc = WordprocessingDocument.Open(path, false);
        return string.Join("\n", doc.MainDocumentPart!.Document.Body!
            .Descendants<Paragraph>()
            .Select(p => p.InnerText));
    }

    private static List<string> ChunkText(string text)
    {
        var chunks = new List<string>();
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var current = new List<string>();

        foreach (var word in words)
        {
            current.Add(word);
            if (current.Count >= ChunkSize)
            {
                chunks.Add(string.Join(" ", current));
                current = current.TakeLast(ChunkOverlap).ToList();
            }
        }

        if (current.Count > 0)
            chunks.Add(string.Join(" ", current));

        return chunks;
    }
}
