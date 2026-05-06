using Coiny.Application.Abstractions.Email;
using Microsoft.Extensions.Options;
using Resend;

namespace Coiny.Infrastructure.ExternalServices.Resend;

public class ResendEmailSender(IResend resend, IOptions<ResendOptions> options) : IEmailSender
{
    public Task SendVerificationEmailAsync(string toAddress, string verificationUrl, CancellationToken ct)
    {
        EmailContent content = EmailTemplates.VerificationEmail(verificationUrl);
        return Send(toAddress, content, ct);
    }

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
