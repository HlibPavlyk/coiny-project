using Coiny.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace Coiny.Api.Controllers;

[ApiController]
[Route("_health")]
[Tags("System")]
public class HealthController : ControllerBase
{
    public record HealthModel(string Status, string Version);

    /// <summary>Liveness probe — always returns 200 while the process is up.</summary>
    [HttpGet]
    public Task<Result<HealthModel>> Get() =>
        Task.FromResult(Result.Success(new HealthModel("ok", "1.0")));
}
