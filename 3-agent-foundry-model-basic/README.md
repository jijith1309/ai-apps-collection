# Azure AI Agent — Basic Foundry Sample

A minimal .NET sample demonstrating how to create, retrieve, and invoke **persistent server-side agents** using the [Azure AI Agents Persistent SDK](https://www.nuget.org/packages/Azure.AI.Agents.Persistent).

## What It Does

1. Creates a persistent agent named `"Joker"` on Azure AI Foundry
2. Retrieves it back as an `AIAgent` via its ID
3. Creates a second agent using the direct `AIAgent` factory method
4. Invokes the agent on a new thread: `"Tell me a joke about a pirate."`
5. Cleans up both agents after execution

## Tech Stack

- .NET 10.0
- [Azure.AI.Agents.Persistent](https://www.nuget.org/packages/Azure.AI.Agents.Persistent)
- [Microsoft.Agents.AI](https://www.nuget.org/packages/Microsoft.Agents.AI)
- Azure Identity (`AzureCliCredential` — no API key needed)

## Prerequisites

1. [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
2. An [Azure AI Foundry](https://ai.azure.com/) project with a deployed model
3. Azure CLI logged in: `az login`

## Configuration (User Secrets)

```bash
cd 3-agent-foundry-model-basic/BasicFoundryAgent
dotnet user-secrets set "AZURE_FOUNDRY_PROJECT_ENDPOINT" "https://<your-project>.api.azureml.ms"
# Optional — defaults to gpt-4o-mini
dotnet user-secrets set "AZURE_FOUNDRY_PROJECT_DEPLOYMENT_NAME" "<deployment-name>"
```

## Running

```bash
cd 3-agent-foundry-model-basic/BasicFoundryAgent
dotnet run
```

## Key Concepts Demonstrated

- `PersistentAgentsClient` — create and manage server-side agents
- `Administration.CreateAgentAsync` vs `CreateAIAgentAsync` — two agent creation paths
- `AIAgent` abstraction for unified invocation
- `AgentThread` for stateful conversation management
- `AzureCliCredential` — passwordless authentication via Azure CLI
- Agent lifecycle management (create → use → delete)
