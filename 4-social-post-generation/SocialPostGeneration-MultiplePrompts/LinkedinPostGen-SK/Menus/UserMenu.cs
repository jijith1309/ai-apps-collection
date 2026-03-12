

public class UserMenu
{
    private readonly AiPromptService _service;

    public UserMenu(AiPromptService service)
    {
        _service = service;
    }

    public async Task RunAsync()
    {
       

        Console.Write("\nEnter LinkedIn post topic: ");
        string topic = Console.ReadLine()!;

        Console.Write("Word limit: ");
        string words = Console.ReadLine()!;

        // First generation
        var prompt = PromptTemplates.LinkedInPost
            .Replace("{{$topic}}", topic)
            .Replace("{{$words}}", words);

        var result = await _service.RunPromptAsync(prompt);

        Console.WriteLine("\n=== Generated LinkedIn Post ===\n");
        Console.WriteLine(result);

        // Next options
        while (true)
        {
            Console.WriteLine("\nChoose an option:");
            Console.WriteLine("1. Rephrase");
            Console.WriteLine("2. Shorten");
            Console.WriteLine("3. Expand");
            Console.WriteLine("4. More Casual");
            Console.WriteLine("5. More Formal");
            Console.WriteLine("0. Exit");

            Console.Write("Option: ");
            var op = Console.ReadLine();

            if (op == "0") break;

            string template = op switch
            {
                "1" => PromptTemplates.Rephrase,
                "2" => PromptTemplates.Shorten,
                "3" => PromptTemplates.Expand,
                "4" => PromptTemplates.MoreCasual,
                "5" => PromptTemplates.MoreFormal,
                _ => PromptTemplates.Rephrase
            };

            string nextPrompt = template.Replace("{{$input}}", result);
            result = await _service.RunPromptAsync( nextPrompt);

            Console.WriteLine("\n=== Updated Output ===\n");
            Console.WriteLine(result);
        }
    }
}
