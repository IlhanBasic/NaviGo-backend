using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Services
{
	public interface IEmailService
	{
		Task SendVerificationEmailAsync(string toEmail, string verificationLink);
		Task SendResetPasswordPinEmailAsync(string toEmail, string pin);

	}

}
