using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Categories.GetCategoryTree;

public record GetCategoryTreeRequest : IRequest<Result<CategoryTreeModel>>;
