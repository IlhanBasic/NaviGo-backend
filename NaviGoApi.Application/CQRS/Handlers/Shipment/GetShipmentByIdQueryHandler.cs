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
	public class GetShipmentByIdQueryHandler : IRequestHandler<GetShipmentByIdQuery, ShipmentDto?>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public GetShipmentByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<ShipmentDto?> Handle(GetShipmentByIdQuery request, CancellationToken cancellationToken)
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

			var shipment = await _unitOfWork.Shipments.GetByIdAsync(request.Id);
			if (shipment == null)
				return null;

			// Provera nullable DriverId i VehicleId
			Domain.Entities.Driver? driver = null;
			if (shipment.DriverId.HasValue)
				driver = await _unitOfWork.Drivers.GetByIdAsync(shipment.DriverId.Value);

			Domain.Entities.Vehicle? vehicle = null;
			if (shipment.VehicleId.HasValue)
				vehicle = await _unitOfWork.Vehicles.GetByIdAsync(shipment.VehicleId.Value);

			var cargoType = await _unitOfWork.CargoTypes.GetByIdAsync(shipment.CargoTypeId);
			var contract = await _unitOfWork.Contracts.GetByIdAsync(shipment.ContractId);

			var shipmentDto = new ShipmentDto
			{
				Id = shipment.Id,
				ContractId = shipment.ContractId,
				ContractName = contract.ContractNumber,
				CargoTypeId = shipment.CargoTypeId,
				CargoTypeName = cargoType.TypeName,
				DriverId = shipment.DriverId.Value, // nullable
				DriverName = driver != null ? $"{driver.FirstName} {driver.LastName}" : null,
				VehicleId = shipment.VehicleId.Value, // nullable
				VehicleName = vehicle != null ? $"{vehicle.Brand}-{vehicle.Model} ({vehicle.ManufactureYear})" : null,
				Description = shipment.Description,
				WeightKg = shipment.WeightKg,
				Priority = shipment.Priority,
				Status = shipment.Status.ToString(),
				ScheduledDeparture = shipment.ScheduledDeparture,
				ScheduledArrival = shipment.ScheduledArrival,
				ActualDeparture = shipment.ActualDeparture,
				ActualArrival = shipment.ActualArrival
			};

			return shipmentDto;
		}

	}
}
