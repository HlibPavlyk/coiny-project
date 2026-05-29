namespace Coiny.Application.Features.Demo;

/// <summary>
/// Feature flag gating the <c>/api/v1/demo/*</c> endpoint surface. Default <c>false</c> — in
/// production these endpoints respond with 404 even to authenticated admins. Enable only on the
/// thesis-demo VPS via <c>"DemoMode:Enabled": true</c> in appsettings.
///
/// <para>
/// Why a flag, not a separate build/branch? The demo endpoints exercise the same code path
/// production runs, so we want them present in the production assembly to prove parity — but
/// disabled by default to prevent accidental misuse.
/// </para>
/// </summary>
public class DemoModeOptions
{
    public const string Section = "DemoMode";

    public bool Enabled { get; init; } = false;
}
