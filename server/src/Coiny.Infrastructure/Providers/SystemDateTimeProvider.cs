using Coiny.Application.Abstractions.Providers;

namespace Coiny.Infrastructure.Providers;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
