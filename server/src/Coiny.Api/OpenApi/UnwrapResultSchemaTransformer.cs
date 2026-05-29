using Coiny.Application.Common.Results;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Coiny.Api.OpenApi;

/// <summary>
/// Replaces the generated schema for <see cref="Result{TValue}"/> with the inner T schema,
/// and collapses non-generic <see cref="Result"/> to an empty schema.
/// </summary>
public class UnwrapResultSchemaTransformer : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        Type type = context.JsonTypeInfo.Type;

        if (type == typeof(Result))
        {
            schema.Properties?.Clear();
            schema.Type = null;
            return;
        }

        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Result<>))
            return;

        Type innerType = type.GetGenericArguments()[0];
        OpenApiSchema innerSchema = await context.GetOrCreateSchemaAsync(innerType, null, cancellationToken);

        schema.Type = innerSchema.Type;
        schema.Format = innerSchema.Format;
        schema.Properties = innerSchema.Properties;
        schema.Items = innerSchema.Items;
        schema.Enum = innerSchema.Enum;
        schema.Description = innerSchema.Description;
    }
}
