public static class PromptTemplates
{
    public const string LinkedInPost = """
        Create a professional LinkedIn post about:
        {{$topic}}
        Maximum words: {{$words}}
    """;

    public const string Rephrase = "Rephrase professionally: {{$input}}";
    public const string Shorten = "Shorten the following text: {{$input}}";
    public const string Expand = "Expand the following text with more detail: {{$input}}";
    public const string MoreCasual = "Rewrite casually: {{$input}}";
    public const string MoreFormal = "Rewrite formally: {{$input}}";
}
