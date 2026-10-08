var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddScoped<SalesPipelineTools>();

builder.Services.AddMcpServer()
                .WithHttpTransport()
                .WithToolsFromAssembly();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// API key is stored in config (appsettings.json or environment variable Mcp__ApiKey)
var expectedKey = builder.Configuration["Mcp:ApiKey"]
    ?? throw new InvalidOperationException("Mcp:ApiKey is not configured.");

app.MapMcp("/api/mcp")
   .AddEndpointFilter(async (context, next) =>
   {
       if (!context.HttpContext.Request.Headers.TryGetValue("X-Api-Key", out var incomingKey)
           || !System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
               System.Text.Encoding.UTF8.GetBytes(incomingKey.ToString()),
               System.Text.Encoding.UTF8.GetBytes(expectedKey)))
       {
           context.HttpContext.Response.StatusCode = 401;
           await context.HttpContext.Response.WriteAsync("Unauthorized: invalid or missing X-Api-Key header.");
           return null;
       }
       return await next(context);
   });

// Add a simple health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("Health")
    .WithOpenApi();

app.Run();


