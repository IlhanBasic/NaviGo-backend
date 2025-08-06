using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.User;
using NaviGoApi.Application.DTOs.User;
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

		public AddUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<UserDto> Handle(AddUserCommand request, CancellationToken cancellationToken)
		{
			var userEntity = _mapper.Map<global::NaviGoApi.Domain.Entities.User>(request.UserDto);


			userEntity.PasswordHash = HashPassword(request.UserDto.Password);

			await _unitOfWork.Users.AddAsync(userEntity);
			await _unitOfWork.SaveChangesAsync();

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
