# Semantic Kernel + Ollama Connection

A minimal .NET sample showing how to connect [Microsoft Semantic Kernel](https://github.com/microsoft/semantic-kernel) to a locally running [Ollama](https://ollama.com/) instance and create a `ChatCompletionAgent`.

## What It Does

Creates a C# tutor agent powered by the `phi4-mini` model running locally via Ollama. The agent answers a question about the difference between a class and a record in C#.

```
Agent: "You are online tutor for c# and dot net developers"
User:  "What is the difference between a class and a record?"
```

## Tech Stack

- .NET 8.0
- [Semantic Kernel](https://www.nuget.org/packages/Microsoft.SemanticKernel) v1.61+
- [Semantic Kernel Ollama Connector](https://www.nuget.org/packages/Microsoft.SemanticKernel.Connectors.Ollama)
- Ollama (local LLM runtime)

## Prerequisites

1. [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
2. [Ollama](https://ollama.com/) installed and running
3. Pull the model:
   ```bash
   ollama pull phi4-mini
   ```

## Running

```bash
cd 1_semantic-kernal-ollama-connection/SemanticKernalOllamaConnection
dotnet run
```

Ollama must be running at `http://localhost:11434` (default).

## Key Concepts Demonstrated

- `Kernel.CreateBuilder()` with `AddOllamaChatCompletion`
- `ChatCompletionAgent` definition with instructions and name
- Streaming agent responses with `InvokeAsync` + `await foreach`
- `ChatHistory` for multi-turn conversation tracking
