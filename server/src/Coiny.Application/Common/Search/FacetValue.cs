namespace Coiny.Application.Common.Search;

/// <summary>One facet bucket: a value present in the results and how many items carry it.</summary>
public sealed record FacetValue(string Value, int Count);
