using System.ClientModel;
using Azure.AI.OpenAI;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.AI;
using OpenAI;
using ChatApp1.Web.Components;
using ChatApp1.Web.Services;
using ChatApp1.Web.Services.Ingestion;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var endpoint = builder.Configuration["AZURE_OPENAI_ENDPOINT"]
    ?? throw new InvalidOperationException("Missing 'AZURE_OPENAI_ENDPOINT' configuration.");
var apiKey = builder.Configuration["AZURE_OPENAI_API_KEY"]
    ?? throw new InvalidOperationException("Missing 'AZURE_OPENAI_API_KEY' configuration.");
var chatDeployment = builder.Configuration["AZURE_OPENAI_DEPLOYMENT"]
    ?? throw new InvalidOperationException("Missing 'AZURE_OPENAI_DEPLOYMENT' configuration.");
var azureOpenAIClient = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

builder.Services.AddChatClient(azureOpenAIClient.GetChatClient(chatDeployment).AsIChatClient())
    .UseFunctionInvocation()
    .UseOpenTelemetry(configure: c =>
        c.EnableSensitiveData = builder.Environment.IsDevelopment());

// Use Ollama with all-minilm for free local embeddings (384 dimensions)
var ollamaEndpoint = builder.Configuration.GetConnectionString("ollama") ?? "http://localhost:11434/v1";
var ollamaClient = new OpenAIClient(new ApiKeyCredential("unused"), new OpenAIClientOptions { Endpoint = new Uri(ollamaEndpoint) });
builder.Services.AddEmbeddingGenerator(ollamaClient.GetEmbeddingClient("all-minilm").AsIEmbeddingGenerator());

var vectorStoreConnectionString = builder.Configuration.GetConnectionString("DbConnection")
    ?? throw new InvalidOperationException("Missing connection string 'DbConnection'.");

// Ensure the SQL Server database exists (auto-create if missing)
var csb = new SqlConnectionStringBuilder(vectorStoreConnectionString);
var databaseName = csb.InitialCatalog;
csb.InitialCatalog = "master";
await using (var conn = new SqlConnection(csb.ConnectionString))
{
    await conn.OpenAsync();
    await using var cmd = conn.CreateCommand();
    cmd.CommandText = $"""
        IF DB_ID('{databaseName}') IS NULL
            CREATE DATABASE [{databaseName}]
        """;
    await cmd.ExecuteNonQueryAsync();
}

builder.Services.AddSqlServerVectorStore(_ => vectorStoreConnectionString);
builder.Services.AddSqlServerCollection<Guid, IngestedChunk>(IngestedChunk.CollectionName, vectorStoreConnectionString);
builder.Services.AddSingleton<DataIngestor>();
builder.Services.AddSingleton<SemanticSearch>();
builder.Services.AddKeyedSingleton("ingestion_directory", new DirectoryInfo(Path.Combine(builder.Environment.WebRootPath, "Data")));

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
