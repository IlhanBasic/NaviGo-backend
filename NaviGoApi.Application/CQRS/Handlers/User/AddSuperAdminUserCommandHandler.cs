using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NaviGoApi.Application.CQRS.Commands.User;
using NaviGoApi.Application.DTOs.User;
using NaviGoApi.Application.Services;
using NaviGoApi.Application.Settings;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.User
{
	public class AddSuperAdminUserCommandHandler : IRequestHandler<AddSuperAdminUserCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public AddSuperAdminUserCommandHandler(
			IUnitOfWork unitOfWork,
			IMapper mapper,
			IHttpContextAccessor httpContextAccessor)

		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(AddSuperAdminUserCommand request, CancellationToken cancellationToken)
		{
			var httpContext = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("HttpContext is not available.");
			var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
			if (string.IsNullOrWhiteSpace(userEmail))
				throw new ValidationException("User email not found in authentication token.");
			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException($"User with email '{userEmail}' not found.");
			if (user.UserRole != UserRole.SuperAdmin)
				throw new ValidationException("Only SuperAdmin can create other SuperAdmins");
			var dto = request.UserSuperAdmin;
			var existingUser = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
			if (existingUser != null)
				throw new ValidationException($"User with EMAIL {dto.Email} already exists.");
			var userEntity = _mapper.Map<Domain.Entities.User>(dto);
			userEntity.PasswordHash = HashPassword(dto.Password);
			userEntity.UserRole = UserRole.SuperAdmin;
			userEntity.CreatedAt = DateTime.UtcNow;
			userEntity.EmailVerified = true; 
			userEntity.UserStatus = UserStatus.Active;
			userEntity.CompanyId = null;
			await _unitOfWork.Users.AddAsync(userEntity);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
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
