var builder = DistributedApplication.CreateBuilder(args);

var markitdown = builder.AddContainer("markitdown", "mcp/markitdown")
    .WithArgs("--http", "--host", "0.0.0.0", "--port", "3001")
    .WithHttpEndpoint(targetPort: 3001, name: "http");

var ollama = builder.AddConnectionString("ollama");

var webApp = builder.AddProject<Projects.ChatApp1_Web>("aichatweb-app");
webApp
    .WithEnvironment("MARKITDOWN_MCP_URL", markitdown.GetEndpoint("http"))
    .WithReference(ollama)
    .WaitFor(markitdown);

builder.Build().Run();
