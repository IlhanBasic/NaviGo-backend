using AutoMapper;
using MediatR;
using Microsoft.Extensions.Options;
using NaviGoApi.Application.CQRS.Commands.User;
using NaviGoApi.Application.DTOs.User;
using NaviGoApi.Application.Services;
using NaviGoApi.Application.Settings;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.User
{
	public class AddUserCommandHandler : IRequestHandler<AddUserCommand, UserDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IEmailService _emailService;
		private readonly string _apiBaseUrl;

		public AddUserCommandHandler(
			IUnitOfWork unitOfWork,
			IMapper mapper,
			IEmailService emailService,
			IOptions<ApiSettings> apiSettings)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_emailService = emailService;
			_apiBaseUrl = apiSettings.Value.BaseUrl;
		}
		public async Task<UserDto> Handle(AddUserCommand request, CancellationToken cancellationToken)
		{
			var dto = request.UserDto;
			var existsUser = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
			if (existsUser != null)
				throw new ValidationException($"User with EMAIL {dto.Email} already exists.");
			if ((dto.UserRole == UserRole.CompanyUser || dto.UserRole == UserRole.CompanyAdmin) && dto.CompanyId == null)
			{
				throw new ValidationException("CompanyUser or CompanyAdmin must be linked to a company.");
			}

			if (dto.UserRole == UserRole.SuperAdmin && dto.CompanyId != null)
			{
				throw new ValidationException("SuperAdmin must not be linked to a company.");
			}

			if (dto.CompanyId != null)
			{
				var companyExists = await _unitOfWork.Companies.GetByIdAsync(dto.CompanyId.Value);
				if (companyExists==null)
				{
					throw new ValidationException($"Company with ID {dto.CompanyId} does not exist.");
				}
				if (companyExists.CompanyType != CompanyType.Client && dto.UserRole == UserRole.RegularUser)
					throw new ValidationException($"RegularUser must work in Client company.");

			}

			var userEntity = _mapper.Map<Domain.Entities.User>(dto);
			userEntity.PasswordHash = HashPassword(dto.Password);
			userEntity.CreatedAt = DateTime.UtcNow;
			userEntity.EmailVerificationToken = Guid.NewGuid().ToString();
			userEntity.EmailVerificationTokenDuration = TimeSpan.FromMinutes(15);

			if (userEntity.UserRole == UserRole.SuperAdmin)
			{
				userEntity.EmailVerified = true;
			}

			userEntity.UserStatus = UserStatus.Inactive;

			// Čuvanje u bazi
			await _unitOfWork.Users.AddAsync(userEntity);
			await _unitOfWork.SaveChangesAsync();

			
			var verificationLink = $"{_apiBaseUrl}api/User/verify-email?token={userEntity.EmailVerificationToken}";
			if (userEntity.UserRole != UserRole.SuperAdmin)
				await _emailService.SendVerificationEmailAsync(userEntity.Email, verificationLink);
			return _mapper.Map<UserDto>(userEntity);
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
