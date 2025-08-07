using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using NaviGoApi.Application.Settings;

namespace NaviGoApi.Application.Services
{
	public class EmailService : IEmailService
	{
		private readonly SmtpSettings _smtpSettings;

		public EmailService(IOptions<SmtpSettings> smtpOptions)
		{
			_smtpSettings = smtpOptions.Value;
		}

		public async Task SendVerificationEmailAsync(string toEmail, string verificationLink)
		{
			var htmlBody = GenerateVerificationEmailHtml(verificationLink);

			var message = new MailMessage
			{
				From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.DisplayName),
				Subject = "Verify Your NaviGo Account",
				Body = htmlBody,
				IsBodyHtml = true
			};

			message.To.Add(toEmail);

			using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
			{
				Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
				EnableSsl = _smtpSettings.EnableSsl
			};

			await client.SendMailAsync(message);
		}

		private string GenerateVerificationEmailHtml(string verificationLink)
		{
			return $@"
				<html>
				<head>
					<style>
						.container {{
							font-family: Arial, sans-serif;
							max-width: 600px;
							margin: auto;
							padding: 20px;
							border: 1px solid #ddd;
							border-radius: 10px;
							background-color: #f9f9f9;
						}}
						.button {{
							display: inline-block;
							padding: 10px 20px;
							margin-top: 20px;
							font-size: 16px;
							color: white;
							background-color: #007bff;
							text-decoration: none;
							border-radius: 5px;
						}}
					</style>
				</head>
				<body>
					<div class='container'>
						<h2>Welcome to NaviGo!</h2>
						<p>Thanks for signing up. Please verify your account by clicking the button below:</p>
						<a href='{verificationLink}' class='button'>Verify Account</a>
						<p>If you did not request this, you can safely ignore this email.</p>
						<p>– NaviGo Team</p>
					</div>
				</body>
				</html>";
		}
	}
}
