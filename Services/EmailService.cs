using System.Net;
using System.Net.Mail;

namespace backend.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _fromEmail;
    private readonly string _password;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        // Charger les paramètres depuis les variables d'environnement
        _smtpServer = Environment.GetEnvironmentVariable("EMAIL_SMTP_SERVER") ?? "smtp.gmail.com";
        _smtpPort = int.Parse(Environment.GetEnvironmentVariable("EMAIL_SMTP_PORT") ?? "587");
        _fromEmail = Environment.GetEnvironmentVariable("EMAIL_SMTP_USER") ?? "fadakcare@gmail.com";
        _password = Environment.GetEnvironmentVariable("EMAIL_SMTP_PASS") ?? "sonnypygkrhapaxi";
    }

    public async Task SendConfirmationEmail(string toEmail, string username)
    {
        using var client = new SmtpClient(_smtpServer, _smtpPort)
        {
            Credentials = new NetworkCredential(_fromEmail, _password),
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 10000
        };

        var message = new MailMessage
        {
            From = new MailAddress(_fromEmail, "Équipe FadakCare"),
            Subject = "Confirmation de votre inscription",
            //Body = BuildEmailBody(username),
            IsBodyHtml = true,
            Priority = MailPriority.Normal
        };

        message.To.Add(new MailAddress(toEmail));

        message.Headers.Add("X-Mailer", "FadakCare Mail Service");
        message.Headers.Add("X-Priority", "3");

        await client.SendMailAsync(message);
    }
}