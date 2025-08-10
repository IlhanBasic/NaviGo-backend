using MediatR;
using NaviGoApi.Application.CQRS.Commands.User;
using NaviGoApi.Application.Services;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.User
{
	public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IEmailService _emailService;

		public ForgotPasswordCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService)
		{
			_unitOfWork = unitOfWork;
			_emailService = emailService;
		}

		public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
			if (user == null)
				return false;

			var pin = GenerateSixDigitPin();
			user.PasswordResetToken = pin;
			user.PasswordResetTokenDuration = TimeSpan.FromMinutes(15);
			await _unitOfWork.Users.UpdateAsync(user);
			await _unitOfWork.SaveChangesAsync();
			await _emailService.SendResetPasswordPinEmailAsync(user.Email, pin);

			return true;
		}

		private string GenerateSixDigitPin()
		{
			using var rng = RandomNumberGenerator.Create();
			var bytes = new byte[4];
			rng.GetBytes(bytes);
			int pin = BitConverter.ToInt32(bytes, 0) % 1_000_000;
			if (pin < 0) pin = -pin;
			return pin.ToString("D6");
		}
	}
}
