using Azure;
using Azure.AI.Inference;

using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using OpenAI.Chat;
// Config
var config = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>(optional: true)
    .Build();

var endpoint = config["AZURE_OPENAI_ENDPOINT"];
var apiKey = config["AZURE_OPENAI_API_KEY"];
var deployment = config["AZURE_OPENAI_DEPLOYMENT"];
if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(deployment))
{
    Console.Error.WriteLine("Missing Azure AI configuration. Set:");
    Console.Error.WriteLine("  AZURE_OPENAI_ENDPOINT, AZURE_OPENAI_API_KEY, AZURE_OPENAI_DEPLOYMENT");
    Console.Error.WriteLine("Tip: in VS use __Project Properties > Debug > Environment variables__.");
    return;
}

// Normalize Foundry endpoint if used
endpoint = endpoint!.TrimEnd('/');
if (endpoint.Contains(".services.ai.azure.com", StringComparison.OrdinalIgnoreCase)
    && !endpoint.EndsWith("/models", StringComparison.OrdinalIgnoreCase))
{
    endpoint = endpoint.EndsWith("/api", StringComparison.OrdinalIgnoreCase)
        ? endpoint + "/models"
        : endpoint + "/api/models";
}

// Decide SDK by endpoint host
bool useAzureOpenAI =
    endpoint.Contains("openai.azure.com", StringComparison.OrdinalIgnoreCase) ||
    endpoint.Contains("cognitiveservices.azure.com", StringComparison.OrdinalIgnoreCase);

Console.WriteLine($"Endpoint: {endpoint}");
Console.WriteLine($"Model: {deployment}");
Console.WriteLine($"SDK: {(useAzureOpenAI ? "Azure.AI.OpenAI" : "Azure.AI.Inference")}");

string urlInput = args.Length > 0 ? args[0] : Prompt("Enter a URL:");
if (!Uri.TryCreate(urlInput, UriKind.Absolute, out var url) || (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps))
{
    Console.Error.WriteLine("Invalid URL. Provide an absolute http(s) URL.");
    return;
}

try
{
    var (suggestion, usage) = useAzureOpenAI
        ? await GetSuggestionsWithAzureOpenAIAsync(url, endpoint, apiKey, deployment)
        : await GetSuggestionsWithInferenceAsync(url, endpoint, apiKey, deployment);
    // var (suggestion, usage) =  await GetSuggestionsWithInferenceAsync(url, endpoint, apiKey, deployment);
    if (suggestion is null)
    {
        Console.Error.WriteLine("The model did not return usable suggestions.");
        return;
    }

    Console.WriteLine();
    Console.WriteLine("Suggested Title:");
    Console.WriteLine($"  {suggestion.Title}");

    Console.WriteLine();
    Console.WriteLine("Slug for short URL:");
    Console.WriteLine($"  {suggestion.Slug}");

    Console.WriteLine();
    Console.WriteLine("UTM (one set):");
    var utm = suggestion.Utm ?? new Utm();
    Console.WriteLine($"  utm_source={utm.Source}");
    Console.WriteLine($"  utm_medium={utm.Medium}");
    Console.WriteLine($"  utm_campaign={utm.Campaign}");
    Console.WriteLine($"  utm_content={utm.Content}");

    Console.WriteLine();
    Console.WriteLine("Tags (5):");
    Console.WriteLine($"  {string.Join(", ", (suggestion.Tags ?? Array.Empty<string>()).Take(5))}");

    Console.WriteLine();
    Console.WriteLine("Token usage:");
    Console.WriteLine($"  Input tokens   : {usage.Input}");
    Console.WriteLine($"  Output tokens  : {usage.Output}");
    Console.WriteLine($"  Total tokens   : {usage.Total}");
}
catch (RequestFailedException ex)
{
    Console.Error.WriteLine($"Azure request failed: {ex.Message}");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Unexpected error: {ex.Message}");
}

static string Prompt(string message)
{
    Console.Write(message + " ");
    return Console.ReadLine() ?? "";
}

// Azure.AI.Inference path (Foundry /api/models)
static async Task<(SuggestedMetadata? suggestion, TokenUsage usage)> GetSuggestionsWithInferenceAsync(
    Uri pageUrl, string endpoint, string apiKey, string model)
{
    var client = new ChatCompletionsClient(new Uri(endpoint), new AzureKeyCredential(apiKey));

    var systemPrompt =
        "You are an expert SEO/content assistant.\n" +
        "Given only a URL, infer reasonable, generic suggestions from the path/keywords and best practices.\n" +
        "Do NOT fabricate factual details about the page content.\n" +
        "Return ONLY a JSON object with the schema:\n" +
        "{ \"title\": string,\n" +
        "  \"tags\": string[5],\n" +
        "  \"utm\": { \"utm_source\": string, \"utm_medium\": string, \"utm_campaign\": string, \"utm_content\": string },\n" +
        "  \"slug\": string }\n" +
        "Constraints:\n" +
        " - title: concise, engaging (<= 70 chars)\n" +
        " - tags: exactly 5 concise tags (no #)\n" +
        " - utm: sensible values\n" +
        " - slug: lowercase, hyphenated, ascii, <= 30 chars, no trailing hyphen.";

    var options = new Azure.AI.Inference.ChatCompletionsOptions
    {
        Model = model,
        Temperature = 0.2f,
        MaxTokens = 400,
        ResponseFormat = new ChatCompletionsResponseFormatJsonObject()
    };
    options.Messages.Add(new ChatRequestSystemMessage(systemPrompt));
    options.Messages.Add(new ChatRequestUserMessage($"URL: {pageUrl}\nRespond with valid JSON only, no markdown, no comments."));

    var response = await client.CompleteAsync(options);

    var usageRaw = response.Value.Usage;
    var input = usageRaw?.PromptTokens ?? 0;
    var output = usageRaw?.CompletionTokens ?? 0;
    var total = usageRaw?.TotalTokens ?? (input + output);

    var contentText = response.Value.Content ?? "";
    if (string.IsNullOrWhiteSpace(contentText))
        return (null, new TokenUsage(input, output, total));

    if (!TryDeserialize(contentText!, out SuggestedMetadata? result))
    {
        if (TryExtractJsonObject(contentText!, out var extracted) && TryDeserialize(extracted, out result))
            return (result, new TokenUsage(input, output, total));
        return (null, new TokenUsage(input, output, total));
    }

    return (result, new TokenUsage(input, output, total));
}

//Azure.AI.OpenAI path(Azure OpenAI cognitiveservices/openai endpoint)
static async Task<(SuggestedMetadata? suggestion, TokenUsage usage)> GetSuggestionsWithAzureOpenAIAsync(
    Uri pageUrl, string endpoint, string apiKey, string deployment)
{
    try
    {
        var azureClient = new Azure.AI.OpenAI.AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
        var chatClient = azureClient.GetChatClient(deployment);

        var systemPrompt =
            "You are an expert SEO/content assistant.\n" +
            "Given only a URL, infer reasonable, generic suggestions from the path/keywords and best practices.\n" +
            "Do NOT fabricate factual details about the page content.\n" +
            "Return ONLY a JSON object with the schema:\n" +
            "{ \"title\": string,\n" +
            "  \"tags\": string[5],\n" +
            "  \"utm\": { \"utm_source\": string, \"utm_medium\": string, \"utm_campaign\": string, \"utm_content\": string },\n" +
            "  \"slug\": string }\n" +
            "Constraints:\n" +
            " - title: concise, engaging (<= 70 chars)\n" +
            " - tags: exactly 5 concise tags (no #)\n" +
            " - utm: sensible values\n" +
            " - slug: lowercase, hyphenated, ascii, <= 30 chars, no trailing hyphen.";

        var messages = new List<ChatMessage>
    {
        new SystemChatMessage(systemPrompt),
        new UserChatMessage($"URL: {pageUrl}\nRespond with valid JSON only, no markdown, no comments.")
    };

        var request = new ChatCompletionOptions
        {
           // Temperature = 0.2f,
            // ma = 400,
        };

        var response = await chatClient.CompleteChatAsync(messages, request);

        // Token usage (handle both older/newer property names)
        var usageRaw = response.Value.Usage;
        var input = usageRaw?.InputTokenCount ?? 0;
        var output = usageRaw?.OutputTokenCount ?? 0;
        var total = usageRaw?.TotalTokenCount ?? (input + output);

        // Message content
        var contentText = response.Value.Content.First().Text;
        if (string.IsNullOrWhiteSpace(contentText))
            return (null, new TokenUsage(input, output, total));

        if (!TryDeserialize<SuggestedMetadata>(contentText, out SuggestedMetadata? result))
        {
            if (TryExtractJsonObject(contentText, out var extracted) && TryDeserialize(extracted, out result))
                return (result, new TokenUsage(input, output, total));
            return (null, new TokenUsage(input, output, total));
        }

        return (result, new TokenUsage(input, output, total));
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Unexpected error: {ex.Message}");
        return (new SuggestedMetadata(), new TokenUsage());
    }

}
static bool TryDeserialize<T>(string json, out T? result)
{
    try
    {
        result = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        });
        return result is not null;
    }
    catch
    {
        result = default;
        return false;
    }
}

static bool TryExtractJsonObject(string text, out string json)
{
    int start = text.IndexOf('{');
    if (start < 0) { json = ""; return false; }
    int depth = 0;
    for (int i = start; i < text.Length; i++)
    {
        if (text[i] == '{') depth++;
        else if (text[i] == '}')
        {
            depth--;
            if (depth == 0) { json = text.Substring(start, i - start + 1); return true; }
        }
    }
    json = "";
    return false;
}

public sealed class SuggestedMetadata
{
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("tags")] public string[] Tags { get; set; } = Array.Empty<string>();
    [JsonPropertyName("utm")] public Utm Utm { get; set; } = new();
    [JsonPropertyName("slug")] public string Slug { get; set; } = "";
}
public sealed class Utm
{
    [JsonPropertyName("utm_source")] public string Source { get; set; } = "site";
    [JsonPropertyName("utm_medium")] public string Medium { get; set; } = "referral";
    [JsonPropertyName("utm_campaign")] public string Campaign { get; set; } = "auto";
    [JsonPropertyName("utm_content")] public string Content { get; set; } = "suggested";
}
public readonly record struct TokenUsage(int Input, int Output, int Total);