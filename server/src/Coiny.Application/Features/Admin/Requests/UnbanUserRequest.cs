using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Admin.Requests;

/// <summary>
/// Admin lifts a ban. <see cref="UserId"/> comes from the route. Reverses only the ban flag and
/// metadata — cancelled lots and payments are terminal and are not restored.
/// </summary>
public record UnbanUserRequest(Guid UserId) : IRequest<Result>;
