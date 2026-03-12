using AgenticAISql.Data;
using Azure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System.Data;

namespace AgenticAISql
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            var config = new ConfigurationBuilder()
                .AddUserSecrets<Program>(optional: true)
                .Build();
            var endpoint = config["AZURE_OPENAI_ENDPOINT"];
            var apiKey = config["AZURE_OPENAI_API_KEY"];
            var deploymentName = config["AZURE_OPENAI_DEPLOYMENT"];

            Console.WriteLine("Hello, World!");
            var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
               .UseSqlServer(config.GetConnectionString("DbConnection"))
               .Options;


            using var db = new AppDbContext(dbOptions);
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            Console.Write("Ask a question (e.g., 'Top 2 customers by total order value'): ");
            var question = Console.ReadLine() ?? "Show the top 2 customers by total order value";
            var prompt = $"""
You are a SQL translator. Return ONLY a single SQL Server-compatible SELECT statement that answers the question.
Schema:
- Customers(Id INTEGER, Name TEXT, City TEXT)
- Orders(Id INTEGER, CustomerId INTEGER, Total REAL, Created TEXT)
- Relationship: Orders.CustomerId -> Customers.Id
Constraints:
- Read-only (SELECT); no INSERT/UPDATE/DELETE/DROP.
- Use table/column names exactly as defined.
- If aggregation is needed, include GROUP BY.
Question: {question}
""";

            var options = new OpenAIClientOptions { Endpoint = new Uri(endpoint) };
            OpenAIClient openAiClient = new OpenAIClient(new AzureKeyCredential(apiKey), options);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You translate natural language to safe, read-only SQL."),
                new UserChatMessage(prompt)
            };

            // FIXED: Use ChatCompletionOptions (official way—no ChatRequest)
            var chatCompletionOptions = new ChatCompletionOptions
            {
                // MaxOutputTokenCount = 3000, // Maps to max_completion_tokens for GPT-5; ~1200 chars + buffer
                Temperature = 1.0f // Your value for creativity
            };
            var chatClient = openAiClient.GetChatClient(deploymentName);
            var response = chatClient.CompleteChat(messages, chatCompletionOptions);

            Console.WriteLine("Message:");
            foreach (var contentPart in response.Value.Content)
            {
                Console.WriteLine(contentPart.Text);
                // Add handling for other types (e.g., ImageContent) if multimodal
            }
            Console.WriteLine();


            // NEW: Token usage logging
            var usage = response.Value.Usage;
            if (usage != null)
            {
                Console.WriteLine($"Token usage: input={usage.InputTokenCount}, output={usage.OutputTokenCount}, total={usage.TotalTokenCount}");
                // GPT-5 specific: Check for reasoning tokens

            }
            else
            {
                Console.WriteLine("Token usage not available.");
            }
            var rawText = string.Join("\n", response.Value.Content.Select(p => p.Text));
            var sql = CleanSql(rawText);
            Console.WriteLine($"Sanitized SQL:\n{sql}\n");
            await using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            await using var reader = await cmd.ExecuteReaderAsync();
            var table = new DataTable();
            table.Load(reader);

            Console.WriteLine("Results:");
            foreach (DataColumn col in table.Columns)
                Console.Write($"{col.ColumnName}\t");
            Console.WriteLine();
            foreach (DataRow row in table.Rows)
            {
                foreach (var item in row.ItemArray)
                    Console.Write($"{item}\t");
                Console.WriteLine();
            }
        }

        private static string CleanSql(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

            // Normalize newlines and trim outer whitespace
            var sql = raw.Replace("\r\n", "\n").Trim();

            // Strip Markdown code fences (```sql ... ```)
            if (sql.StartsWith("```"))
            {
                var firstNewline = sql.IndexOf('\n');
                if (firstNewline >= 0)
                {
                    sql = sql[(firstNewline + 1)..];
                    var fenceClose = sql.IndexOf("```", StringComparison.Ordinal);
                    if (fenceClose >= 0)
                    {
                        sql = sql[..fenceClose];
                    }
                }
                sql = sql.Trim();
            }

            // Drop leading "sql" label if present
            if (sql.StartsWith("sql", StringComparison.OrdinalIgnoreCase))
            {
                sql = sql[3..].TrimStart();
            }

            // Remove stray quotes/backticks that sometimes prefix the statement
            sql = sql.Trim('`', '\"', '\'');

            return sql.Trim();
        }
    }
}
