using MediatR;
using NaviGoApi.Application.CQRS.Commands.User;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.User
{
	public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
	{
		private readonly IUnitOfWork _unitOfWork;

		public ResetPasswordCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.Users.GetByPasswordResetTokenAsync(request.Token);
			if (user == null || user.PasswordResetTokenDuration > TimeSpan.FromMinutes(15))
				return false;


			user.PasswordHash = HashPassword(request.NewPassword);
			user.PasswordResetToken = null;
			user.PasswordResetTokenDuration = null;

			await _unitOfWork.Users.UpdateAsync(user);
			await _unitOfWork.SaveChangesAsync();

			return true;
		}

		private string HashPassword(string password)
		{
			using var sha256 = SHA256.Create();
			var bytes = Encoding.UTF8.GetBytes(password);
			var hash = sha256.ComputeHash(bytes);
			return Convert.ToBase64String(hash);
		}
	}
}
