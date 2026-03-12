using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.Diagnostics;

namespace SocialPostGeneration
{
    public static class AzureOpenAIManager
    {
        public static void RunAzureOpenAi(string endpoint, string apiKey, string deploymentName)
        {
            string? GenerateLinkedInPostPrompt = @"
Generate a LinkedIn post from the content below.
Rules:
- Length ~500 chars (±10)
- Tone professional, formal, insight-driven
- Include 3–8 relevant hashtags
- Output ONLY valid JSON in exactly this format:
{""postText"":"""",""needsMoreInfo"":false,""containsDisallowedContent"":false}

Decision logic (strict order, do not reorder):
1) If content contains violence, killing, harm, threats, profanity, hate, sexual, abusive, or disallowed language → set containsDisallowedContent=true and leave postText empty
2) Else if content is too short, unclear, or lacks context → set needsMoreInfo=true and leave postText empty
3) Else generate postText and keep both flags false

Content:
{{$inputText}}
";
            AzureOpenAIClient azureClient = new(
    new Uri(endpoint),
    new AzureKeyCredential(apiKey));

            Console.Write("Enter your prompt topic: ");
            string? topic = Console.ReadLine();

            // 1) Replace the placeholder with the user's input
            string finalPrompt = GenerateLinkedInPostPrompt.Replace("{{$inputText}}", topic ?? string.Empty);

            ChatClient chatClient = azureClient.GetChatClient(deploymentName);

            // 2) Send the composed prompt to the model
            List<ChatMessage> messages = new()
{
    new SystemChatMessage("You are a helpful assistant."),
    new UserChatMessage(finalPrompt),
};



            // Measure time
            var sw = Stopwatch.StartNew();
            var response = chatClient.CompleteChat(messages, new ChatCompletionOptions() { MaxOutputTokenCount=500});
            sw.Stop();

            // Output model content
            Console.WriteLine(response.Value.Content[0].Text);
            Console.WriteLine();

            // Token usage (if provided)
            var usage = response.Value.Usage;
            if (usage != null)
            {
                Console.WriteLine($"Token usage: input={usage.InputTokenCount}, output={usage.OutputTokenCount}, total={usage.TotalTokenCount}");
            }
            else
            {
                Console.WriteLine("Token usage not available in response.");
            }

            // Time taken
            Console.WriteLine($"Latency: {sw.Elapsed.TotalMilliseconds:F0} ms");
        }

    }
}
