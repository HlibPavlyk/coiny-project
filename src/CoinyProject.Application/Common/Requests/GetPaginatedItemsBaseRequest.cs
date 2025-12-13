using System.ComponentModel;
using CoinyProject.Application.Common.Querying.Models;

namespace CoinyProject.Application.Common.Requests;

public record GetPaginatedItemsBaseRequest : GetItemsBaseRequest, IPaginate
{
    public int Offset { get; init; }
    [DefaultValue(10)] public int Count { get; init; } = 10;
}