using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.Contract;
using NaviGoApi.Application.Services;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using NaviGoApi.Application.CQRS.Commands.User;
using System;

namespace NaviGoApi.Application.CQRS.Handlers.Contract
{
	public class UpdateCarrierContractStatusCommandHandler : IRequestHandler<UpdateCarrierContractStatusCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IEmailService _emailService;

		public UpdateCarrierContractStatusCommandHandler(
			IMapper mapper,
			IUnitOfWork unitOfWork,
			IHttpContextAccessor httpContextAccessor,
			IEmailService emailService)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
			_emailService = emailService;
		}

		//public async Task<Unit> Handle(UpdateCarrierContractStatusCommand request, CancellationToken cancellationToken)
		//{
		//	if (_httpContextAccessor.HttpContext == null)
		//		throw new ValidationException("HttpContext is null.");

		//	var userEmail = _httpContextAccessor.HttpContext.User
		//		.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

		//	if (string.IsNullOrEmpty(userEmail))
		//		throw new ValidationException("User email not found in token.");

		//	var currentUser = await _unitOfWork.Users.GetByEmailAsync(userEmail)
		//		?? throw new ValidationException("User not found.");

		//	if (currentUser.CompanyId == null)
		//		throw new ValidationException("You don't work in any company.");

		//	var company = await _unitOfWork.Companies.GetByIdAsync(currentUser.CompanyId.Value)
		//		?? throw new ValidationException($"Company with ID {currentUser.CompanyId.Value} doesn't exist.");

		//	if (company.CompanyType != CompanyType.Carrier)
		//		throw new ValidationException("Company must have Carrier Company Type.");

		//	var contract = await _unitOfWork.Contracts.GetByIdAsync(request.Id)
		//		?? throw new ValidationException("Contract not found.");

		//	var shipments = (await _unitOfWork.Shipments.GetByContractIdAsync(contract.Id)).ToList();
		//	var allCargoTypes = await _unitOfWork.CargoTypes.GetAllAsync();

		//	if (request.ContractDto.ContractStatus == ContractStatus.Active)
		//	{


		//		if (!request.ContractDto.DriverId.HasValue || !request.ContractDto.VehicleId.HasValue)
		//			throw new ValidationException("DriverId and VehicleId are required when accepting a contract.");
		//		var contractDriver = await _unitOfWork.Drivers.GetByIdAsync(request.ContractDto.DriverId.Value);
		//		if (contractDriver == null)
		//			throw new ValidationException($"Driver with ID {request.ContractDto.DriverId.Value} doesn't exist.");
		//		if (contractDriver.DriverStatus != DriverStatus.Available)
		//			throw new ValidationException("This driver wasn't available.");
		//		var contractVehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.ContractDto.VehicleId.Value);
		//		if (contractVehicle == null)
		//			throw new ValidationException($"Vehicle with ID {request.ContractDto.VehicleId.Value} doesn't exist.");
		//		if (contractVehicle.VehicleStatus != VehicleStatus.Free)
		//			throw new ValidationException("This vehicle wasn't available.");
		//		contract.ContractStatus = ContractStatus.Active;

		//		foreach (var shipment in shipments)
		//		{
		//			shipment.DriverId = contractDriver.Id;
		//			shipment.VehicleId = contractVehicle.Id;
		//			shipment.Status = ShipmentStatus.Scheduled;
		//			await _unitOfWork.Shipments.UpdateAsync(shipment);
		//		}

		//		await _unitOfWork.SaveChangesAsync();

		//		var forwarder = await _unitOfWork.Companies.GetByIdAsync(contract.ForwarderId)
		//			?? throw new ValidationException($"Forwarder company with ID {contract.ForwarderId} not found.");

		//		var client = await _unitOfWork.Users.GetByIdAsync(contract.ClientId)
		//			?? throw new ValidationException($"Client user with ID {contract.ClientId} not found.");

		//		string clientName;
		//		if (client.CompanyId != null)
		//		{
		//			var clientCompany = await _unitOfWork.Companies.GetByIdAsync(client.CompanyId.Value);
		//			clientName = clientCompany?.CompanyName ?? $"{client.FirstName} {client.LastName}";
		//		}
		//		else
		//		{
		//			clientName = $"{client.FirstName} {client.LastName}";
		//		}

		//		var contractDto = new ContractDetailsDto
		//		{
		//			ContractId = contract.Id,
		//			ContractNumber = contract.ContractNumber,
		//			ContractDate = contract.ContractDate,
		//			PenaltyRatePerHour = contract.PenaltyRatePerHour,
		//			MaxPenaltyPercent = contract.MaxPenaltyPercent,
		//			ClientName = clientName,
		//			ForwarderName = forwarder.CompanyName ?? "Unknown Forwarder",
		//			RouteId = contract.RouteId,
		//		};
		//		var allDrivers = await _unitOfWork.Drivers.GetAllAsync();
		//		var allVehicles = await _unitOfWork.Vehicles.GetAllAsync();
		//		foreach (var shipment in shipments)
		//		{
		//			var cargoType = allCargoTypes.FirstOrDefault(c => c.Id == shipment.CargoTypeId);

		//			var driver = shipment.DriverId.HasValue
		//				? allDrivers.FirstOrDefault(d => d.Id == shipment.DriverId.Value)
		//				: null;

		//			var vehicle = shipment.VehicleId.HasValue
		//				? allVehicles.FirstOrDefault(v => v.Id == shipment.VehicleId.Value)
		//				: null;

		//			contractDto.Shipments.Add(new ShipmentInfoDto
		//			{
		//				ShipmentId = shipment.Id,
		//				CargoType = cargoType?.TypeName ?? "Unknown",
		//				WeightKg = shipment.WeightKg,
		//				Priority = shipment.Priority,
		//				DriverName = driver != null ? $"{driver.FirstName} {driver.LastName}" : null,
		//				VehicleName = vehicle != null ? $"{vehicle.Brand} {vehicle.Model} {vehicle.ManufactureYear}" : null,
		//				ScheduledDeparture = shipment.ScheduledDeparture,
		//				ScheduledArrival = shipment.ScheduledArrival,
		//				Status = shipment.Status
		//			});
		//		}


		//		var clientUser = await _unitOfWork.Users.GetByIdAsync(contract.ClientId);
		//		if (clientUser.CompanyId != null)
		//		{
		//			var clientAdmins = (await _unitOfWork.Users.GetAllAsync())
		//				.Where(u => u.CompanyId == clientUser.CompanyId && u.UserRole == UserRole.CompanyAdmin);
		//			foreach (var admin in clientAdmins)
		//			{
		//				if (!string.IsNullOrEmpty(admin.Email))
		//					await _emailService.SendEmailAfterContractAcception(admin.Email, contractDto);
		//			}
		//		}
		//		else
		//		{
		//			await _emailService.SendEmailAfterContractAcception(clientUser.Email, contractDto);
		//		}
		//		var forwarderAdmins = (await _unitOfWork.Users.GetAllAsync())
		//			.Where(u => u.CompanyId == forwarder.Id && u.UserRole == UserRole.CompanyAdmin);

		//		foreach (var admin in forwarderAdmins)
		//		{
		//			if (!string.IsNullOrEmpty(admin.Email))
		//				await _emailService.SendEmailAfterContractAcception(admin.Email, contractDto);
		//		}

		//	}
		//	else
		//	{
		//		contract.ContractStatus = ContractStatus.Cancelled;

		//		if (shipments.Any())
		//		{
		//			Console.WriteLine($"Pronađeno {shipments.Count} pošiljaka za brisanje.");

		//			foreach (var shipment in shipments)
		//			{
		//				await _unitOfWork.Shipments.DeleteAsync(shipment); 
		//				Console.WriteLine($"Obrisana pošiljka: ID {shipment.Id}");
		//			}
		//		}

		//		await _unitOfWork.SaveChangesAsync();

		//		var client = await _unitOfWork.Users.GetByIdAsync(contract.ClientId)
		//			?? throw new ValidationException($"Client user with ID {contract.ClientId} not found.");

		//		if (!string.IsNullOrEmpty(client.Email))
		//			await _emailService.SendEmailAfterContractRejection(client.Email, contract);
		//	}


		//	return Unit.Value;
		//}
		public async Task<Unit> Handle(UpdateCarrierContractStatusCommand request, CancellationToken cancellationToken)
		{
			if (_httpContextAccessor.HttpContext == null)
				throw new ValidationException("HttpContext is null.");

			var userEmail = _httpContextAccessor.HttpContext.User
				.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

			if (string.IsNullOrEmpty(userEmail))
				throw new ValidationException("User email not found in token.");

			var currentUser = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException("User not found.");

			if (currentUser.CompanyId == null)
				throw new ValidationException("You don't work in any company.");

			var company = await _unitOfWork.Companies.GetByIdAsync(currentUser.CompanyId.Value)
				?? throw new ValidationException($"Company with ID {currentUser.CompanyId.Value} doesn't exist.");

			if (company.CompanyType != CompanyType.Carrier)
				throw new ValidationException("Company must have Carrier Company Type.");

			var contract = await _unitOfWork.Contracts.GetByIdAsync(request.Id)
				?? throw new ValidationException("Contract not found.");

			var shipments = (await _unitOfWork.Shipments.GetByContractIdAsync(contract.Id)).ToList();
			var allCargoTypes = await _unitOfWork.CargoTypes.GetAllAsync();

			if (request.ContractDto.ContractStatus == ContractStatus.Active)
			{
				if (!request.ContractDto.DriverId.HasValue || !request.ContractDto.VehicleId.HasValue)
					throw new ValidationException("DriverId and VehicleId are required when accepting a contract.");

				var contractDriver = await _unitOfWork.Drivers.GetByIdAsync(request.ContractDto.DriverId.Value)
					?? throw new ValidationException($"Driver with ID {request.ContractDto.DriverId.Value} doesn't exist.");

				if (contractDriver.DriverStatus != DriverStatus.Available)
					throw new ValidationException("This driver wasn't available.");

				var contractVehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.ContractDto.VehicleId.Value)
					?? throw new ValidationException($"Vehicle with ID {request.ContractDto.VehicleId.Value} doesn't exist.");

				if (contractVehicle.VehicleStatus != VehicleStatus.Free)
					throw new ValidationException("This vehicle wasn't available.");

				// Ažuriranje statusa
				contract.ContractStatus = ContractStatus.Active;
				await _unitOfWork.Contracts.UpdateAsync(contract);

				contractDriver.DriverStatus = DriverStatus.OnRoute;
				await _unitOfWork.Drivers.UpdateAsync(contractDriver);

				contractVehicle.VehicleStatus = VehicleStatus.OnRoute;
				await _unitOfWork.Vehicles.UpdateAsync(contractVehicle);

				foreach (var shipment in shipments)
				{
					shipment.DriverId = contractDriver.Id;
					shipment.VehicleId = contractVehicle.Id;
					shipment.Status = ShipmentStatus.Scheduled;
					await _unitOfWork.Shipments.UpdateAsync(shipment);
				}

				await _unitOfWork.SaveChangesAsync();

				// Klijent & špediter ID-jevi + imena samo “flat”
				var forwarder = await _unitOfWork.Companies.GetByIdAsync(contract.ForwarderId)
					?? throw new ValidationException($"Forwarder company with ID {contract.ForwarderId} not found.");

				var client = await _unitOfWork.Users.GetByIdAsync(contract.ClientId)
					?? throw new ValidationException($"Client user with ID {contract.ClientId} not found.");

				var contractDto = new ContractDetailsDto
				{
					ContractId = contract.Id,
					ContractNumber = contract.ContractNumber,
					ContractDate = contract.ContractDate,
					PenaltyRatePerHour = contract.PenaltyRatePerHour,
					MaxPenaltyPercent = contract.MaxPenaltyPercent,
					ClientName = $"{client.FirstName} {client.LastName}",
					ForwarderName = forwarder.CompanyName,
					RouteId = contract.RouteId
				};

				var allDrivers = await _unitOfWork.Drivers.GetAllAsync();
				var allVehicles = await _unitOfWork.Vehicles.GetAllAsync();

				foreach (var shipment in shipments)
				{
					var cargoType = allCargoTypes.FirstOrDefault(c => c.Id == shipment.CargoTypeId);
					var driver = shipment.DriverId.HasValue
						? allDrivers.FirstOrDefault(d => d.Id == shipment.DriverId.Value)
						: null;
					var vehicle = shipment.VehicleId.HasValue
						? allVehicles.FirstOrDefault(v => v.Id == shipment.VehicleId.Value)
						: null;

					contractDto.Shipments.Add(new ShipmentInfoDto
					{
						ShipmentId = shipment.Id,
						CargoType = cargoType?.TypeName ?? "Unknown",
						WeightKg = shipment.WeightKg,
						Priority = shipment.Priority,
						DriverName = driver != null ? $"{driver.FirstName} {driver.LastName}" : null,
						VehicleName = vehicle != null ? $"{vehicle.Brand} {vehicle.Model} {vehicle.ManufactureYear}" : null,
						ScheduledDeparture = shipment.ScheduledDeparture,
						ScheduledArrival = shipment.ScheduledArrival,
						Status = shipment.Status
					});
				}

				// Slanje mejlova (ostaje flat – koristi se samo Email i Id)
				if (client.CompanyId != null)
				{
					var clientAdmins = (await _unitOfWork.Users.GetAllAsync())
						.Where(u => u.CompanyId == client.CompanyId && u.UserRole == UserRole.CompanyAdmin);

					foreach (var admin in clientAdmins)
					{
						if (!string.IsNullOrEmpty(admin.Email))
							await _emailService.SendEmailAfterContractAcception(admin.Email, contractDto);
					}
				}
				else
				{
					await _emailService.SendEmailAfterContractAcception(client.Email, contractDto);
				}

				var forwarderAdmins = (await _unitOfWork.Users.GetAllAsync())
					.Where(u => u.CompanyId == forwarder.Id && u.UserRole == UserRole.CompanyAdmin);

				foreach (var admin in forwarderAdmins)
				{
					if (!string.IsNullOrEmpty(admin.Email))
						await _emailService.SendEmailAfterContractAcception(admin.Email, contractDto);
				}
			}
			else
			{
				contract.ContractStatus = ContractStatus.Cancelled;
				await _unitOfWork.Contracts.UpdateAsync(contract);

				if (shipments.Any())
				{
					foreach (var shipment in shipments)
					{
						await _unitOfWork.Shipments.DeleteAsync(shipment);
					}
				}

				await _unitOfWork.SaveChangesAsync();

				var client = await _unitOfWork.Users.GetByIdAsync(contract.ClientId)
					?? throw new ValidationException($"Client user with ID {contract.ClientId} not found.");

				if (!string.IsNullOrEmpty(client.Email))
					await _emailService.SendEmailAfterContractRejection(client.Email, contract);
			}

			return Unit.Value;
		}

	}


	public class ContractDetailsDto
	{
		public int ContractId { get; set; }
		public string ContractNumber { get; set; } = null!;
		public DateTime ContractDate { get; set; }
		public decimal PenaltyRatePerHour { get; set; }
		public decimal MaxPenaltyPercent { get; set; }
		public string ClientName { get; set; } = null!;
		public string ForwarderName { get; set; } = null!;
		public int RouteId { get; set; }
		public List<ShipmentInfoDto> Shipments { get; set; } = new List<ShipmentInfoDto>();
	}

	public class ShipmentInfoDto
	{
		public int ShipmentId { get; set; }
		public string? CargoType { get; set; }
		public double WeightKg { get; set; }
		public int Priority { get; set; }
		public string? DriverName { get; set; }
		public string? VehicleName { get; set; }
		public DateTime ScheduledDeparture { get; set; }
		public DateTime ScheduledArrival { get; set; }
		public ShipmentStatus Status { get; set; }
	}
}