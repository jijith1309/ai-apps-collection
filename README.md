# Microsoft AI Apps — Sample Collection

A collection of .NET and Angular AI application samples demonstrating various Microsoft and Azure AI patterns. Each project is self-contained and runnable locally.

## Projects

| # | Project | Tech Stack | AI Pattern |
|---|---------|-----------|------------|
| 1 | [Semantic Kernel + Ollama](./1_semantic-kernal-ollama-connection/) | .NET 8, Semantic Kernel | Local LLM with Ollama |
| 2 | [URL Smartening](./2-url-smartening/) | .NET 10, Azure AI Inference | SEO metadata generation |
| 3 | [Azure AI Agent (Basic)](./3-agent-foundry-model-basic/) | .NET 10, Azure AI Agents | Persistent server-side agents |
| 4 | [Social Post Generation](./4-social-post-generation/) | .NET 9/10, Semantic Kernel | LinkedIn post generation |
| 6 | [Agentic SQL](./6-agentic-sql/) | .NET 10, OpenAI SDK, EF Core | Natural language to SQL |
| 7a | [Angular + .NET RAG Chat](./7-chat-apps/angular-dotnet-embedding-chat/) | Angular 21, ASP.NET Core 10 | Full-stack RAG chat app |
| 7b | [Blazor Chat App](./7-chat-apps/ChatApp1/) | Blazor, .NET Aspire | RAG with Blazor UI |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Ollama](https://ollama.com/) (for local model projects) with `phi4-mini` and `all-minilm` models
- Azure OpenAI resource (for cloud projects)
- SQL Server / LocalDB (for database projects)
- Node.js 20+ (for the Angular project)

## Secrets Management

**No secrets are stored in this repository.** All projects use [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) or environment variables. See each project's README for setup instructions.

Common variables used across projects:

```
AZURE_OPENAI_ENDPOINT
AZURE_OPENAI_API_KEY
AZURE_OPENAI_DEPLOYMENT
AZURE_FOUNDRY_PROJECT_ENDPOINT
```

## Tech Stack Highlights

- **.NET 8 / 9 / 10** — latest runtime features
- **Semantic Kernel** — Microsoft's AI orchestration SDK
- **Azure AI Agents SDK** — persistent agent management
- **Azure OpenAI + Azure AI Inference** — cloud LLM access
- **Ollama** — local LLM execution (no cloud required)
- **Angular 21** — latest Angular with signals
- **Entity Framework Core 10** — ORM with SQL Server
- **.NET Aspire** — distributed app orchestration
- **PrimeNG + Tailwind CSS** — modern UI components
