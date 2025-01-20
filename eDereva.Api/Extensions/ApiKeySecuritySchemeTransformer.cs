using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace eDereva.Api.Extensions;

public sealed class ApiKeySecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    private readonly string _apiKey;

    public ApiKeySecuritySchemeTransformer(IConfiguration configuration)
    {
        _apiKey = configuration["ApiSettings:ApiKey"] ??
                  throw new ArgumentException("ApiSettings:ApiKey configuration is missing");
    }

    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_apiKey))
        {
            var apiKeySecurityScheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                Name = "X-API-KEY", // The header name for the API key
                In = ParameterLocation.Header,
                Description = "API Key required for accessing this API"
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes["ApiKey"] = apiKeySecurityScheme;
            var apiKeyRequirement = new OpenApiSecurityRequirement
            {
                [apiKeySecurityScheme] = Array.Empty<string>()
            };
            foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
                operation.Value.Security.Add(apiKeyRequirement);
        }

        await Task.CompletedTask;
    }
}