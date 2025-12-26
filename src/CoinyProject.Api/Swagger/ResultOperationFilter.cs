using CoinyProject.Application.Common.Results;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CoinyProject.Api.Swagger;

public class ResultOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var returnType = context.MethodInfo.ReturnType;

        // Unwrap Task<T>
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            returnType = returnType.GetGenericArguments()[0];

        // Check if it's Result (non-generic)
        if (returnType == typeof(Result))
        {
            ConfigureResultResponses(operation, null, context);
            return;
        }

        // Check if it's Result<T>
        if (!returnType.IsGenericType || returnType.GetGenericTypeDefinition() != typeof(Result<>))
            return;

        var innerType = returnType.GetGenericArguments()[0];
        ConfigureResultResponses(operation, innerType, context);
    }

    private static void ConfigureResultResponses(OpenApiOperation operation, Type? innerType, OperationFilterContext context)
    {
        operation.Responses.Clear();

        if (innerType != null)
        {
            var schema = context.SchemaGenerator.GenerateSchema(innerType, context.SchemaRepository);
            operation.Responses.Add("200", new OpenApiResponse
            {
                Description = "Success",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new() { Schema = schema }
                }
            });
        }
        else
        {
            operation.Responses.Add("200", new OpenApiResponse { Description = "Success" });
        }
    }
}
