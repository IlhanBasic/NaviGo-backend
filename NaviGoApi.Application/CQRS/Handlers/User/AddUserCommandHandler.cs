using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.User;
using NaviGoApi.Application.DTOs.User;
using NaviGoApi.Application.Services;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
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

		public AddUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_emailService = emailService;
		}

		public async Task<UserDto> Handle(AddUserCommand request, CancellationToken cancellationToken)
		{
			var userEntity = _mapper.Map<Domain.Entities.User>(request.UserDto);

			userEntity.PasswordHash = HashPassword(request.UserDto.Password);
			var verificationToken = System.Guid.NewGuid().ToString();
			userEntity.EmailVerificationToken = verificationToken;
			await _unitOfWork.Users.AddAsync(userEntity);
			await _unitOfWork.SaveChangesAsync();

			var verificationLink = $"https://localhost:7028/api/User/verify-email?token={verificationToken}";
			// Pošalji email sa linkom za verifikaciju
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