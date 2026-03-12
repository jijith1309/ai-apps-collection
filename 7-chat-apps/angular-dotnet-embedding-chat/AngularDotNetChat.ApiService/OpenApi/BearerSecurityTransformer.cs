using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace AngularDotNetChat.ApiService.OpenApi;

/// <summary>
/// Adds JWT Bearer security scheme to the OpenAPI document and marks
/// authorized endpoints with the required security requirement.
/// </summary>
internal sealed class BearerSecurityTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Paste your JWT token below. Obtain it from **POST /api/auth/login**."
        };

        // Build lookup: (path, method) → endpoint metadata
        var metaLookup = context.DescriptionGroups
            .SelectMany(g => g.Items)
            .Where(d => d.RelativePath is not null)
            .ToDictionary(
                d => ("/" + d.RelativePath!, d.HttpMethod ?? ""),
                d => d.ActionDescriptor.EndpointMetadata,
                PathMethodComparer.Instance);

        foreach (var (path, item) in document.Paths)
        {
            foreach (var (method, operation) in item.Operations)
            {
                var key = (path, method.ToString());
                if (!metaLookup.TryGetValue(key, out var metadata))
                    continue;

                var requiresAuth = metadata.Any(m => m is AuthorizeAttribute);
                if (requiresAuth)
                {
                    operation.Security =
                    [
                        new OpenApiSecurityRequirement
                        {
                            [new OpenApiSecuritySchemeReference("Bearer")] = []
                        }
                    ];
                }
            }
        }

        return Task.CompletedTask;
    }

    private sealed class PathMethodComparer : IEqualityComparer<(string path, string method)>
    {
        public static readonly PathMethodComparer Instance = new();
        public bool Equals((string path, string method) x, (string path, string method) y) =>
            string.Equals(x.path, y.path, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.method, y.method, StringComparison.OrdinalIgnoreCase);
        public int GetHashCode((string path, string method) obj) =>
            HashCode.Combine(obj.path.ToLowerInvariant(), obj.method.ToLowerInvariant());
    }
}
