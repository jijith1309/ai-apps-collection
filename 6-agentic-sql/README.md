# Agentic SQL

A .NET console app that translates natural language questions into SQL queries using Azure OpenAI, then executes them against a SQL Server database and displays the results.

## What It Does

```
Ask a question: Top 2 customers by total order value

Sanitized SQL:
SELECT TOP 2 c.Name, SUM(o.Total) AS TotalValue
FROM Customers c
JOIN Orders o ON o.CustomerId = c.Id
GROUP BY c.Name
ORDER BY TotalValue DESC

Results:
Name          TotalValue
Ada Lovelace  850.00
Alan Turing   620.00
```

The app:
1. Accepts a natural language question
2. Sends schema + question to Azure OpenAI to generate a `SELECT` statement
3. Strips any markdown fences from the LLM output
4. Executes the query (read-only) against a SQL Server database
5. Displays results in a tabular format and reports token usage

## Tech Stack

- .NET 10.0
- [OpenAI SDK](https://www.nuget.org/packages/OpenAI) (Azure-compatible)
- [Entity Framework Core 10](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer)
- SQL Server / LocalDB

## Database Schema

The app auto-creates the database on startup with sample data:

```
Customers(Id, Name, City)
Orders(Id, CustomerId, Total, Created)
```

Sample data: Ada Lovelace, Alan Turing, Grace Hopper with associated orders.

## Prerequisites

1. [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
2. SQL Server or LocalDB
3. Azure OpenAI deployment

## Configuration (User Secrets)

```bash
cd 6-agentic-sql/AgenticAISql
dotnet user-secrets set "AZURE_OPENAI_ENDPOINT"        "https://<resource>.openai.azure.com/"
dotnet user-secrets set "AZURE_OPENAI_API_KEY"         "<your-api-key>"
dotnet user-secrets set "AZURE_OPENAI_DEPLOYMENT"      "<deployment-name>"
dotnet user-secrets set "ConnectionStrings:DbConnection" "Server=localhost\\SQLEXPRESS;Database=AgenticSqlDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

## Running

```bash
cd 6-agentic-sql/AgenticAISql
dotnet run
```

## Key Concepts Demonstrated

- Natural language to SQL translation via LLM
- Schema-aware prompt engineering (injecting table definitions)
- Read-only SQL enforcement (SELECT only, no DML)
- Markdown code fence stripping from LLM responses
- `EnsureDeleted` + `EnsureCreated` for demo database lifecycle
- Dynamic query execution with `DbConnection` and `DataTable`
- Token usage reporting
