using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Queries.Vehicle;
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
	public class GetVehicleByIdQueryHandler : IRequestHandler<GetVehicleByIdQuery, VehicleDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public GetVehicleByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<VehicleDto> Handle(GetVehicleByIdQuery request, CancellationToken cancellationToken)
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
			var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.VehicleId);
			if (vehicle == null)
			{
				throw new KeyNotFoundException($"Vehicle with Id {request.VehicleId} not found.");
			}
			var company = await _unitOfWork.Companies.GetByIdAsync(vehicle.CompanyId);
			var current = await _unitOfWork.Locations.GetByIdAsync(vehicle.CurrentLocationId.Value);
			var type = await _unitOfWork.VehicleTypes.GetByIdAsync(vehicle.VehicleTypeId);
			var vehicledto = new VehicleDto
			{
				Id = vehicle.Id,
				CreatedAt = vehicle.CreatedAt,
				Brand = vehicle.Brand,
				ManufactureYear = vehicle.ManufactureYear,
				Model = vehicle.Model,
				CapacityKg = vehicle.CapacityKg,
				Categories = vehicle.Categories,
				CompanyId = vehicle.CompanyId,
				CurrentLocationId = vehicle.CurrentLocationId.Value,
				EngineCapacityCc = vehicle.EngineCapacityCc,
				InsuranceExpiry = vehicle.InsuranceExpiry,
				LastInspectionDate = vehicle.LastInspectionDate,
				RegistrationNumber = vehicle.RegistrationNumber,
				VehiclePicture = vehicle.VehiclePicture,
				VehicleStatus = vehicle.VehicleStatus.ToString(),
				VehicleTypeId = vehicle.VehicleTypeId,
				CompanyName = company != null ? company.CompanyName : "",
				CurrentLocationName = current != null ? $"{current.FullAddress}, {current.City}, {current.Country}" : "",
				VehicleTypeName = type != null ? type.TypeName : ""
			};
			//return _mapper.Map<VehicleDto>(vehicle);
			return vehicledto;
		}
	}
}
