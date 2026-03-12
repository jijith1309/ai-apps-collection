using System.Runtime.CompilerServices;
using System.Text.Json;
using AngularDotNetChat.ApiService.Data;
using AngularDotNetChat.ApiService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;

namespace AngularDotNetChat.ApiService.Services;

public class ChatService(
    AppDbContext db,
    IChatClient chatClient,
    IEmbeddingService embeddingService,
    ILogger<ChatService> logger) : IChatService
{
    private const int TopK = 5;

    public async IAsyncEnumerable<string> StreamAnswerAsync(
        string query,
        int? documentId,
        int userId,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var queryEmbedding = await embeddingService.GetEmbeddingAsync(query, ct);

        var chunks = await GetRelevantChunksAsync(queryEmbedding, documentId, userId, ct);

        var context = chunks.Count > 0
            ? string.Join("\n\n---\n\n", chunks.Select(c => c.ChunkText))
            : "No relevant document passages found for this query.";

        var systemPrompt = $"""
            You are an expert research assistant. Your job is to answer the user's question thoroughly by combining two sources:

            1. **Document Context** (highest priority): The extracted passages from the user's uploaded documents provided below.
            2. **Your Knowledge**: Supplement the document content with relevant background information, explanations, definitions, industry context, related concepts, and any additional details from your training knowledge that help give a complete and useful answer.

            Instructions:
            - Always start your answer by addressing what the document says about the topic.
            - Then enrich the answer with broader context, background details, or related knowledge that the document may not cover.
            - If the document contains partial information, fill in the gaps using your knowledge.
            - Clearly distinguish between information sourced from the document and information you are adding from your general knowledge. Use phrases like "According to the document..." and "Additionally, from general knowledge...".
            - If the document has no relevant information at all, still answer the question using your knowledge and note that no relevant document content was found.
            - Format responses clearly using bullet points or numbered lists where appropriate.
            - Be concise yet comprehensive.

            --- Document Context ---
            {context}
            --- End of Document Context ---
            """;

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, query)
        };

        await foreach (var update in chatClient.GetStreamingResponseAsync(messages, cancellationToken: ct))
        {
            var text = update.Text;
            if (!string.IsNullOrEmpty(text))
                yield return text;
        }
    }

    private async Task<List<DocumentChunk>> GetRelevantChunksAsync(
        float[] queryEmbedding,
        int? documentId,
        int userId,
        CancellationToken ct)
    {
        var chunksQuery = db.DocumentChunks
            .Include(c => c.Document)
            .Where(c => c.Document.UploadedByUserId == userId);

        if (documentId.HasValue)
            chunksQuery = chunksQuery.Where(c => c.DocumentId == documentId.Value);

        var allChunks = await chunksQuery.ToListAsync(ct);

        return allChunks
            .Select(c => (Chunk: c, Score: CosineSimilarity(queryEmbedding,
                JsonSerializer.Deserialize<float[]>(c.EmbeddingJson) ?? [])))
            .OrderByDescending(x => x.Score)
            .Take(TopK)
            .Select(x => x.Chunk)
            .ToList();
    }

    private static float CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length || a.Length == 0) return 0f;
        float dot = 0, magA = 0, magB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }
        return magA == 0 || magB == 0 ? 0f : dot / (MathF.Sqrt(magA) * MathF.Sqrt(magB));
    }
}
