using System.Text;
using AngularDotNetChat.ApiService.Data;
using AngularDotNetChat.ApiService.OpenApi;
using AngularDotNetChat.ApiService.Services;
using AngularDotNetChat.ServiceDefaults;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.IdentityModel.Tokens;
using OpenAI;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// --- Database ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddDbContextFactory<AppDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Authentication ---
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
        // Allow token via query string for SSE (EventSource cannot set headers)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token))
                    ctx.Token = token;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// --- CORS (Angular dev server — allow any localhost port for Aspire dynamic ports) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
        policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// --- OpenAPI + Scalar (Microsoft recommended for .NET 10) ---
builder.Services.AddOpenApi();

// --- AI: (Ollama in dev, Azure OpenAI in prod) ---
var useOllama = builder.Configuration.GetValue<bool>("UseOllama");

if (useOllama)
{
    // Aspire injects the Ollama endpoint as a connection string; fall back to appsettings for standalone runs
    var ollamaEndpoint = builder.Configuration.GetConnectionString("ollama")
        ?? builder.Configuration["Ollama:Endpoint"]!;
    var ollamaEmbeddingModel = builder.Configuration["Ollama:EmbeddingModel"]!;
    var ollamaChatModel = builder.Configuration["Ollama:ChatModel"]!;

    builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(
        new OllamaEmbeddingGenerator(new Uri(ollamaEndpoint), ollamaEmbeddingModel));

    builder.Services.AddSingleton<IChatClient>(
        new OllamaChatClient(new Uri(ollamaEndpoint), ollamaChatModel));
}
else
{
    var azureApiKey = new System.ClientModel.ApiKeyCredential(builder.Configuration["AzureOpenAI:ApiKey"]!);
    var chatDeployment = builder.Configuration["AzureOpenAI:ChatDeployment"]!;
    var embeddingDeployment = builder.Configuration["AzureOpenAI:EmbeddingDeployment"]!;

    // Chat uses the AI Foundry OpenAI-compatible endpoint (/openai/v1)
    var chatClient = new AzureOpenAIClient(
        new Uri(builder.Configuration["AzureOpenAI:ChatEndpoint"]!), azureApiKey);

    // Embeddings use the base Azure OpenAI endpoint (no /openai/v1 suffix)
    var embeddingClient = new AzureOpenAIClient(
        new Uri(builder.Configuration["AzureOpenAI:EmbeddingEndpoint"]!), azureApiKey);

    builder.Services.AddSingleton<IChatClient>(_ =>
        chatClient.GetChatClient(chatDeployment).AsIChatClient());

    builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(
        embeddingClient.GetEmbeddingClient(embeddingDeployment).AsIEmbeddingGenerator());
}

// --- Application Services ---
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
builder.Services.AddScoped<IChatService, ChatService>();

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

var app = builder.Build();

app.MapDefaultEndpoints();

// --- API Docs (development only) ---
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "DocChat API";
        options.Theme = ScalarTheme.Purple;
        options.DefaultHttpClient = new(ScalarTarget.Http, ScalarClient.HttpClient);
    });
}

// --- Middleware ---
app.UseCors("Angular");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// --- Auto-migrate on startup ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
