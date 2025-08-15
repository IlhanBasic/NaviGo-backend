using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NaviGoApi.Application.CQRS.Commands.VehicleType;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.VehicleType
{
	public class UpdateVehicleTypeCommandHandler : IRequestHandler<UpdateVehicleTypeCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public UpdateVehicleTypeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_httpContextAccessor = httpContextAccessor;
		}
		public async Task<Unit> Handle(UpdateVehicleTypeCommand request, CancellationToken cancellationToken)
		{
			var httpContext = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("HttpContext is not available.");

			var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
			if (string.IsNullOrWhiteSpace(userEmail))
				throw new ValidationException("User email not found in authentication token.");
			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException($"User with email '{userEmail}' not found.");
			if (user.UserStatus != UserStatus.Active)
				throw new ValidationException("Your account is not activated.");
			if (user.UserRole != UserRole.SuperAdmin)
				throw new ValidationException("You are not allowed to update vehicle type.");
			var existing = await _unitOfWork.VehicleTypes.GetByIdAsync(request.Id);
			if (existing == null)
				throw new ValidationException("Vehicle type not found.");

			var newTypeName = request.VehicleTypeDto.TypeName.Trim();
			var exists = await _unitOfWork.VehicleTypes.GetByTypeName(newTypeName);
			if (exists != null && exists.Id!=request.Id)
			{
				throw new ValidationException($"Vehicle type with name '{newTypeName}' already exists.");
			}
			_mapper.Map(request.VehicleTypeDto, existing);
			await _unitOfWork.VehicleTypes.UpdateAsync(existing);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}


	}
}
