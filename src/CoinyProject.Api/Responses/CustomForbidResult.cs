using Microsoft.AspNetCore.Mvc;

namespace CoinyProject.Api.Responses;

public class CustomForbidResult : IActionResult
{
    private readonly string _message;

    public CustomForbidResult(string message)
    {
        _message = message;
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        response.StatusCode = StatusCodes.Status403Forbidden;

        response.ContentType = "application/json";
        await response.WriteAsJsonAsync(_message);
    }
}