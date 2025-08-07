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
	public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
	{
		private readonly IUnitOfWork _unitOfWork;

		public ChangePasswordCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
			if (user == null)
				return false;

			if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
				return false;

			user.PasswordHash = HashPassword(request.NewPassword);

			 _unitOfWork.Users.Update(user);
			await _unitOfWork.SaveChangesAsync();

			return true;
		}

		private bool VerifyPassword(string plainPassword, string hashedPassword)
		{
			var plainHash = HashPassword(plainPassword);
			return plainHash == hashedPassword;
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
