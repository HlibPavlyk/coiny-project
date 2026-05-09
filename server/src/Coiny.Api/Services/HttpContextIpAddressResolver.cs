using System.Net;
using Coiny.Application.Abstractions.Http;

namespace Coiny.Api.Services;

public class HttpContextIpAddressResolver(IHttpContextAccessor httpContextAccessor) : IIpAddressResolver
{
    public string? Resolve()
    {
        HttpContext? ctx = httpContextAccessor.HttpContext;
        if (ctx is null)
            return null;

        // Behind Coolify/Caddy: X-Forwarded-For = "client, proxy1, proxy2"
        // Take the first non-private address — public IP that is closest to the client.
        if (ctx.Request.Headers.TryGetValue("X-Forwarded-For", out var fwd) && fwd.Count > 0)
        {
            string raw = fwd.ToString();
            foreach (string candidate in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (IPAddress.TryParse(candidate, out IPAddress? parsed) && !IsPrivate(parsed))
                    return parsed.ToString();
            }
        }

        return ctx.Connection.RemoteIpAddress?.ToString();
    }

    private static bool IsPrivate(IPAddress ip)
    {
        if (IPAddress.IsLoopback(ip))
            return true;

        if (ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            return false;

        byte[] b = ip.GetAddressBytes();
        // 10.0.0.0/8, 172.16.0.0/12, 192.168.0.0/16
        return b[0] == 10
            || (b[0] == 172 && b[1] >= 16 && b[1] <= 31)
            || (b[0] == 192 && b[1] == 168);
    }
}
