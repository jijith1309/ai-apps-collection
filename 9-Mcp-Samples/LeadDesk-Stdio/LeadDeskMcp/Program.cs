using LeadDeskMcp.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Configure all logs to go to stderr (stdout is used for the MCP protocol messages).
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

// Use stdio transport (works if HTTP transport extension is not available)
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<SalesPipelineTools>()
    .WithTools<RandomNumberTools>();


await builder.Build().RunAsync();
