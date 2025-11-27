using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;

using OpenAI.Chat;
using System.Diagnostics;


var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>(optional: true)
    .Build();

var endpoint = config["AZURE_OPENAI_ENDPOINT"];
var apiKey = config["AZURE_OPENAI_API_KEY"];
var deploymentName = config["AZURE_OPENAI_DEPLOYMENT"];
string? GenerateLinkedInPostPrompt = @"
Generate one LinkedIn post (exactly 1300±10 chars) from the text below.
Style: professional, formal, insight-driven,Include Hastags in postText.  
Include 3–8 relevant hashtags.  
Return valid JSON only:
{
  ""postText"":  """"
}
Rules:
- No explanations or text outside JSON.
- Maintain coherence and engagement.
- Avoid filler or repetition.
Input:
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
var response = chatClient.CompleteChat(messages);
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



