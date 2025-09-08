using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Queries.Shipment;
using NaviGoApi.Application.DTOs.Shipment;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Shipment
{
	public class GetAllShipmentQueryHandler : IRequestHandler<GetAllShipmentQuery, IEnumerable<ShipmentDto?>>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public GetAllShipmentQueryHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<IEnumerable<ShipmentDto?>> Handle(GetAllShipmentQuery request, CancellationToken cancellationToken)
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

			var shipments = (await _unitOfWork.Shipments.GetAllAsync(request.Search)).ToList();
			var contracts = (await _unitOfWork.Contracts.GetAllAsync()).ToList();
			var drivers = (await _unitOfWork.Drivers.GetAllAsync()).ToList();
			var vehicles = (await _unitOfWork.Vehicles.GetAllAsync()).ToList();
			var cargoTypes = (await _unitOfWork.CargoTypes.GetAllAsync()).ToList();
			var users = (await _unitOfWork.Users.GetAllAsync()).ToList();

			var contractsDict = contracts.ToDictionary(c => c.Id);
			var driversDict = drivers.ToDictionary(d => d.Id);
			var vehiclesDict = vehicles.ToDictionary(v => v.Id);
			var cargoTypesDict = cargoTypes.ToDictionary(c => c.Id);
			var usersDict = users.ToDictionary(u => u.Id);

			Domain.Entities.Company? userCompany = null;
			if (user.CompanyId != null)
			{
				userCompany = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value)
					?? throw new ValidationException($"Company with ID {user.CompanyId.Value} doesn't exist.");
			}

			var visibleShipments = shipments.Where(s =>
			{
				if (!contractsDict.TryGetValue(s.ContractId, out var contract))
					return false;

				if (user.UserRole == UserRole.SuperAdmin)
					return true;

				if (contract.ClientId == user.Id)
					return true;

				if (userCompany != null)
				{
					if (userCompany.CompanyType == CompanyType.Forwarder && contract.ForwarderId == userCompany.Id)
						return true;

					if (userCompany.CompanyType == CompanyType.Carrier)
					{
						if (s.VehicleId.HasValue && vehiclesDict.TryGetValue(s.VehicleId.Value, out var v) && v.CompanyId == userCompany.Id)
							return true;
					}

					if (userCompany.CompanyType == CompanyType.Client)
					{
						if (usersDict.TryGetValue(contract.ClientId, out var clientUser)
							&& clientUser.CompanyId == userCompany.Id)
						{
							return true;
						}
					}
				}

				return false;
			}).ToList();

			var shipmentsDto = visibleShipments.Select(s =>
			{
				driversDict.TryGetValue(s.DriverId ?? 0, out var driver);
				cargoTypesDict.TryGetValue(s.CargoTypeId, out var cargoType);
				contractsDict.TryGetValue(s.ContractId, out var contract);
				vehiclesDict.TryGetValue(s.VehicleId ?? 0, out var vehicle);

				return new ShipmentDto
				{
					Id = s.Id,
					Status = s.Status.ToString(),
					Description = s.Description,
					Priority = s.Priority,
					WeightKg = s.WeightKg,
					ScheduledDeparture = s.ScheduledDeparture,
					ScheduledArrival = s.ScheduledArrival,
					ActualDeparture = s.ActualDeparture,
					ActualArrival = s.ActualArrival,
					CargoTypeId = s.CargoTypeId,
					ContractId = s.ContractId,
					DriverId = s.DriverId.Value, // nullable
					VehicleId = s.VehicleId.Value, // nullable

					CargoTypeName = cargoType?.TypeName ?? string.Empty,
					ContractName = contract?.ContractNumber ?? string.Empty,
					DriverName = driver != null ? $"{driver.FirstName} {driver.LastName}" : string.Empty,
					VehicleName = vehicle != null ? $"{vehicle.Brand}-{vehicle.Model} ({vehicle.ManufactureYear})" : string.Empty
				};
			}).ToList();

			return shipmentsDto;
		}

	}
}
