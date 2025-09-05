using Google.Apis.Auth;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.User;
using NaviGoApi.Application.Services;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.User
{
	public class GoogleAuthenticateCommandHandler : IRequestHandler<GoogleAuthenticateCommand, (string accessToken, string refreshToken)?>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ITokenService _tokenService;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public GoogleAuthenticateCommandHandler(
			IUnitOfWork unitOfWork,
			ITokenService tokenService,
			IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_tokenService = tokenService;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<(string accessToken, string refreshToken)?> Handle(GoogleAuthenticateCommand request, CancellationToken cancellationToken)
		{
			GoogleJsonWebSignature.Payload payload;

			try
			{
				payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken);
			}
			catch
			{
				throw new ValidationException("Invalid Google token provided. Please try again.");
			}

			var user = await _unitOfWork.Users.GetByEmailAsync(payload.Email);
			if (user == null)
				throw new ValidationException("User with this email does not exist.");

			if (!user.EmailVerified)
				throw new ValidationException("Email is not verified.");

			if (user.UserStatus == UserStatus.Inactive)
				throw new ValidationException("User account is inactive.");

			// Generiši refresh token i JWT token preko TokenService
			var refreshToken = _tokenService.GenerateRefreshToken(GetIpAddress(), user.Id);
			await _unitOfWork.Users.AddRefreshTokenAsync(refreshToken);
			await _unitOfWork.SaveChangesAsync();

			var accessToken = await _tokenService.GenerateJwtToken(user);

			return (accessToken, refreshToken.Token);
		}

		private string GetIpAddress()
		{
			var httpContext = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("HttpContext is not available.");

			return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
		}
	}
}
