using MediatR;
using NaviGoApi.Application.CQRS.Commands.User;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.User
{
	public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, bool>
	{
		private readonly IUnitOfWork _unitOfWork;

		public VerifyEmailCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<bool> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.Users.GetByEmailVerificationTokenAsync(request.Token);

			if (user == null || user.EmailVerified || user.EmailVerificationTokenDuration > TimeSpan.FromMinutes(15))
			{
				return false;
			}

			user.EmailVerified = true;
			user.EmailVerificationToken = null;
			user.EmailVerificationTokenDuration = TimeSpan.FromMinutes(15);

			await _unitOfWork.SaveChangesAsync();

			return true;
		}
	}
}
