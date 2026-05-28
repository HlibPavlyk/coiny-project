using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Moderation.BanUser;

/// <summary>
/// Admin bans a user. <see cref="UserId"/> comes from the route; the controller composes it via
/// <c>request with { UserId = id }</c>. Banning runs the auto-cleanup pipeline (cancel active lots,
/// cancel in-flight payments) — see <c>BanUserHandler</c>.
/// </summary>
public record BanUserRequest(Guid UserId, string Reason) : IRequest<Result>;
