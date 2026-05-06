namespace Coiny.Infrastructure.ExternalServices.Resend;

internal sealed record EmailContent(string Subject, string HtmlBody, string TextBody);

internal static class EmailTemplates
{
    internal static EmailContent VerificationEmail(string verificationUrl) => new(
        Subject: "Verify your Coiny email address",
        HtmlBody: $"""
            <p>Welcome to Coiny!</p>
            <p>Click the link below to verify your email address:</p>
            <p><a href="{verificationUrl}">{verificationUrl}</a></p>
            <p>This link expires in 24 hours.</p>
            """,
        TextBody: $"Welcome to Coiny!\n\nVerify your email address:\n{verificationUrl}\n\nThis link expires in 24 hours."
    );
}
