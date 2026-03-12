namespace AngularDotNetChat.ApiService.Services;

/// <summary>Service contract for AI chat with document context.</summary>
public interface IChatService
{
    IAsyncEnumerable<string> StreamAnswerAsync(string query, int? documentId, int userId, CancellationToken ct = default);
}
