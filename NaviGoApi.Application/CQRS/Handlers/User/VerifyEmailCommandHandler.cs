using MediatR;
using NaviGoApi.Application.CQRS.Commands.User;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Threading;
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

			if (user == null || user.EmailVerified)
				return false;
			var tokenDuration = user.EmailVerificationTokenDuration ?? TimeSpan.FromMinutes(15);
			var tokenCreationTime = user.CreatedAt;
			if (DateTime.UtcNow > tokenCreationTime.Add(tokenDuration))
			{
				Console.WriteLine("VerifyEmail: Token expired.");
				return false;
			}
			user.EmailVerified = true;
			user.EmailVerificationToken = null;
			user.EmailVerificationTokenDuration = null;

			await _unitOfWork.Users.UpdateAsync(user);
			await _unitOfWork.SaveChangesAsync();
			Console.WriteLine("VerifyEmail: User email successfully verified.");
			return true;
		}
	}
}
