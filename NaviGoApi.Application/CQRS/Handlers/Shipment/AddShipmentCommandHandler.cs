using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.Shipment;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Shipment
{
	public class AddShipmentCommandHandler : IRequestHandler<AddShipmentCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public AddShipmentCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(AddShipmentCommand request, CancellationToken cancellationToken)
		{
			var contract = await _unitOfWork.Contracts.GetByIdAsync(request.ShipmentDto.ContractId);
			if (contract == null)
				throw new ValidationException($"Contract with ID {request.ShipmentDto.ContractId} doesn't exist.");

			if (contract.RouteId == null)
				throw new ValidationException($"Contract with ID {contract.Id} does not have an associated route.");

			var route = await _unitOfWork.Routes.GetByIdAsync(contract.RouteId);
			if (route == null)
				throw new ValidationException($"Route with ID {contract.RouteId} does not exist.");

			var company = await _unitOfWork.Companies.GetByIdAsync(route.CompanyId);
			if (company == null)
				throw new ValidationException($"Company with ID {route.CompanyId} doesn't exist.");

			if (company.CompanyType != Domain.Entities.CompanyType.Carrier)
				throw new ValidationException($"Company that created this route in contract must be a Carrier.");

			var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.ShipmentDto.VehicleId);
			if (vehicle == null)
				throw new ValidationException($"Vehicle with ID {request.ShipmentDto.VehicleId} doesn't exist.");
			if (vehicle.InsuranceExpiry < DateTime.UtcNow)
				throw new ValidationException($"Vehicle with ID {request.ShipmentDto.VehicleId} have expired insurance.");
			if (vehicle.VehicleStatus != Domain.Entities.VehicleStatus.Free)
				throw new ValidationException($"Vehicle with ID {request.ShipmentDto.VehicleId} is occupied.");
			bool vehicleOwnedByCompany = vehicle.CompanyId == company.Id;
			if (!vehicleOwnedByCompany)
				throw new ValidationException($"Company {company.CompanyName} doesn't own vehicle {vehicle.Brand}-{vehicle.Model}/{vehicle.ManufactureYear}.");

			// Dohvati sve drivere iz baze i filtriraj po kompaniji
			var allDrivers = await _unitOfWork.Drivers.GetAllAsync();
			var companyDrivers = allDrivers.Where(d => d.CompanyId == company.Id).ToList();

			if (!companyDrivers.Any(x => x.Id == request.ShipmentDto.DriverId))
				throw new ValidationException($"Driver with ID {request.ShipmentDto.DriverId} doesn't work in company {company.CompanyName}.");

			var driver = companyDrivers.First(d => d.Id == request.ShipmentDto.DriverId);
			if (driver.LicenseExpiry < DateTime.UtcNow)
				throw new ValidationException($"Driver with ID {request.ShipmentDto.DriverId} have expired licence. So he cannot drive.");
			var driverCategories = driver.LicenseCategories.Split(',', StringSplitOptions.RemoveEmptyEntries);
			var vehicleCategories = vehicle.Categories.Split(',', StringSplitOptions.RemoveEmptyEntries);

			bool hasValidLicense = driverCategories.Any(dc => vehicleCategories.Any(vc => vc.Trim() == dc.Trim()));
			if (!hasValidLicense)
				throw new ValidationException($"Driver with ID {request.ShipmentDto.DriverId} doesn't have required licences to drive this vehicle.");

			var cargoType = await _unitOfWork.CargoTypes.GetByIdAsync(request.ShipmentDto.CargoTypeId);
			if (cargoType == null)
				throw new ValidationException($"Cargo Type with ID {request.ShipmentDto.CargoTypeId} doesn't exist.");
			
			var existingShipments = await _unitOfWork.Shipments.GetByContractIdAsync(contract.Id);
			var totalWeight = existingShipments.Sum(s => s.WeightKg);

			if (totalWeight + request.ShipmentDto.WeightKg > vehicle.CapacityKg)
				throw new ValidationException($"Vehicle capacity exceeded. Available capacity: {vehicle.CapacityKg - totalWeight} kg.");

			var shipment = _mapper.Map<Domain.Entities.Shipment>(request.ShipmentDto);
			shipment.Status = Domain.Entities.ShipmentStatus.Scheduled;

			await _unitOfWork.Shipments.AddAsync(shipment);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}

	}
}
