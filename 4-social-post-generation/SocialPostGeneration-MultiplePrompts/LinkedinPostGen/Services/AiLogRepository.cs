public class AiLogRepository
{
    public void Save(AiLogEntry entry)
    {
        // TODO: Replace this with real SQL / EF Core / Dapper
        Console.WriteLine($"\n[DB] Saved Log → User: {entry.UserId}, Total Tokens: {entry.TotalTokens}\n");
    }
}
