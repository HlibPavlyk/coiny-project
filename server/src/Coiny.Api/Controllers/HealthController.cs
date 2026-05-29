using Coiny.Application.Abstractions.Infrastructure.Data;
using Meilisearch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace Coiny.Api.Controllers;

/// <summary>
/// Liveness + dependency-probe endpoint. Returns 200 even when dependencies are degraded — Coolify
/// uses this for routing health, and a non-200 here would cause unnecessary container churn for a
/// transient Meilisearch hiccup. Each probe is wrapped in a 2-second timeout so a stuck dependency
/// can't tie the whole response up.
/// </summary>
[ApiController]
[Route("health")]
[Tags("System")]
public class HealthController(
    IApplicationDbContext db,
    MeilisearchClient meilisearch,
    IConfiguration configuration)
    : ControllerBase
{
    private static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(2);

    public record HealthModel(string Status, string Postgres, string Meilisearch, string Stripe);

    /// <summary>Returns overall + per-dependency status. Always 200 so the deploy platform doesn't restart on flake.</summary>
    [AllowAnonymous, HttpGet]
    public async Task<HealthModel> Get(CancellationToken ct)
    {
        string postgres = await ProbeAsync(async probeCt =>
        {
            // CanConnectAsync issues a lightweight SELECT 1; ideal for "is the DB reachable".
            bool ok = await db.Database.CanConnectAsync(probeCt);
            return ok ? "ok" : "down";
        }, ct);

        string meili = await ProbeAsync(async probeCt =>
        {
            MeiliSearchHealth h = await meilisearch.HealthAsync(probeCt);
            return string.Equals(h.Status, "available", StringComparison.OrdinalIgnoreCase) ? "ok" : "degraded";
        }, ct);

        string stripeKey = configuration["Stripe:SecretKey"] ?? "";
        string stripe = await ProbeAsync(async probeCt =>
        {
            if (string.IsNullOrEmpty(stripeKey))
                return "down";

            // BalanceService.GetAsync is the cheapest authenticated Stripe call — verifies the
            // secret key is valid and Stripe API is reachable. Per-request ApiKey via
            // RequestOptions avoids depending on the static StripeConfiguration.ApiKey, which is
            // only set after our StripeClient is first resolved from DI (lazy singleton).
            var balanceService = new BalanceService();
            _ = await balanceService.GetAsync(
                requestOptions: new RequestOptions { ApiKey = stripeKey },
                cancellationToken: probeCt);
            return "ok";
        }, ct);

        string overall = (postgres, meili, stripe) is ("ok", "ok", "ok") ? "ok" : "degraded";
        return new HealthModel(overall, postgres, meili, stripe);
    }

    /// <summary>
    /// Wraps a probe in a 2-second timeout and a try/catch that maps exceptions to "down". A
    /// timeout reports "degraded" rather than "down" because the dependency might be slow but
    /// alive — both buckets need operator attention but the distinction shows up in logs.
    /// </summary>
    private static async Task<string> ProbeAsync(Func<CancellationToken, Task<string>> probe, CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(ProbeTimeout);
        try
        {
            return await probe(cts.Token);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested && !ct.IsCancellationRequested)
        {
            return "degraded";
        }
        catch
        {
            return "down";
        }
    }
}
