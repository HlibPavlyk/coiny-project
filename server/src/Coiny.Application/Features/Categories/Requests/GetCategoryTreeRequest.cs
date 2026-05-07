using Coiny.Application.Common.Results;
using Coiny.Application.Features.Categories.Models;
using MediatR;

namespace Coiny.Application.Features.Categories.Requests;

public record GetCategoryTreeRequest : IRequest<Result<CategoryTreeModel>>;
