using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NaviGoApi.Application.CQRS.Commands.User;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.User
{
	public class AuthenticateCommandHandler : IRequestHandler<AuthenticateCommand, (string accessToken, string refreshToken)?>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly string _jwtSecret;

		public AuthenticateCommandHandler(IUnitOfWork unitOfWork, IConfiguration configuration)
		{
			_unitOfWork = unitOfWork;
			_jwtSecret = configuration["JWT_SECRET"]
				?? throw new Exception("JWT_SECRET nije pronađen u konfiguraciji.");
		}

		public async Task<(string accessToken, string refreshToken)?> Handle(AuthenticateCommand request, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
			if (user == null || user.EmailVerified==false)
				return null;
			
			var hashedPassword = HashPassword(request.Password);
			if (user.PasswordHash != hashedPassword)
				return null;

			var refreshToken = GenerateRefreshToken(""); 

			user.RefreshTokens.Add(refreshToken);
			await _unitOfWork.SaveChangesAsync();

			var accessToken = GenerateJwtToken(user);

			return (accessToken, refreshToken.Token);
		}

		private string HashPassword(string password)
		{
			using var sha256 = SHA256.Create();
			var bytes = Encoding.UTF8.GetBytes(password);
			var hashBytes = sha256.ComputeHash(bytes);
			return Convert.ToBase64String(hashBytes);
		}

		private RefreshToken GenerateRefreshToken(string ipAddress)
		{
			var randomBytes = new byte[64];
			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(randomBytes);

			return new RefreshToken
			{
				Token = Convert.ToBase64String(randomBytes),
				Expires = DateTime.UtcNow.AddDays(7),
				Created = DateTime.UtcNow,
				CreatedByIp = ipAddress
			};
		}

		private string GenerateJwtToken(global::NaviGoApi.Domain.Entities.User user)
		{
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.Email),
				new Claim("email", user.Email),
				new Claim("role", user.UserRole.ToString()),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			};

			var token = new JwtSecurityToken(
				claims: claims,
				expires: DateTime.UtcNow.AddHours(2),
				signingCredentials: creds);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
