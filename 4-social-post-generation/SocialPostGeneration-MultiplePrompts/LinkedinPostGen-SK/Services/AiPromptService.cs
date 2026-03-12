using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using System;


public class AiPromptService
{
    private readonly IChatCompletionService _chat;
    private readonly ILogger<AiPromptService> _logger;

    public AiPromptService(IChatCompletionService chat, ILogger<AiPromptService> logger)
    {

        _chat = chat;
        _logger = logger;
    }
    public async Task<string> RunPromptAsync(string prompt, int maxTokens = 150)
    {
        var settings = new AzureOpenAIPromptExecutionSettings
        {
            // Azure o-models expect MaxOutputTokenCount
            //MaxTokens = maxTokens,
            // Temperature = 1,

        };

        try
        {
            // Use the chat completion service directly
            var result = await _chat.GetChatMessageContentAsync(prompt, settings);
            return result.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chat completion failed for user");
            throw;
        }
    }

   
}