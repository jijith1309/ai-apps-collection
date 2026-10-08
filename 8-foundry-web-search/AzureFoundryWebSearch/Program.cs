using Microsoft.Extensions.Configuration;
using OpenAI.Responses;
using System.ClientModel;
using System.Text.Json;
using System.Text.RegularExpressions;

#pragma warning disable OPENAI001

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>(optional: true)
    .Build();

var endpoint = config["AzureFoundry:Endpoint"]
    ?? throw new InvalidOperationException("AzureFoundry:Endpoint is required.");
var apiKey = config["AzureFoundry:ApiKey"]
    ?? throw new InvalidOperationException("AzureFoundry:ApiKey is required.");
var modelName = config["AzureFoundry:ModelName"]
    ?? throw new InvalidOperationException("AzureFoundry:ModelName is required.");

ResponsesClient client = new(
    new ApiKeyCredential(apiKey),
    new OpenAI.OpenAIClientOptions { Endpoint = new Uri(endpoint) });

// ── Input ─────────────────────────────────────────────────────────────────────
Console.Write("Enter website/domain: ");
string website = Console.ReadLine()
    ?? throw new InvalidOperationException("Website is required.");

// ═════════════════════════════════════════════════════════════════════════════
// PASS 1 — Find roles + real people names
// ═════════════════════════════════════════════════════════════════════════════
Console.WriteLine("\n[Pass 1] Finding buying committee roles...\n");

string pass1Prompt = """
    You are a B2B sales intelligence researcher.

    Target company website: {website}

    Search the web and research this company. Then identify 3-5 real buying 
    committee members a sales rep should approach.

    Prioritise people who:
    - evaluate or recommend vendors
    - influence purchasing decisions
    - manage implementation or operations
    - own tools, workflows, or KPIs

    Prefer roles like:
    - Department Heads, Senior Managers, Operations Leads
    - RevOps, MarketingOps, IT, Engineering, Procurement
    - Customer Success, Enablement, Platform/Admin teams

    Avoid CEO, Founder, Owner, President (unless company is very small).

    Rules:
    - Only include REAL people found via web search evidence
    - If no real person found for a role, return the role with null name fields
    - Do NOT invent or guess names
    - Find business email only if publicly discoverable — otherwise null
    - Do NOT return LinkedIn URLs

    Output ONLY valid JSON array. No markdown. No explanation.

    Schema:
    [
      {"first_name": "string|null",
        "last_name": "string|null",
        "job_title": "string",
        "email": "string|null",
        "company_name": "string"
      }
    ]
    """;

var pass1Options = new CreateResponseOptions { Model = modelName, MaxOutputTokenCount = 2000 };
pass1Options.Tools.Add(ResponseTool.CreateWebSearchTool());
pass1Prompt = pass1Prompt.Replace("{website}", website);
pass1Options.InputItems.Add(ResponseItem.CreateUserMessageItem(pass1Prompt));

var pass1Response = await client.CreateResponseAsync(pass1Options);

foreach (ResponseItem item in pass1Response.Value.OutputItems)
    if (item is WebSearchCallResponseItem w)
        Console.WriteLine($"  [web search] {w.Status}");

string? pass1Json = pass1Response.Value.OutputItems
    .OfType<MessageResponseItem>()
    .FirstOrDefault()
    ?.Content?.FirstOrDefault()?.Text;

if (string.IsNullOrWhiteSpace(pass1Json))
{
    Console.WriteLine("Pass 1 returned no response.");
    return;
}

// Parse Pass 1 results
List<Person> persons = ParsePersons(pass1Json);
Console.WriteLine($"\nFound {persons.Count} people. Resolving LinkedIn profiles...\n");

// ═════════════════════════════════════════════════════════════════════════════
// PASS 2 — LinkedIn URL resolution per person
// ═════════════════════════════════════════════════════════════════════════════
foreach (var person in persons)
{
    // Skip LinkedIn search if no name found in pass 1
    if (string.IsNullOrWhiteSpace(person.FirstName) && string.IsNullOrWhiteSpace(person.LastName))
    {
        Console.WriteLine($"  [{person.JobTitle}] No name found — skipping LinkedIn search.");
        continue;
    }

    string searchTarget = $"{person.FirstName} {person.LastName} {person.CompanyName}".Trim();
    Console.WriteLine($"  [Pass 2] Searching LinkedIn for: {searchTarget}");

    string pass2Prompt = $"""
        Search the web for this exact query:
        site:linkedin.com/in "{person.FirstName} {person.LastName}" "{person.CompanyName}"

        Also try:
        site:linkedin.com/in {person.FirstName} {person.LastName} {person.CompanyName}

        Return up to 3 real LinkedIn profile URLs found in search results.

        Rules:
        - Only return URLs that appear verbatim in search results
        - URLs must start with https://www.linkedin.com/in/ or https://linkedin.com/in/
        - Do NOT construct or guess URLs
        - If no results found, return an empty array

        Output ONLY valid JSON array of strings. No markdown. No explanation.
        Example: ["https://www.linkedin.com/in/john-smith-123", "https://www.linkedin.com/in/john-smith-456"]
        """;

    var pass2Options = new CreateResponseOptions { Model = modelName, MaxOutputTokenCount = 500 };
    pass2Options.Tools.Add(ResponseTool.CreateWebSearchTool());
    pass2Options.InputItems.Add(ResponseItem.CreateUserMessageItem(pass2Prompt));

    try
    {
        var pass2Response = await client.CreateResponseAsync(pass2Options);

        string? pass2Raw = pass2Response.Value.OutputItems
            .OfType<MessageResponseItem>()
            .FirstOrDefault()
            ?.Content?.FirstOrDefault()?.Text;

        person.LinkedInUrls = ParseLinkedInUrls(pass2Raw);

        Console.WriteLine($"    → {(person.LinkedInUrls.Count > 0
            ? $"{person.LinkedInUrls.Count} profile(s) found"
            : "No profiles found")}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"    → LinkedIn search failed: {ex.Message}");
    }

    // Small delay to avoid rate limiting
    await Task.Delay(500);
}

// ═════════════════════════════════════════════════════════════════════════════
// Output
// ═════════════════════════════════════════════════════════════════════════════
Console.WriteLine("\n══ Results ══════════════════════════════════════════════\n");

foreach (var person in persons)
{
    Console.WriteLine($"Name    : {person.FirstName} {person.LastName}");
    Console.WriteLine($"Title   : {person.JobTitle}");
    Console.WriteLine($"Email   : {person.Email ?? "—"}");

    if (person.LinkedInUrls.Count > 0)
    {
        Console.WriteLine("LinkedIn:");
        foreach (var url in person.LinkedInUrls)
            Console.WriteLine($"  → {url}");
    }
    else
    {
        Console.WriteLine("LinkedIn: —");
    }

    Console.WriteLine("─────────────────────────────────────────────────────");
}

// ═════════════════════════════════════════════════════════════════════════════
// Helpers
// ═════════════════════════════════════════════════════════════════════════════
static List<Person> ParsePersons(string raw)
{
    try
    {
        var json = raw.Replace("```json", "").Replace("```", "").Trim();
        var start = json.IndexOf('[');
        var end = json.LastIndexOf(']');
        if (start == -1 || end == -1) return [];

        json = json[start..(end + 1)];
        using var doc = JsonDocument.Parse(json);

        return doc.RootElement.EnumerateArray().Select(p => new Person
        {
            FirstName = p.TryGetProperty("first_name", out var fn) ? fn.GetString() : null,
            LastName = p.TryGetProperty("last_name", out var ln) ? ln.GetString() : null,
            JobTitle = p.TryGetProperty("job_title", out var jt) ? jt.GetString() ?? "" : "",
            Email = p.TryGetProperty("email", out var em) ? em.GetString() : null,
            CompanyName = p.TryGetProperty("company_name", out var cn) ? cn.GetString() ?? "" : "",
        }).ToList();
    }
    catch { return []; }
}

static List<string> ParseLinkedInUrls(string? raw)
{
    if (string.IsNullOrWhiteSpace(raw)) return [];

    try
    {
        var json = raw.Replace("```json", "").Replace("```", "").Trim();
        var start = json.IndexOf('[');
        var end = json.LastIndexOf(']');
        if (start == -1 || end == -1) return [];

        json = json[start..(end + 1)];
        using var doc = JsonDocument.Parse(json);

        return doc.RootElement
            .EnumerateArray()
            .Select(e => e.GetString())
            .Where(url =>
                !string.IsNullOrWhiteSpace(url) &&
                (url!.StartsWith("https://www.linkedin.com/in/") ||
                 url!.StartsWith("https://linkedin.com/in/")))
            .Select(url => url!)
            .Distinct()
            .Take(3)
            .ToList();
    }
    catch { return []; }
}

// ═════════════════════════════════════════════════════════════════════════════
// Model
// ═════════════════════════════════════════════════════════════════════════════
class Person
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string JobTitle { get; set; } = "";
    public string? Email { get; set; }
    public string CompanyName { get; set; } = "";
    public List<string> LinkedInUrls { get; set; } = [];
}