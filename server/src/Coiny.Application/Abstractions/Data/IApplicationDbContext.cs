using Coiny.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Coiny.Application.Abstractions.Data;

/// <summary>EF Core seam exposed to Application handlers. Concrete implementation lives in Infrastructure.</summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Category> Categories { get; }
    DbSet<Lot> Lots { get; }
    DbSet<LotImage> LotImages { get; }
    DbSet<Bid> Bids { get; }
    DbSet<EmailVerificationToken> EmailVerificationTokens { get; }
    DbSet<OutboxEvent> OutboxEvents { get; }
    DbSet<EmailOutboxEvent> EmailOutboxEvents { get; }
    DbSet<Report> Reports { get; }

    /// <summary>Exposed so handlers can wrap multi-step writes in an explicit transaction.</summary>
    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
