using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Coiny.Api.Pipeline;

public static class CookieAuthHelper
{
    public static void IssueAuthCookie(HttpContext ctx, string jwt, IConfiguration configuration)
    {
        int lifetimeMinutes = configuration.GetValue("Jwt:AccessTokenLifetimeMinutes", 60);
        string? cookieDomain = configuration["Auth:CookieDomain"];

        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            MaxAge = TimeSpan.FromSeconds(lifetimeMinutes * 60),
        };

        if (!string.IsNullOrEmpty(cookieDomain))
            options.Domain = cookieDomain;

        ctx.Response.Cookies.Append("coiny_auth", jwt, options);
    }

    public static void ClearAuthCookie(HttpContext ctx)
    {
        ctx.Response.Cookies.Append("coiny_auth", string.Empty, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            MaxAge = TimeSpan.Zero,
        });
    }
}
