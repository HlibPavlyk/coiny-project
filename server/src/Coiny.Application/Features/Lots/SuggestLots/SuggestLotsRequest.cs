using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Lots.SuggestLots;

/// <summary>
/// Lightweight autocomplete query for the header search bar. Returns up to 8 public lots ranked by
/// Meilisearch relevance. Read-only, anonymous — the publicly visible status set is enforced server-side.
/// </summary>
public record SuggestLotsRequest(string Q) : IRequest<Result<IReadOnlyList<LotSuggestItem>>>;
