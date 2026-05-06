namespace Coiny.Application.Common.Requests;

/// <summary>Base for non-paginated list requests. SearchText and ColumnsSearch are inert for EF-backed endpoints.</summary>
public record GetItemsBaseRequest
{
    public string? SearchText { get; init; }
    public Dictionary<string, string>? ColumnsSearch { get; init; }
}
