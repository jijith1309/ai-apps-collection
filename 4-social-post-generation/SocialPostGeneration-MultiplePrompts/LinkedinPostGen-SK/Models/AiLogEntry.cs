public class AiLogEntry
{
    public string UserId { get; set; } = "";
    public string InputPrompt { get; set; } = "";
    public string Output { get; set; } = "";
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int TotalTokens { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
