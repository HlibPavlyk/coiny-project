using Coiny.Application.Abstractions.ExternalServices.Email;
using Microsoft.Extensions.Options;
using Resend;

namespace Coiny.Infrastructure.ExternalServices.Resend;

public class ResendEmailSender(IResend resend, IOptions<ResendOptions> options) : IEmailSender
{
    public Task SendVerificationEmailAsync(string toAddress, string verificationUrl, CancellationToken ct) =>
        Send(toAddress, EmailTemplates.VerificationEmail(verificationUrl), ct);

    public Task SendWonPayEmailAsync(
        string toAddress,
        string lotTitle,
        long amountUahKopiykas,
        DateTime dueAtUtc,
        string paymentUrl,
        CancellationToken ct) =>
        Send(toAddress,
            EmailTemplates.WonPayEmail(lotTitle, amountUahKopiykas, dueAtUtc, paymentUrl, reminder: false),
            ct);

    public Task SendWonPayReminderEmailAsync(
        string toAddress,
        string lotTitle,
        long amountUahKopiykas,
        DateTime dueAtUtc,
        string paymentUrl,
        CancellationToken ct) =>
        Send(toAddress,
            EmailTemplates.WonPayEmail(lotTitle, amountUahKopiykas, dueAtUtc, paymentUrl, reminder: true),
            ct);

    public Task SendShipmentStatusEmailAsync(
        string toAddress,
        string lotTitle,
        string ttn,
        string status,
        CancellationToken ct) =>
        Send(toAddress, EmailTemplates.ShipmentStatusEmail(lotTitle, ttn, status), ct);

    private Task Send(string toAddress, EmailContent content, CancellationToken ct) =>
        resend.EmailSendAsync(new EmailMessage
        {
            From = options.Value.FromAddress,
            To = [toAddress],
            Subject = content.Subject,
            HtmlBody = content.HtmlBody,
            TextBody = content.TextBody,
        }, ct);
}
