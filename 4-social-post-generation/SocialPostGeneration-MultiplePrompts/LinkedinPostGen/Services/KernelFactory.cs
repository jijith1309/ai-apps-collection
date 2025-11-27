using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public static class KernelFactory
{
    public static Kernel CreateKernel(IConfiguration config, IServiceProvider sp)
    {
        var builder = Kernel.CreateBuilder();

        
        builder.Services.AddSingleton<AiLogRepository>();

        builder.AddAzureOpenAIChatCompletion(
            deploymentName: config["AZURE_OPENAI_DEPLOYMENT"]!,
            endpoint: config["AZURE_OPENAI_ENDPOINT"]!,
            apiKey: config["AZURE_OPENAI_API_KEY"]!
        );

     //   builder.Services.AddChatCompletionHandler<ChatLoggingFilter>();

        return builder.Build();
    }
}
