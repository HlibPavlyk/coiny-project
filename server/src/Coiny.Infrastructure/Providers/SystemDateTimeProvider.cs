using Coiny.Application.Abstractions.Infrastructure.Providers;

namespace Coiny.Infrastructure.Providers;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
