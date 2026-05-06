namespace Coiny.Application.Abstractions.Email;

public interface IEmailSender
{
    Task SendVerificationEmailAsync(string toAddress, string verificationUrl, CancellationToken ct);
}
