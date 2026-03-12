using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(cfg => cfg.AddUserSecrets<Program>())
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;

        services.AddKernel()
            .AddAzureOpenAIChatCompletion(
                deploymentName: config["AZURE_OPENAI_DEPLOYMENT"],
                endpoint: config["AZURE_OPENAI_ENDPOINT"],
                apiKey: config["AZURE_OPENAI_API_KEY"]);

        services.AddSingleton<AiPromptService>();
        services.AddSingleton<UserMenu>();
    })
    .Build();

await host.Services.GetRequiredService<UserMenu>().RunAsync();