namespace Reservation.API.Services.Email;

public interface IEmailSender
{
    Task SendTicketEmailAsync(
        string toEmail,
        string recipientName,
        string movieTitle,
        IEnumerable<(string FileName, byte[] Content)> attachments,
        CancellationToken cancellationToken = default);
}
