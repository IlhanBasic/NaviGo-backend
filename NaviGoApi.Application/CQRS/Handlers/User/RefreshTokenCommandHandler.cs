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
	public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, (string accessToken, string refreshToken)?>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly string _jwtSecret;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public RefreshTokenCommandHandler(IUnitOfWork unitOfWork, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_jwtSecret = configuration["JWT_SECRET"]
				?? throw new Exception("JWT_SECRET nije pronađen u konfiguraciji.");
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<(string accessToken, string refreshToken)?> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.Users.FindByRefreshTokenAsync(request.Token);
			if (user == null)
				return null;

			//var refreshToken = user.RefreshTokens.SingleOrDefault(rt => rt.Token == request.Token);

			//if (refreshToken == null || !refreshToken.IsActive)
			//	return null;
			var refreshToken = await _unitOfWork.Users.GetRefreshTokenAsync(request.Token);
			if (refreshToken == null || !refreshToken.IsActive)
				return null;

			//var newRefreshToken = GenerateRefreshToken(request.IpAddress);
			//user.RefreshTokens.Add(newRefreshToken);
			//await _unitOfWork.SaveChangesAsync();
			refreshToken.Revoked = DateTime.UtcNow;
			refreshToken.RevokedByIp = GetIpAddress(); 

			var newRefreshToken = GenerateRefreshToken(GetIpAddress(),user.Id);
			//user.RefreshTokens.Add(newRefreshToken);
			await _unitOfWork.Users.AddRefreshTokenAsync(newRefreshToken);
			await _unitOfWork.SaveChangesAsync();

			var newAccessToken = GenerateJwtToken(user);

			return (newAccessToken, newRefreshToken.Token);
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

		private string GenerateJwtToken(global::NaviGoApi.Domain.Entities.User user)
		{
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.Email),
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
