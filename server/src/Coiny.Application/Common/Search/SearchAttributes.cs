namespace Coiny.Application.Common.Search;

/// <summary>
/// Marks a search-document property as full-text searchable in the index.
/// NOTE: for searchable attributes, declaration order is the ranking priority in Meilisearch
/// (earlier = higher weight). The search adapter derives the list in property declaration order,
/// so keep the most important fields first.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SearchableAttribute : Attribute;

/// <summary>Marks a property as filterable (usable in facet/where clauses). Order is irrelevant.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class FilterableAttribute : Attribute;

/// <summary>Marks a property as sortable. Order is irrelevant. The camelCase property name is the sort column.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SortableAttribute : Attribute;
