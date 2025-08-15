using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.Vehicle;
using NaviGoApi.Application.DTOs.Vehicle;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Vehicle
{
	public class UpdateVehicleCommandHandler : IRequestHandler<UpdateVehicleCommand, VehicleDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public UpdateVehicleCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<VehicleDto> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
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
			if (user.UserRole != UserRole.CompanyAdmin)
				throw new ValidationException("You are not allowed to update vehicle.");
			var existingVehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.Id);
			if (existingVehicle == null)
			{
				throw new ValidationException($"Vehicle with Id {request.Id} not found.");
			}
			if (existingVehicle.CompanyId != user.CompanyId)
				throw new ValidationException("User cannot update vehicle for wrong company.");

			_mapper.Map(request.VehicleUpdateDto, existingVehicle);

			await _unitOfWork.Vehicles.UpdateAsync(existingVehicle);
			await _unitOfWork.SaveChangesAsync();

			return _mapper.Map<VehicleDto>(existingVehicle);
		}
	}
}
