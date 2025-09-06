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
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Vehicle
{
	public class GetAllVehiclesQueryHandler : IRequestHandler<GetAllVehiclesQuery, IEnumerable<VehicleDto>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public GetAllVehiclesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<IEnumerable<VehicleDto>> Handle(GetAllVehiclesQuery request, CancellationToken cancellationToken)
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

			if (!user.CompanyId.HasValue)
				throw new ValidationException("User is not assigned to a company.");

			var userCompany = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);
			if (userCompany == null)
				throw new ValidationException("Your company doesn't exist.");

			if (userCompany.CompanyType != CompanyType.Carrier && userCompany.CompanyType != CompanyType.Forwarder)
				throw new ValidationException("Only Carrier and Forwarder companies can see vehicles.");
			IEnumerable<Domain.Entities.Vehicle> vehicles;

			if (userCompany.CompanyType == CompanyType.Forwarder)
			{
				vehicles = (await _unitOfWork.Vehicles.GetAllAsync())
							.Where(v => v.VehicleStatus == VehicleStatus.Free)
							.ToList();
			}
			else 
			{
				vehicles = (await _unitOfWork.Vehicles.GetByCompanyIdAsync(userCompany.Id))
							.ToList();
			}

			if (!vehicles.Any())
				return new List<VehicleDto>();

			var locationIds = vehicles
							  .Where(v => v.CurrentLocationId.HasValue)
							  .Select(v => v.CurrentLocationId!.Value)
							  .Distinct()
							  .ToList();

			var allLocations = (await _unitOfWork.Locations.GetAllAsync())
							   .Where(l => locationIds.Contains(l.Id))
							   .ToDictionary(l => l.Id, l => l);

			var vehicleTypeIds = vehicles.Select(v => v.VehicleTypeId).Distinct().ToList();
			var allVehicleTypes = (await _unitOfWork.VehicleTypes.GetAllAsync())
								  .Where(t => vehicleTypeIds.Contains(t.Id))
								  .ToDictionary(t => t.Id, t => t);

			var vehiclesDto = vehicles.Select(v =>
			{
				var location = v.CurrentLocationId.HasValue && allLocations.ContainsKey(v.CurrentLocationId.Value)
							   ? allLocations[v.CurrentLocationId.Value]
							   : null;
				var type = allVehicleTypes.ContainsKey(v.VehicleTypeId) ? allVehicleTypes[v.VehicleTypeId] : null;

				return new VehicleDto
				{
					Id = v.Id,
					CreatedAt = v.CreatedAt,
					Brand = v.Brand,
					ManufactureYear = v.ManufactureYear,
					Model = v.Model,
					CapacityKg = v.CapacityKg,
					Categories = v.Categories,
					CompanyId = v.CompanyId,
					CurrentLocationId = v.CurrentLocationId ?? 0,
					EngineCapacityCc = v.EngineCapacityCc,
					InsuranceExpiry = v.InsuranceExpiry,
					LastInspectionDate = v.LastInspectionDate,
					RegistrationNumber = v.RegistrationNumber,
					VehiclePicture = v.VehiclePicture,
					VehicleStatus = v.VehicleStatus.ToString(),
					VehicleTypeId = v.VehicleTypeId,
					CompanyName = userCompany.CompanyName,
					CurrentLocationName = location != null
										  ? $"{location.FullAddress}, {location.City}, {location.Country}"
										  : "",
					VehicleTypeName = type != null ? type.TypeName : ""
				};
			}).ToList();

			return vehiclesDto;
		}
	}
}
