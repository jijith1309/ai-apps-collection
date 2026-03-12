# DocChat AI — Document-Aware Chat Assistant

A full-stack AI-powered chat application that lets users upload documents and have intelligent conversations about their content. Built with **Angular 21**, **ASP.NET Core 10**, and **RAG (Retrieval-Augmented Generation)** using local or cloud-hosted LLMs.

---

## What It Does

Users upload PDF or Word documents, which are automatically processed into semantic chunks and embedded as vectors. They can then chat with an AI assistant that retrieves the most relevant document sections and answers questions grounded in the actual document content — with real-time streaming responses.

---

## Key Features

- **Document Upload & Processing** — Upload PDF, DOC, or DOCX files (up to 20 MB); documents are chunked and embedded automatically
- **RAG-Powered Chat** — Answers are generated using the top-5 most semantically relevant document chunks (cosine similarity search)
- **Streaming Responses** — Real-time token-by-token streaming via Server-Sent Events (SSE)
- **JWT Authentication** — Secure registration and login; all documents and chats are user-scoped
- **Embedding Status Tracking** — Live status indicators (Pending → Processing → Completed / Failed) with retry support
- **Swappable AI Backend** — Local Ollama LLM for development; Azure OpenAI for production

---

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Angular 21, PrimeNG, Tailwind CSS 4 |
| Backend | ASP.NET Core 10, Entity Framework Core 10 |
| Database | SQL Server (LocalDB) |
| AI (Local Dev) | Ollama — `all-minilm` (embeddings), `phi4-mini` (chat) |
| AI (Production) | Azure OpenAI |
| Document Parsing | PdfPig (PDF), OpenXml SDK (DOCX/DOC) |
| Auth | JWT Bearer Tokens |
| Orchestration | .NET Aspire |

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Angular 21 SPA                          │
│  Auth  │  Documents (upload/status)  │  Chat (SSE stream)   │
└───────────────────────┬─────────────────────────────────────┘
                        │ HTTP / SSE
┌───────────────────────▼─────────────────────────────────────┐
│                  ASP.NET Core 10 API                        │
│  AuthController  │  DocumentController  │  ChatController   │
│               Service Layer                                  │
│  AuthService  │  DocumentService  │  EmbeddingService        │
│                    ChatService (RAG)                         │
└───────────────────────┬─────────────────────────────────────┘
                        │
           ┌────────────┼────────────┐
           │            │            │
      SQL Server     Ollama /    File Storage
      (EF Core)    Azure OpenAI   (Uploads/)
```

### RAG Pipeline

```
Upload File
    │
    ▼
Extract Text (PdfPig / OpenXml)
    │
    ▼
Chunk Text (150 words, 20-word overlap)
    │
    ▼
Generate Embeddings (Ollama all-minilm / Azure OpenAI)
    │
    ▼
Store Chunks + Vectors in SQL Server

─────────────────────────────────────

User Sends Chat Query
    │
    ▼
Embed Query Vector
    │
    ▼
Cosine Similarity Search → Top-5 Chunks
    │
    ▼
Build Prompt: System + Chunks + Query
    │
    ▼
Stream Response via SSE (phi4-mini / Azure OpenAI)
```

---

## Project Structure

```
angular-dotnet-embedding-chat/
├── AngularDotNetChat.ApiService/      # Main ASP.NET Core API
│   ├── Controllers/                   # Thin controllers (Auth, Document, Chat)
│   ├── Services/                      # Business logic + data access
│   ├── Models/                        # EF Core entities + EmbeddingStatus enum
│   ├── DTOs/                          # Request/response record types
│   └── Data/                          # AppDbContext + Migrations
│
├── AngularDotNetChat.AppHost/         # .NET Aspire orchestration
├── AngularDotNetChat.ServiceDefaults/ # Shared Aspire service config
│
└── angular-chat-app/                  # Angular 21 frontend
    └── src/app/
        ├── core/                      # Auth guard, interceptors, services
        ├── features/
        │   ├── auth/                  # Login/register
        │   ├── documents/             # Upload + status table
        │   └── chat/                  # Chat UI + SSE streaming
        └── shared/                    # Shell, sidebar, topbar, markdown pipe
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- SQL Server (LocalDB or Express) at `localhost\SQLEXPRESS01`
- [Ollama](https://ollama.com/) running locally (for local dev without Azure OpenAI)

### Configure Secrets (Required)

Sensitive values are intentionally left empty in `appsettings.json`. Use [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) to supply them locally — they are stored outside the project directory and never committed to Git.

```bash
cd AngularDotNetChat.ApiService

dotnet user-secrets set "Jwt:Key" "your-super-secret-key-min-32-chars"
dotnet user-secrets set "Jwt:Issuer" "AngularDotNetChat"
```

For Azure OpenAI (optional — skip if using Ollama):

```bash
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-azure-api-key"
dotnet user-secrets set "AzureOpenAI:ChatEndpoint" "https://your-resource.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:EmbeddingEndpoint" "https://your-resource.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ChatDeployment" "gpt-4o"
dotnet user-secrets set "AzureOpenAI:EmbeddingDeployment" "text-embedding-3-small"
```

### Pull Ollama Models

```bash
ollama pull all-minilm    # Embedding model
ollama pull phi4-mini     # Chat model
```

### Run with .NET Aspire (Recommended)

```bash
# From the solution root
dotnet run --project AngularDotNetChat.AppHost
```

Aspire starts both the API and Angular dev server with automatic service wiring. Database migrations run automatically on startup.

### Run Manually

**Backend:**
```bash
cd AngularDotNetChat.ApiService
dotnet run
# API at http://localhost:5107
# Scalar API docs at http://localhost:5107/scalar/v1
```

**Frontend:**
```bash
cd angular-chat-app
npm install
npm start
# App at http://localhost:4200
```

### Azure OpenAI (Optional)

Configure `appsettings.json` to switch from Ollama to Azure OpenAI:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "ApiKey": "your-api-key",
    "ChatDeployment": "gpt-4o"
  }
}
```

---

## API Overview

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/auth/register` | Create a new account |
| POST | `/api/auth/login` | Authenticate and receive JWT |
| GET | `/api/documents` | List user's uploaded documents |
| POST | `/api/documents/upload` | Upload a PDF/DOCX file |
| POST | `/api/documents/{id}/retry-embedding` | Retry failed embedding |
| GET | `/api/chat/stream` | SSE streaming chat (JWT via query param) |

Full interactive docs available at `/scalar/v1` when running locally.

---

## Design Decisions

**No Repository Layer** — Data access lives directly in service classes to keep the codebase lean and avoid unnecessary abstraction for a project of this scope.

**SSE over WebSockets** — Server-Sent Events are simpler for unidirectional streaming; JWT is passed as a query parameter since `EventSource` does not support custom headers.

**Angular Signals over RxJS** — Signal-based state management keeps component reactivity straightforward without the complexity of observable chains.

**Ollama / Azure OpenAI Abstraction** — `Microsoft.Extensions.AI` provides a unified `IChatClient` and `IEmbeddingGenerator` interface, making the LLM backend swappable via configuration with no code changes.

---

## Screenshots

> _Add screenshots of the Documents page and Chat interface here._

---

## License

MIT
