using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Coiny.Api.OpenApi;

/// <summary>Declares the Bearer security scheme so Scalar's Authorize panel is enabled.</summary>
public class JwtBearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                In = ParameterLocation.Header,
                BearerFormat = "Json Web Token",
            },
        };
        return Task.CompletedTask;
    }
}
