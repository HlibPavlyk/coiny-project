using System.Text.Json;

namespace Coiny.Application.Features.Lots.Validators;

/// <summary>
/// Light-touch validation for the <c>Attributes</c> JSONB blob.
/// Per <c>/docs/01-data-model.md</c> §3 the schemas are open-ended (unknown keys allowed),
/// so we only check the value is a JSON object — subcategory-specific shape is enforced softly
/// at render-time by the frontend's <c>CategoryAttributeForm</c>.
/// </summary>
internal static class LotAttributesValidator
{
    public static bool IsJsonObject(JsonElement element) =>
        element.ValueKind == JsonValueKind.Object;
}
