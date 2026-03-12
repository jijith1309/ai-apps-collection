# Social Post Generation

Two .NET samples for generating LinkedIn posts from user-provided content using Azure OpenAI — a basic variant and an advanced Semantic Kernel variant.

## Projects

### Variant A — Basic (`SocialPostGeneration/`)

A .NET 9 console app that takes user input and generates a structured LinkedIn post.

**Output JSON:**
```json
{
  "postText": "...",
  "needsMoreInfo": false,
  "containsDisallowedContent": false
}
```

- Validates content for disallowed material before generating
- Reports latency and token usage
- Supports both Azure OpenAI and direct OpenAI endpoints

### Variant B — Semantic Kernel (`SocialPostGeneration-MultiplePrompts/LinkedinPostGen-SK/`)

A .NET 10 app built with Semantic Kernel that demonstrates more advanced prompt engineering patterns.

- Multiple prompt templates (`PromptTemplates.cs`)
- AI invocation logging (`AiLogEntry.cs`)
- Interactive menu system (`UserMenu.cs`)
- `AiPromptService` wrapping Semantic Kernel calls

## Tech Stack

| | Variant A | Variant B |
|--|-----------|-----------|
| Runtime | .NET 9 | .NET 10 |
| AI SDK | Azure.AI.OpenAI | Semantic Kernel |
| Pattern | Direct chat completion | Prompt templates + SK |

## Prerequisites

1. .NET 9 SDK (Variant A) / .NET 10 SDK (Variant B)
2. Azure OpenAI deployment

## Configuration (User Secrets)

**Variant A:**
```bash
cd 4-social-post-generation/SocialPostGeneration
dotnet user-secrets set "AZURE_OPENAI_ENDPOINT"   "https://<resource>.openai.azure.com/"
dotnet user-secrets set "AZURE_OPENAI_API_KEY"    "<your-api-key>"
dotnet user-secrets set "AZURE_OPENAI_DEPLOYMENT" "<deployment-name>"
```

**Variant B:**
```bash
cd 4-social-post-generation/SocialPostGeneration-MultiplePrompts/LinkedinPostGen-SK
dotnet user-secrets set "AZURE_OPENAI_ENDPOINT"   "https://<resource>.openai.azure.com/"
dotnet user-secrets set "AZURE_OPENAI_API_KEY"    "<your-api-key>"
dotnet user-secrets set "AZURE_OPENAI_DEPLOYMENT" "<deployment-name>"
```

## Running

```bash
# Variant A
cd 4-social-post-generation/SocialPostGeneration
dotnet run

# Variant B
cd 4-social-post-generation/SocialPostGeneration-MultiplePrompts/LinkedinPostGen-SK
dotnet run
```

## Key Concepts Demonstrated

- Structured JSON output from LLMs
- Content safety validation (disallowed content check)
- Token usage and latency measurement
- Semantic Kernel prompt templates
- AI invocation logging pattern
