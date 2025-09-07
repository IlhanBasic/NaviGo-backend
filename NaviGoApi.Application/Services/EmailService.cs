using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using NaviGoApi.Application.Settings;
using NaviGoApi.Domain.Entities;

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
		public async Task SendEmailCheckUserNotification(string toEmail, User user)
		{
			var htmlBody = GenerateCheckUserEmailHtml(user);
			var message = new MailMessage
			{
				From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.DisplayName),
				Subject = "New User Registration - Action Required",
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
		public async Task SendEmailCheckCompanyNotification(string toEmail, Company company)
		{
			var htmlBody = GenerateCheckCompanyEmailHtml(company);
			var message = new MailMessage
			{
				From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.DisplayName),
				Subject = "New Company Registration - Action Required",
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
		public async Task SendEmailPickupChangeNotification(string toEmail, Shipment shipment)
		{
			var htmlBody = GeneratePickupChangeEmailHtml(shipment);
			var message = new MailMessage
			{
				From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.DisplayName),
				Subject = "Pickup Time Updated for Your Shipment",
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
		public async Task SendEmailUserStatusNotification(string toEmail, User user)
		{
			var htmlBody = GenerateUserStatusEmailHtml(user);
			var message = new MailMessage
			{
				From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.DisplayName),
				Subject = "Your Account Status Has Changed",
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
		public async Task SendEmailCompanyStatusNotification(string toEmail, Company company)
		{
			var htmlBody = GenerateCompanyStatusEmailHtml(company);
			var message = new MailMessage
			{
				From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.DisplayName),
				Subject = "Your Company Status Has Changed",
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
            <p>Please check your dashboard</>
            <div class='footer'>
                – NaviGo Team
            </div>
        </div>
    </body>
    </html>";
		}
		public string GenerateCheckUserEmailHtml(User user)
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
            color: #28a745;
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
            background-color: #28a745;
            color: #ffffff;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h2>New User Registration</h2>
        <p>Dear Admin,</p>
        <p>A new user has registered with the email: <span class='highlight'>{user.Email}</span>.</p>
        <p>Please review the account and approve or reject the registration as appropriate.</p>
        <div class='footer'>
            – NaviGo Team
        </div>
    </div>
</body>
</html>";
		}
		public string GenerateCheckCompanyEmailHtml(Company company)
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
            color: #28a745;
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
            background-color: #28a745;
            color: #ffffff;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h2>New Company Registration</h2>
        <p>Dear Admin,</p>
        <p>A new company has been registered:</p>
        <p><span class='highlight'>{company.CompanyName}</span> (<span class='highlight'>{company.PIB}</span>)</p>
        <p>Please review and approve or reject this company registration.</p>
        <div class='footer'>
            – NaviGo Team
        </div>
    </div>
</body>
</html>";
		}
		public string GeneratePickupChangeEmailHtml(Shipment shipment)
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
            color: #ffc107;
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
            background-color: #ffc107;
            color: #ffffff;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h2>Pickup Time Updated</h2>
        <p>The pickup time for your shipment <span class='highlight'>#{shipment.Id}</span> has been changed.</p>
        <p><strong>Old Pickup:</strong> {shipment.PickupChange?.OldTime.ToLocalTime():f}</p>
        <p><strong>New Pickup:</strong> {shipment.PickupChange?.NewTime.ToLocalTime():f}</p>
        <p>Please check your shipment details for more information.</p>
        <div class='footer'>
            – NaviGo Team
        </div>
    </div>
</body>
</html>";
		}
		public string GenerateUserStatusEmailHtml(User user)
		{
			string statusText = user.UserStatus == UserStatus.Active ? "Active" : "Inactive";
			string statusColor = user.UserStatus == UserStatus.Active ? "#28a745" : "#dc3545"; // zeleno / crveno

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
            color: #007bff;
            margin-bottom: 20px;
        }}
        p {{
            color: #333333;
            font-size: 16px;
            line-height: 1.5;
        }}
        .status {{
            font-weight: bold;
            color: {statusColor};
            font-size: 18px;
        }}
        .footer {{
            margin-top: 30px;
            font-size: 14px;
            color: #888888;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h2>Account Status Update</h2>
        <p>Dear {user.FirstName} {user.LastName},</p>
        <p>Your account status has been updated. The current status of your account is:</p>
        <p class='status'>{statusText}</p>
        <p>If you believe this is a mistake or have any questions, please contact support.</p>
        <div class='footer'>
            – NaviGo Team
        </div>
    </div>
</body>
</html>";
		}
		public string GenerateCompanyStatusEmailHtml(Company company)
		{
			string statusText;
			string statusColor;
			string extraMessage;

			switch (company.CompanyStatus)
			{
				case CompanyStatus.Approved:
					statusText = "Approved";
					statusColor = "#28a745"; // green
					extraMessage = "Congratulations! Your company has been approved and can now fully use the NaviGo platform.";
					break;

				case CompanyStatus.Rejected:
					statusText = "Rejected";
					statusColor = "#dc3545"; // red
					extraMessage = "Unfortunately, your company registration has been rejected. Please contact support if you believe this is a mistake.";
					break;

				default: // Pending
					statusText = "Pending";
					statusColor = "#ffc107"; // yellow
					extraMessage = "Your company registration is still under review. We will notify you once the verification process is complete.";
					break;
			}

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
            color: #007bff;
            margin-bottom: 20px;
        }}
        p {{
            color: #333333;
            font-size: 16px;
            line-height: 1.5;
        }}
        .status {{
            font-weight: bold;
            color: {statusColor};
            font-size: 18px;
        }}
        .footer {{
            margin-top: 30px;
            font-size: 14px;
            color: #888888;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h2>Company Status Update</h2>
        <p>Dear {company.CompanyName},</p>
        <p>The current status of your company account is:</p>
        <p class='status'>{statusText}</p>
        <p>{extraMessage}</p>
        <div class='footer'>
            – NaviGo Team
        </div>
    </div>
</body>
</html>";
		}
	}

}
