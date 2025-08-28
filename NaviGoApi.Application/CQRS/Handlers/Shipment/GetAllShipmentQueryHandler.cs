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
			var shipments = await _unitOfWork.Shipments.GetAllAsync(request.Search);
			var shipmentsDto = new List<ShipmentDto>();
			//return _mapper.Map<IEnumerable<ShipmentDto?>>(shipments);
			foreach(var s in shipments)
			{
				var driver = await _unitOfWork.Drivers.GetByIdAsync(s.DriverId);
				var cargoType = await _unitOfWork.CargoTypes.GetByIdAsync(s.CargoTypeId);
				var contract = await _unitOfWork.Contracts.GetByIdAsync(s.ContractId);
				var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(s.VehicleId);
				shipmentsDto.Add(new ShipmentDto
				{
					ActualArrival = s.ActualArrival,
					ActualDeparture = s.ActualDeparture,
					ScheduledArrival= s.ScheduledArrival,
					ScheduledDeparture= s.ScheduledDeparture,
					CargoTypeId = s.CargoTypeId,
					ContractId = s.ContractId,
					Description = s.Description,
					DriverId = s.DriverId,
					Priority = s.Priority,
					WeightKg = s.WeightKg,
					Status = s.Status.ToString(),
					Id = s.Id,
					CargoTypeName=cargoType.TypeName,
					ContractName=contract.ContractNumber,
					DriverName = $"{driver.FirstName} {driver.LastName}",
					VehicleName=$"{vehicle.Brand}-{vehicle.Model} ({vehicle.ManufactureYear})",
					VehicleId=s.VehicleId
				});
			}
			return shipmentsDto;
		}
	}
}
