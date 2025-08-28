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

			//return _mapper.Map<ShipmentDto>(shipment);
			var driver = await _unitOfWork.Drivers.GetByIdAsync(shipment.DriverId);
			var cargoType = await _unitOfWork.CargoTypes.GetByIdAsync(shipment.CargoTypeId);
			var contract = await _unitOfWork.Contracts.GetByIdAsync(shipment.ContractId);
			var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(shipment.VehicleId);
			var shipmentDto = new ShipmentDto
			{
				ActualArrival = shipment.ActualArrival,
				ActualDeparture = shipment.ActualDeparture,
				ScheduledArrival = shipment.ScheduledArrival,
				ScheduledDeparture = shipment.ScheduledDeparture,
				CargoTypeId = shipment.CargoTypeId,
				ContractId = shipment.ContractId,
				Description = shipment.Description,
				DriverId = shipment.DriverId,
				Priority = shipment.Priority,
				WeightKg = shipment.WeightKg,
				Status = shipment.Status.ToString(),
				Id = shipment.Id,
				CargoTypeName = cargoType.TypeName,
				ContractName = contract.ContractNumber,
				DriverName = $"{driver.FirstName} {driver.LastName}",
				VehicleName = $"{vehicle.Brand}-{vehicle.Model} ({vehicle.ManufactureYear})",
				VehicleId = shipment.VehicleId
			};
			return shipmentDto;
		}
	}
}
