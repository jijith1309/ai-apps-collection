using Microsoft.Extensions.Configuration;
using SocialPostGeneration;


var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>(optional: true)
    .Build();

var endpoint = config["AZURE_OPENAI_ENDPOINT"];
var apiKey = config["AZURE_OPENAI_API_KEY"];
var deploymentName = config["AZURE_OPENAI_DEPLOYMENT"];
Console.WriteLine("Select \n 1. Azure Open AI or 2. Direct Open AI");
var choice = Console.ReadLine();
if (choice == "1")
{
    AzureOpenAIManager.RunAzureOpenAi(endpoint!, apiKey!, deploymentName!);
    
}
else if (choice == "2")
{
    OpenAIManager.Run(config["AZURE_OPENAI_ENDPOINT2"]!, apiKey!, deploymentName!);
}
