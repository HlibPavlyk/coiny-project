using System.ComponentModel;
using CoinyProject.Application.Models;

namespace CoinyProject.Application.Requests;

public record GetPaginatedItemsBaseRequest : GetItemsBaseRequest, IPaginate
{
    public int Offset { get; init; }
    [DefaultValue(10)] public int Count { get; init; } = 10;
}