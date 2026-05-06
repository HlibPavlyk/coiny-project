using System.Text.Json;
using Coiny.Application.Abstractions.Data;
using Coiny.Application.Abstractions.Email;
using Coiny.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Coiny.Infrastructure.Jobs;

public class EmailOutboxFlushJob(
    IApplicationDbContext db,
    IEmailSender emailSender,
    IConfiguration configuration,
    ILogger<EmailOutboxFlushJob> logger)
{
    private const int _batchSize = 100;

    public async Task RunAsync(CancellationToken ct)
    {
        List<EmailOutboxEvent> pending = await db.EmailOutboxEvents
            .Where(e => e.ProcessedAt == null)
            .OrderBy(e => e.Id)
            .Take(_batchSize)
            .ToListAsync(ct);

        foreach (EmailOutboxEvent evt in pending)
        {
            try
            {
                await DispatchAsync(evt, ct);
                evt.ProcessedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                evt.AttemptCount++;
                evt.LastError = ex.Message;
                logger.LogWarning(ex, "EmailOutboxFlushJob: failed to process event {Id} ({EventType}), attempt {Attempt}",
                    evt.Id, evt.EventType, evt.AttemptCount);
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task DispatchAsync(EmailOutboxEvent evt, CancellationToken ct)
    {
        switch (evt.EventType)
        {
            case "EmailVerificationRequested":
                await HandleVerificationAsync(evt, ct);
                break;

            default:
                // Sprint 3: AuctionWonPayWithin96h, ShipmentStatusChanged
                logger.LogInformation("EmailOutboxFlushJob: no handler for EventType={EventType} — skipping (TODO sprint 3)", evt.EventType);
                break;
        }
    }

    private async Task HandleVerificationAsync(EmailOutboxEvent evt, CancellationToken ct)
    {
        using var doc = JsonDocument.Parse(evt.Payload);

        string toAddress = doc.RootElement.GetProperty("toAddress").GetString()
            ?? throw new InvalidOperationException($"Event {evt.Id}: missing toAddress in payload.");

        string token = doc.RootElement.GetProperty("token").GetString()
            ?? throw new InvalidOperationException($"Event {evt.Id}: missing token in payload.");

        string frontendBase = configuration["Frontend:BaseUrl"]?.TrimEnd('/')
            ?? throw new InvalidOperationException("Frontend:BaseUrl is not configured.");

        string verificationUrl = $"{frontendBase}/verify-email?token={Uri.EscapeDataString(token)}";

        await emailSender.SendVerificationEmailAsync(toAddress, verificationUrl, ct);
    }
}
