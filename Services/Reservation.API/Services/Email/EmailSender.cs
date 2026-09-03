using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Reservation.API.Settings;

namespace Reservation.API.Services.Email;

public class EmailSender(IOptions<EmailSettings> options) : IEmailSender
{
    private readonly EmailSettings _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));

    public async Task SendTicketEmailAsync(
        string toEmail,
        string recipientName,
        string movieTitle,
        IEnumerable<(string FileName, byte[] Content)> attachments,
        CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(new MailboxAddress(recipientName, toEmail));
        message.Subject = $"Your CineMATF ticket(s) for {movieTitle}";

        var builder = new BodyBuilder
        {
            TextBody = $"""
                Hi {recipientName},

                Thanks for your purchase! Your ticket(s) for {movieTitle} are attached to this email.
                You can also download them any time from your reservations page.

                Enjoy the movie!
                CineMATF
                """
        };

        foreach (var attachment in attachments)
        {
            builder.Attachments.Add(attachment.FileName, attachment.Content);
        }

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);
        await client.AuthenticateAsync(_settings.SenderEmail, _settings.SenderPassword, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
