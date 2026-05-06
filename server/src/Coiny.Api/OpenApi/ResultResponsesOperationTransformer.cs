using Coiny.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Coiny.Api.OpenApi;

/// <summary>
/// For operations returning <see cref="Result"/> or <see cref="Result{TValue}"/>:
/// replaces the raw 200 schema with the unwrapped inner type and adds standard
/// 400/401/403/404/409/500 ProblemDetails responses.
/// </summary>
public class ResultResponsesOperationTransformer : IOpenApiOperationTransformer
{
    private static readonly string[] ErrorStatuses = ["400", "401", "403", "404", "409", "500"];

    private static readonly Dictionary<string, string> StatusDescriptions = new()
    {
        ["400"] = "Validation Error",
        ["401"] = "Unauthorized",
        ["403"] = "Forbidden",
        ["404"] = "Resource Not Found",
        ["409"] = "Conflict",
        ["500"] = "Internal Server Error",
    };

    public async Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor is not ControllerActionDescriptor descriptor)
            return;

        Type returnType = UnwrapTask(descriptor.MethodInfo.ReturnType);

        bool isNonGenericResult = returnType == typeof(Result);
        bool isGenericResult = returnType.IsGenericType &&
                               returnType.GetGenericTypeDefinition() == typeof(Result<>);

        if (!isNonGenericResult && !isGenericResult)
            return;

        // Replace 200 response with correct schema
        operation.Responses ??= new OpenApiResponses();

        if (isNonGenericResult)
        {
            operation.Responses["200"] = new OpenApiResponse { Description = "Success" };
        }
        else
        {
            Type innerType = returnType.GetGenericArguments()[0];
            OpenApiSchema innerSchema = await context.GetOrCreateSchemaAsync(innerType, null, cancellationToken);
            context.Document?.AddComponent(innerType.Name, innerSchema);

            operation.Responses["200"] = new OpenApiResponse
            {
                Description = "Success",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchemaReference(innerType.Name, context.Document),
                    },
                },
            };
        }

        // Add standard ProblemDetails error responses
        OpenApiSchema problemSchema = await context.GetOrCreateSchemaAsync(typeof(ProblemDetails), null, cancellationToken);
        context.Document?.AddComponent("ProblemDetails", problemSchema);

        foreach (string status in ErrorStatuses)
        {
            operation.Responses[status] = new OpenApiResponse
            {
                Description = StatusDescriptions[status],
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/problem+json"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchemaReference("ProblemDetails", context.Document),
                    },
                },
            };
        }
    }

    private static Type UnwrapTask(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
            return type.GetGenericArguments()[0];
        return type;
    }
}
