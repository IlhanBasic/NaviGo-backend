using NaviGoApi.Application.CQRS.Handlers.Contract;
using NaviGoApi.Domain.Entities;
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
		Task SendSuspendEmailAsync (string toEmail);
		Task SendEmailCheckUserNotification (string toEmail, User user);
		Task SendEmailCheckCompanyNotification(string toEmail, Company company);
		Task SendEmailPickupChangeNotification(string toEmail, Shipment shipment);
		Task SendEmailUserStatusNotification(string toEmail, User user);
		Task SendEmailCompanyStatusNotification(string toEmail, Company company);
		Task SendEmailContractCreatedNotification(string toEmail, ContractNotificationDto contract);

		Task SendEmailAfterContractRejection(string toEmail, Contract contract);

		Task SendEmailAfterContractAcception(string toEmail, ContractDetailsDto contract);
	}

}
