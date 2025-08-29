using Google.Apis.Auth;
using MediatR;
using Microsoft.AspNetCore.Http;
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
	public class GoogleAuthenticateCommandHandler : IRequestHandler<GoogleAuthenticateCommand, (string accessToken, string refreshToken)?>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly string _jwtSecret;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public GoogleAuthenticateCommandHandler(IUnitOfWork unitOfWork, IConfiguration configuration,IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
			_jwtSecret = configuration["JWT_SECRET"] ?? throw new Exception("JWT_SECRET nije pronađen u konfiguraciji.");
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
				return null;
			}

			// Pronađi korisnika po email-u
			var user = await _unitOfWork.Users.GetByEmailAsync(payload.Email);

			if (user == null)
			{
				return null;
			}

			// Nastavi dalje sa refresh tokenom i jwt tokenom kao i ranije
			var refreshToken = GenerateRefreshToken(GetIpAddress(),user.Id);
			//user.RefreshTokens.Add(refreshToken);
			await _unitOfWork.Users.AddRefreshTokenAsync(refreshToken);
			await _unitOfWork.SaveChangesAsync();

			var accessToken = await GenerateJwtToken(user);

			return (accessToken, refreshToken.Token);
		}

		private RefreshToken GenerateRefreshToken(string ipAddress,int id)
		{
			var randomBytes = new byte[64];
			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(randomBytes);

			return new RefreshToken
			{
				Token = Convert.ToBase64String(randomBytes),
				Expires = DateTime.UtcNow.AddDays(7),
				Created = DateTime.UtcNow,
				CreatedByIp = ipAddress,
				UserId=id
			};
		}

		private async Task<string> GenerateJwtToken(global::NaviGoApi.Domain.Entities.User user)
		{
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			string companyType = "";
			if (user.CompanyId.HasValue)
			{
				var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);
				companyType = company?.CompanyType.ToString() ?? "";
			}

			var claims = new[]
			{
		new Claim(JwtRegisteredClaimNames.Sub, user.Email),
		new Claim("email", user.Email),
		new Claim("firstName", user.FirstName ?? ""),
		new Claim("lastName", user.LastName ?? ""),
		new Claim("companyType", companyType),
		new Claim("companyId",user.CompanyId.ToString() ?? ""),
		new Claim("id", user.Id.ToString()),
		new Claim("role", user.UserRole.ToString()),
		new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
	};

			var token = new JwtSecurityToken(
				claims: claims,
				expires: DateTime.UtcNow.AddHours(2),
				signingCredentials: creds);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		private string GetIpAddress()
		{
			var httpContext = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("HttpContext is not available.");

			return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
		}
	}

}
