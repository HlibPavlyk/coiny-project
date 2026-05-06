namespace Coiny.Application.Common.Results;

/// <summary>Discriminates the category of an Error so ResultTransformFilter can map to the correct HTTP status.</summary>
public enum ErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Forbidden = 5,
    Internal = 6,
}
