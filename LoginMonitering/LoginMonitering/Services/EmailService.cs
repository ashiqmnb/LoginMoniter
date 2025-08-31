
using System.Net.Mail;
using System.Net;
using static System.Net.WebRequestMethods;

namespace LoginMonitering.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<bool> SendEmail(string receiverMail, string otp)
        {
            try
            {
                string host = _config["EmailSettings:Host"];
                int port = _config.GetValue<int>("EmailSettings:Port", 587);
                string senderEmail = _config["EmailSettings:Username"];
                string password = _config["EmailSettings:Password"];

                SmtpClient smtpClient = new SmtpClient(host);
                smtpClient.Port = port;
                smtpClient.Credentials = new NetworkCredential(senderEmail, password);
                smtpClient.EnableSsl = true;
                var n = host;

                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(senderEmail);
                mailMessage.To.Add(receiverMail);

                string subject = "Your OTP for Email Verification";
                var body = $@"
				<!DOCTYPE html>
				<html>
				<head>
					<style>
						body {{ font-family: Arial, sans-serif; text-align: center; background-color: #f4f4f4; padding: 20px; }}
						.container {{ max-width: 500px; background: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1); margin: auto; }}
						.otp {{ font-size: 24px; font-weight: bold; color: #333; padding: 10px; background: #f4f4f4; display: inline-block; border-radius: 5px; }}
						.footer {{ margin-top: 20px; font-size: 12px; color: #666; }}
					</style>
				</head>
				<body class='container'>
					<h2>ParkEase Email Verification</h2>
					<p>Use the following OTP to verify your email address:</p>
					<p class='otp'>{otp}</p>
					<p>This OTP is valid for 5 minutes. Do not share it with anyone.</p>
					<p class='footer'>If you didn’t request this, you can ignore this email.</p>
				</body>
				</html>";


                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;

                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }
    }
}
