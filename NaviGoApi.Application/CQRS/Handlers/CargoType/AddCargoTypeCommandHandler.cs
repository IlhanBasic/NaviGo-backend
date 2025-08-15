using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.CargoType;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.CargoType
{
	public class AddCargoTypeCommandHandler : IRequestHandler<AddCargoTypeCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public AddCargoTypeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(AddCargoTypeCommand request, CancellationToken cancellationToken)
		{
			var httpContext = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("HttpContext is not available.");

			var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
			if (string.IsNullOrWhiteSpace(userEmail))
				throw new ValidationException("User email not found in authentication token.");
			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException($"User with email '{userEmail}' not found.");
			if (user.UserRole != UserRole.SuperAdmin)
				throw new ValidationException("You are not allowed to add cargo type.");
			var typeName = request.CargoTypeDto.TypeName.Trim();
			var exists = await _unitOfWork.CargoTypes.GetByTypeName(typeName);
			if (exists!=null)
			{
				throw new ValidationException($"Cargo type with name '{typeName}' already exists.");
			}
			var cargoType = _mapper.Map<Domain.Entities.CargoType>(request.CargoTypeDto);
			await _unitOfWork.CargoTypes.AddAsync(cargoType);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}
}
