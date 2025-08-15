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
		public async Task SendResetPasswordPinEmailAsync(string toEmail, string pin)
		{
			var htmlBody = GenerateResetPasswordPinEmailHtml(pin);

			var message = new MailMessage
			{
				From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.DisplayName),
				Subject = "Your NaviGo Password Reset PIN",
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
		private string GenerateResetPasswordPinEmailHtml(string pin)
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
                    text-align: center;
                }}
                .pin {{
                    font-size: 32px;
                    font-weight: bold;
                    color: #dc3545;
                    margin: 20px 0;
                    letter-spacing: 5px;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <h2>Password Reset PIN</h2>
                <p>Your password reset PIN is:</p>
                <div class='pin'>{pin}</div>
                <p>This PIN is valid for 15 minutes.</p>
                <p>If you did not request a password reset, please ignore this email.</p>
                <p>– NaviGo Team</p>
            </div>
        </body>
        </html>";
		}
		private string GenerateSuspendEmailHtml(string email)
		{
			return $@"
    <html>
    <head>
        <style>
            body {{
                font-family: Arial, sans-serif;
                background-color: #f4f4f4;
                margin: 0;
                padding: 0;
            }}
            .container {{
                max-width: 600px;
                margin: 50px auto;
                padding: 30px;
                background-color: #ffffff;
                border-radius: 12px;
                box-shadow: 0 4px 10px rgba(0,0,0,0.1);
                text-align: center;
            }}
            h2 {{
                color: #dc3545;
                margin-bottom: 20px;
            }}
            p {{
                color: #333333;
                font-size: 16px;
                line-height: 1.5;
            }}
            .highlight {{
                font-weight: bold;
                color: #007bff;
            }}
            .footer {{
                margin-top: 30px;
                font-size: 14px;
                color: #888888;
            }}
            .button {{
                display: inline-block;
                margin-top: 25px;
                padding: 12px 25px;
                background-color: #dc3545;
                color: #ffffff;
                text-decoration: none;
                border-radius: 5px;
                font-weight: bold;
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <h2>Account Suspended</h2>
            <p>Dear <span class='highlight'>{email}</span>,</p>
            <p>We noticed that after logging in once, multiple requests to our services were made from different IP addresses in a short period of time.</p>
            <p>As a precaution, your account has been temporarily suspended to protect your data and prevent misuse.</p>
            <p>If you believe this is a mistake or need assistance, please contact our support team immediately.</p>
            <a href='mailto:support@navigo.com' class='button'>Contact Support</a>
            <div class='footer'>
                – NaviGo Team
            </div>
        </div>
    </body>
    </html>";
		}

		public async Task SendSuspendEmailAsync(string toEmail)
		{
			var htmlBody = GenerateSuspendEmailHtml(toEmail);
			var message = new MailMessage
			{
				From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.DisplayName),
				Subject = "Suspended Your NaviGo Account",
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
	}

}
