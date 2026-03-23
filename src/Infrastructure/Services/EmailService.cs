using Domain.Interfaces.Services;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration)
    {
        _host = configuration["Email:Host"] ?? "localhost";
        _port = int.Parse(configuration["Email:Port"] ?? "1025");
        _fromEmail = configuration["Email:FromEmail"] ?? "noreply@cleanarch.com";
        _fromName = configuration["Email:FromName"] ?? "CleanArch App";
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_fromName, _fromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();

        // Mailpit não usa SSL — desabilita para desenvolvimento
        await client.ConnectAsync(_host, _port, useSsl: false, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(quit: true, ct);
    }
}