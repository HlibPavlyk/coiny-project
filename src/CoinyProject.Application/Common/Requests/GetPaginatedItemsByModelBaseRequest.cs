namespace CoinyProject.Application.Common.Requests;

public record GetPaginatedItemsByModelBaseRequest
{
    public GetPaginatedItemsBaseRequest Paginate { get; init; }
}
