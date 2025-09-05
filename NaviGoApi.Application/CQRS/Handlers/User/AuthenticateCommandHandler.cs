using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.User;
using NaviGoApi.Application.Services;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.User
{
	public class AuthenticateCommandHandler : IRequestHandler<AuthenticateCommand, (string accessToken, string refreshToken)?>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ITokenService _tokenService;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public AuthenticateCommandHandler(
			IUnitOfWork unitOfWork,
			ITokenService tokenService,
			IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_tokenService = tokenService;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<(string accessToken, string refreshToken)?> Handle(AuthenticateCommand request, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

			if (user == null)
				throw new ValidationException("User with this email does not exist.");

			if (!user.EmailVerified)
				throw new ValidationException("Email is not verified.");

			if (user.UserStatus == UserStatus.Inactive)
				throw new ValidationException("User account is inactive.");

			var hashedPassword = HashPassword(request.Password);
			if (user.PasswordHash != hashedPassword)
				throw new ValidationException("Incorrect password.");

			var refreshToken = _tokenService.GenerateRefreshToken(GetIpAddress(), user.Id);
			await _unitOfWork.Users.AddRefreshTokenAsync(refreshToken);
			await _unitOfWork.SaveChangesAsync();

			var accessToken = await _tokenService.GenerateJwtToken(user);

			return (accessToken, refreshToken.Token);
		}

		private string HashPassword(string password)
		{
			using var sha256 = SHA256.Create();
			var bytes = Encoding.UTF8.GetBytes(password);
			var hashBytes = sha256.ComputeHash(bytes);
			return Convert.ToBase64String(hashBytes);
		}

		private string GetIpAddress()
		{
			var httpContext = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("HttpContext is not available.");

			return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
		}
	}
}
