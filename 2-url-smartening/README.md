# URL Smartening

A .NET console app that uses an AI model to generate SEO-friendly metadata from a URL — title, slug, UTM parameters, and tags — without fetching the actual page content.

## What It Does

Given a URL (e.g., `https://example.com/blog/dotnet-ai-guide`), the app uses an AI model to infer:

```json
{
  "title": "A Developer's Guide to AI in .NET",
  "slug": "dotnet-ai-guide",
  "utm": {
    "utm_source": "blog",
    "utm_medium": "organic",
    "utm_campaign": "dotnet-ai",
    "utm_content": "guide"
  },
  "tags": ["dotnet", "ai", "csharp", "azure", "developer"]
}
```

Token usage is reported after each request.

## Tech Stack

- .NET 10.0
- [Azure.AI.Inference](https://www.nuget.org/packages/Azure.AI.Inference) — for Azure AI Foundry endpoints
- [Azure.AI.OpenAI](https://www.nuget.org/packages/Azure.AI.OpenAI) — for Azure OpenAI endpoints
- Automatically detects which SDK to use based on the endpoint URL

## Prerequisites

1. [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
2. An Azure OpenAI or Azure AI Foundry deployment

## Configuration (User Secrets)

```bash
cd 2-url-smartening/Url-Smartener
dotnet user-secrets set "AZURE_OPENAI_ENDPOINT" "https://<your-resource>.openai.azure.com/"
dotnet user-secrets set "AZURE_OPENAI_API_KEY"  "<your-api-key>"
dotnet user-secrets set "AZURE_OPENAI_DEPLOYMENT" "<deployment-name>"
```

## Running

```bash
cd 2-url-smartening/Url-Smartener
dotnet run
# Prompts: Enter a URL:
```

Or pass the URL as an argument:

```bash
dotnet run -- "https://example.com/blog/dotnet-ai-guide"
```

## Key Concepts Demonstrated

- Dual SDK support: `Azure.AI.Inference` (Foundry) vs `Azure.AI.OpenAI` (cognitive services)
- Structured JSON output from LLMs via `ResponseFormat = ChatCompletionsResponseFormatJsonObject`
- Robust JSON extraction with fallback parsing
- Token usage tracking
- .NET User Secrets for credential management
