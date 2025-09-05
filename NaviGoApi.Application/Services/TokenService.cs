using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
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

namespace NaviGoApi.Application.Services
{
	public class TokenService : ITokenService
	{
		private readonly string _jwtSecret;
		private readonly IUnitOfWork _unitOfWork;

		public TokenService(IConfiguration configuration, IUnitOfWork unitOfWork)
		{
			_jwtSecret = configuration["JWT_SECRET"] ?? throw new Exception("JWT_SECRET nije pronađen");
			_unitOfWork = unitOfWork;
		}

		public async Task<string> GenerateJwtToken(User user)
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

		public RefreshToken GenerateRefreshToken(string ipAddress, int userId)
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
				UserId = userId
			};
		}
	}
}
