namespace Coiny.Application.Common.Querying;

/// <summary>Describes one sort criterion: a column name (validated by the handler) and direction.</summary>
public record SortByModel(string ColumnName, SortDirection Direction);
