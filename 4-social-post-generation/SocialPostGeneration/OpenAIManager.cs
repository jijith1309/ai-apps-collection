using Azure; // For AzureKeyCredential

using OpenAI; // Core OpenAIClient
using OpenAI.Chat; // For ChatMessage, ChatCompletionOptions, etc.
using System.Diagnostics;

namespace SocialPostGeneration
{
    static class OpenAIManager
    {
        public static async Task RunAsync(string endpoint, string? apiKey = null, string deploymentName = "gpt-5-nano")
        {
            string? GenerateLinkedInPostPrompt = @"
Generate one LinkedIn post (exactly 1300±10 chars) from the text below.
Style: professional, formal, insight-driven. Include 3–8 relevant hashtags.
Return valid JSON only:
{
  ""postText"": """"
}
Rules:
- No explanations or text outside JSON.
- Maintain coherence and engagement.
- Avoid filler or repetition.
Input:
{{$inputText}}
";

            Console.Write("Enter your prompt topic: ");
            string? topic = Console.ReadLine();

            // 1) Replace the placeholder with the user's input
            string finalPrompt = GenerateLinkedInPostPrompt.Replace("{{$inputText}}", topic ?? string.Empty);

            // Ensure endpoint has /openai/v1 for GPT-5 compatibility
            //if (!endpoint.EndsWith("/openai/v1", System.StringComparison.OrdinalIgnoreCase))
            //    endpoint = endpoint.TrimEnd('/') + "/openai/v1";

            // 2) Create client (key-based for testing; use DefaultAzureCredential for prod)
            OpenAIClient openAiClient;
            if (string.IsNullOrEmpty(apiKey))
            {
                // Prod: Managed identity (add Azure.Identity NuGet)
                // var credential = new DefaultAzureCredential();
                // openAiClient = new OpenAIClient(credential, new OpenAIClientOptions { Endpoint = new Uri(endpoint) });
                throw new InvalidOperationException("API key required for this example; implement managed identity for prod.");
            }
            else
            {
                var options = new OpenAIClientOptions { Endpoint = new Uri(endpoint) };
                openAiClient = new OpenAIClient(new AzureKeyCredential(apiKey), options);
            }

            // 3) Prepare messages (in OpenAI.Chat—no ChatRequest needed)
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a helpful assistant."),
                new UserChatMessage(finalPrompt)
            };

            // FIXED: Use ChatCompletionOptions (official way—no ChatRequest)
            var chatCompletionOptions = new ChatCompletionOptions
            {
                // MaxOutputTokenCount = 3000, // Maps to max_completion_tokens for GPT-5; ~1200 chars + buffer
                Temperature = 1.0f // Your value for creativity
            };

            var sw = Stopwatch.StartNew();
            try
            {
                // FIXED: Use GetChatClient().CompleteChatAsync (official, no ChatRequest)
                var chatClient = openAiClient.GetChatClient(deploymentName);
                var response = await chatClient.CompleteChatAsync(messages, chatCompletionOptions);

                sw.Stop();

                // Output content (handles multi-part if needed)
                Console.WriteLine("Chat Role: " + response.Value.Role);
                Console.WriteLine("Message:");
                foreach (var contentPart in response.Value.Content)
                {
                    Console.WriteLine(contentPart.Text);
                    // Add handling for other types (e.g., ImageContent) if multimodal
                }
                Console.WriteLine();


                // NEW: Token usage logging
                var usage = response.Value.Usage;
                if (usage != null)
                {
                    Console.WriteLine($"Token usage: input={usage.InputTokenCount}, output={usage.OutputTokenCount}, total={usage.TotalTokenCount}");
                    // GPT-5 specific: Check for reasoning tokens

                }
                else
                {
                    Console.WriteLine("Token usage not available.");
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                Console.WriteLine($"Error: {ex.Message}");

            }

            // Time taken
            Console.WriteLine($"Latency: {sw.Elapsed.TotalMilliseconds:F0} ms");
        }

        // Sync wrapper for backward compat (avoid in new code)
        public static void Run(string endpoint, string apiKey, string deploymentName)
        {
            RunAsync(endpoint, apiKey, deploymentName).GetAwaiter().GetResult();
        }
    }
}