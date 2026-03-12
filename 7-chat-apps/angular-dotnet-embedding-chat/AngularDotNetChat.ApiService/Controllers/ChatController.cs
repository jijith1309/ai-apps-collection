using System.Security.Claims;
using System.Text;
using AngularDotNetChat.ApiService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AngularDotNetChat.ApiService.Controllers;

/// <summary>Handles AI chat queries with document context using Server-Sent Events streaming.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("text/event-stream")]
public class ChatController(IChatService chatService) : ControllerBase
{
    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Streams an AI-generated answer for the given query using SSE.</summary>
    /// <param name="query">The user's question.</param>
    /// <param name="documentId">Optional document ID to scope the search to a specific document.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Server-Sent Events stream of text chunks.</returns>
    [HttpGet("stream")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task Stream(
        [FromQuery] string query,
        [FromQuery] int? documentId,
        CancellationToken ct)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("X-Accel-Buffering", "no");

        await foreach (var chunk in chatService.StreamAnswerAsync(query, documentId, CurrentUserId, ct))
        {
            var sseData = $"data: {chunk}\n\n";
            await Response.WriteAsync(sseData, Encoding.UTF8, ct);
            await Response.Body.FlushAsync(ct);
        }

        await Response.WriteAsync("data: [DONE]\n\n", ct);
        await Response.Body.FlushAsync(ct);
    }
}
