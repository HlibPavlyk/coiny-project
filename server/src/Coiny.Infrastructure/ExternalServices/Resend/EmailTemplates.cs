using System.Globalization;

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

    internal static EmailContent WonPayEmail(
        string lotTitle,
        long amountUahKopiykas,
        DateTime dueAtUtc,
        string paymentUrl,
        bool reminder)
    {
        string formattedAmount = FormatUah(amountUahKopiykas);
        string formattedDue = dueAtUtc.ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture);
        string subjectPrefix = reminder ? "REMINDER: " : string.Empty;

        return new EmailContent(
            Subject: $"{subjectPrefix}You won \"{lotTitle}\" on Coiny — pay within 96h",
            HtmlBody: $"""
                <p>Congratulations — you won the auction for <strong>{lotTitle}</strong>.</p>
                <p><strong>Amount due:</strong> {formattedAmount}<br/>
                <strong>Pay by:</strong> {formattedDue}</p>
                <p><a href="{paymentUrl}">Complete payment</a></p>
                <p>If you don't pay by the deadline, the order is cancelled automatically and the lot returns to the marketplace.</p>
                """,
            TextBody: $"""
                Congratulations — you won the auction for "{lotTitle}".

                Amount due: {formattedAmount}
                Pay by: {formattedDue}

                Complete payment: {paymentUrl}

                If you don't pay by the deadline, the order is cancelled automatically.
                """
        );
    }

    internal static EmailContent ShipmentStatusEmail(string lotTitle, string ttn, string status) => new(
        Subject: $"\"{lotTitle}\" — {status}",
        HtmlBody: $"""
            <p>An update on your purchase of <strong>{lotTitle}</strong>:</p>
            <p><strong>Status:</strong> {status}<br/>
            <strong>Nova Poshta TTN:</strong> {ttn}</p>
            <p>You can track the parcel on the Nova Poshta website.</p>
            """,
        TextBody: $"""
            An update on your purchase of "{lotTitle}":

            Status: {status}
            Nova Poshta TTN: {ttn}

            You can track the parcel on the Nova Poshta website.
            """
    );

    // Formats kopiykas as "1,234.50 UAH" using the en-US locale — matches the rest of the UI
    // (English-first per user preference). The kopiyka→UAH conversion is integer-division-safe.
    private static string FormatUah(long kopiykas) =>
        string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:N2} UAH", kopiykas / 100m);
}
