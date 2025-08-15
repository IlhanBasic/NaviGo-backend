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
	public class AddVehicleCommandHandler : IRequestHandler<AddVehicleCommand, VehicleDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public AddVehicleCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_httpContextAccessor = httpContextAccessor;
		}
		public async Task<VehicleDto> Handle(AddVehicleCommand request, CancellationToken cancellationToken)
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
				throw new ValidationException("You are not allowed to add vehicle.");
			var dto = request.VehicleCreateDto;
			var company = await _unitOfWork.Companies.GetByIdAsync(dto.CompanyId);
			if (company == null)
			{
				throw new ValidationException($"Company with ID {dto.CompanyId} does not exist.");
			}
			if (company.CompanyStatus == Domain.Entities.CompanyStatus.Pending)
				throw new ValidationException("Pending company cannot create vehicles.");
			if (user.CompanyId != company.Id)
				throw new ValidationException("You cannot add vehicle to wrong company.");
			if (company.CompanyType != Domain.Entities.CompanyType.Carrier)
				throw new ValidationException("Only Carrier companies can have registrated vehicles on this platform");
			var vehicleType = await _unitOfWork.VehicleTypes.GetByIdAsync(dto.VehicleTypeId);
			if (vehicleType == null)
			{
				throw new ValidationException($"Vehicle type with ID {dto.VehicleTypeId} does not exist.");
			}
			if (dto.CurrentLocationId.HasValue)
			{
				var location = await _unitOfWork.Locations.GetByIdAsync(dto.CurrentLocationId.Value);
				if (location == null)
				{
					throw new ValidationException($"Location with ID {dto.CurrentLocationId.Value} does not exist.");
				}
			}
			var existingVehicleWithReg = await _unitOfWork.Vehicles.GetByRegistrationNumberAsync(dto.RegistrationNumber);
			if (existingVehicleWithReg != null)
			{
				throw new ValidationException($"Registration number '{dto.RegistrationNumber}' is already assigned to another vehicle.");
			}
			if (dto.LastInspectionDate.HasValue && dto.LastInspectionDate.Value.Year < dto.ManufactureYear)
				throw new ValidationException("Inspection Date cannot be before Manufacture Year.");
			var vehicleEntity = _mapper.Map<Domain.Entities.Vehicle>(dto);
			vehicleEntity.VehicleStatus = Domain.Entities.VehicleStatus.Free;

			await _unitOfWork.Vehicles.AddAsync(vehicleEntity);
			await _unitOfWork.SaveChangesAsync();

			return _mapper.Map<VehicleDto>(vehicleEntity);
		}

	}
}
