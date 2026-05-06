using Coiny.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Abstractions.Data;

/// <summary>EF Core seam exposed to Application handlers. Concrete implementation lives in Infrastructure.</summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Category> Categories { get; }
    DbSet<Lot> Lots { get; }
    DbSet<LotImage> LotImages { get; }
    DbSet<EmailVerificationToken> EmailVerificationTokens { get; }
    DbSet<OutboxEvent> OutboxEvents { get; }
    DbSet<EmailOutboxEvent> EmailOutboxEvents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
