using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// --- Ollama (local install at localhost:11434 — must be running before starting) ---
var ollama = builder.AddConnectionString("ollama", ReferenceExpression.Create($"http://localhost:11434"));

// .NET 10 API Service
var apiService = builder.AddProject<Projects.AngularDotNetChat_ApiService>("apiservice")
    .WithReference(ollama)
    .WithExternalHttpEndpoints();

// Angular 21 Frontend (npm dev server)
builder.AddExecutable("angular-chat-app", "npm", "../angular-chat-app", "run", "start")
    .WithReference(apiService)
    .WithHttpEndpoint(targetPort: 4200, env: "PORT")
    .WithExternalHttpEndpoints();

builder.Build().Run();
