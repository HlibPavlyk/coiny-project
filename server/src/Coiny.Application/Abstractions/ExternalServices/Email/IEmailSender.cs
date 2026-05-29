namespace Coiny.Application.Abstractions.ExternalServices.Email;

public interface IEmailSender
{
    Task SendVerificationEmailAsync(string toAddress, string verificationUrl, CancellationToken ct);

    /// <summary>
    /// "You won {lot} — pay within 96h". Triggered by <c>AuctionCloseJob</c> via the
    /// <c>AuctionWonPayWithin96h</c> outbox event.
    /// </summary>
    Task SendWonPayEmailAsync(
        string toAddress,
        string lotTitle,
        long amountUahKopiykas,
        DateTime dueAtUtc,
        string paymentUrl,
        CancellationToken ct);

    /// <summary>
    /// Same template as <see cref="SendWonPayEmailAsync"/> with a "REMINDER:" subject prefix.
    /// Triggered by <c>PaymentReminderJob</c> at the 48h mark.
    /// </summary>
    Task SendWonPayReminderEmailAsync(
        string toAddress,
        string lotTitle,
        long amountUahKopiykas,
        DateTime dueAtUtc,
        string paymentUrl,
        CancellationToken ct);

    /// <summary>
    /// "{Lot} is {status}". Triggered by <c>NovaPoshtaPollingJob</c> for the user-visible
    /// transitions only (InTransit / Delivered).
    /// </summary>
    Task SendShipmentStatusEmailAsync(
        string toAddress,
        string lotTitle,
        string ttn,
        string status,
        CancellationToken ct);
}
