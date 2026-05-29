using Hangfire.Dashboard;

namespace Coiny.Api.Pipeline;

/// <summary>
/// Restricts Hangfire dashboard access (<c>/hangfire</c>) to authenticated users carrying the given
/// role on their identity claims. The standard <c>[Authorize]</c> attribute does not apply here
/// because Hangfire's dashboard is mounted via <c>UseHangfireDashboard</c> at the OWIN level, not
/// the MVC controller pipeline — it needs an <see cref="IDashboardAuthorizationFilter"/>.
///
/// <para>
/// In dev the dashboard is left wide open (see <c>Program.cs</c>) — having to log in just to peek
/// at recurring jobs slows iteration. In prod this filter is mandatory.
/// </para>
/// </summary>
public class HangfireRoleDashboardFilter(string role) : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole(role);
    }
}
