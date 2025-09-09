using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.Payment;
using NaviGoApi.Application.Services;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Payment
{
	public class UpdatePaymentCommandHandler : IRequestHandler<UpdatePaymentCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IEmailService _emailService;

		public UpdatePaymentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor,IEmailService emailService)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_httpContextAccessor = httpContextAccessor;
			_emailService = emailService;
		}


		public async Task<Unit> Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
		{
			var httpContext = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("HttpContext is not available.");

			var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value
				?? throw new ValidationException("User email not found in authentication token.");

			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException($"User with email '{userEmail}' not found.");

			if (user.UserStatus != UserStatus.Active)
				throw new ValidationException("User must be active to perform this operation.");

			if (user.CompanyId == null)
				throw new ValidationException("Only company users can update payments.");

			var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value)
				?? throw new ValidationException("Company not found.");

			if (company.CompanyStatus != CompanyStatus.Approved)
				throw new ValidationException("Company must be approved to perform this action.");

			if (company.CompanyType != CompanyType.Forwarder && company.CompanyType != CompanyType.Carrier)
				throw new ValidationException("Only Forwarder or Carrier companies can update payments.");

			if (user.UserRole != UserRole.CompanyAdmin)
				throw new ValidationException("You must be a company admin to update payments.");

			var payment = await _unitOfWork.Payments.GetByIdAsync(request.Id)
				?? throw new ValidationException($"Payment with Id {request.Id} not found.");

			payment.PaymentStatus = request.PaymentDto.PaymentStatus;

			var contract = await _unitOfWork.Contracts.GetByIdAsync(payment.ContractId)
					?? throw new ValidationException("Contract must exist before this action.");

			var shipments = (await _unitOfWork.Shipments.GetByContractIdAsync(contract.Id)).ToList();
			if (!shipments.Any())
				throw new ValidationException("This Contract doesn't have any shipments.");

			if (request.PaymentDto.PaymentStatus == PaymentStatus.Verified)
			{
				foreach (var s in shipments)
				{
					if (s.DriverId == null) throw new ValidationException($"Shipment {s.Id} must have a driver assigned.");
					if (s.VehicleId == null) throw new ValidationException($"Shipment {s.Id} must have a vehicle assigned.");
				}

				var driverIds = shipments.Select(s => s.DriverId!.Value).Distinct().ToList();
				var vehicleIds = shipments.Select(s => s.VehicleId!.Value).Distinct().ToList();

				// batch update Drivers
				var allDrivers = await _unitOfWork.Drivers.GetAllAsync();
				foreach (var drv in allDrivers.Where(d => driverIds.Contains(d.Id)))
					drv.DriverStatus = DriverStatus.OnRoute;

				// batch update Vehicles
				var allVehicles = await _unitOfWork.Vehicles.GetAllAsync();
				foreach (var veh in allVehicles.Where(v => vehicleIds.Contains(v.Id)))
					veh.VehicleStatus = VehicleStatus.OnRoute;

				// update Shipments
				foreach (var s in shipments)
					s.Status = ShipmentStatus.InTransit;

				var clientUser = await _unitOfWork.Users.GetByIdAsync(contract.ClientId)
					?? throw new ValidationException("Client must exist in contract.");

				if (clientUser.UserRole == UserRole.RegularUser)
					await _emailService.SendEmailAfterPaymentAcception(clientUser.Email, payment);
				else
				{
					if (clientUser.CompanyId == null) throw new ValidationException("Client must have a company.");
					var allUsers = await _unitOfWork.Users.GetAllAsync();
					foreach (var u in allUsers.Where(u => u.CompanyId == clientUser.CompanyId && u.UserRole == UserRole.CompanyAdmin))
						await _emailService.SendEmailAfterPaymentAcception(u.Email, payment);
				}
			}
			else if (request.PaymentDto.PaymentStatus == PaymentStatus.Rejected)
			{
				var clientUser = await _unitOfWork.Users.GetByIdAsync(contract.ClientId)
					?? throw new ValidationException("Client must exist in contract.");

				if (clientUser.UserRole == UserRole.RegularUser)
					await _emailService.SendEmailAfterPaymentRejection(clientUser.Email, payment);
				else
				{
					if (clientUser.CompanyId == null) throw new ValidationException("Client must have a company.");
					var allUsers = await _unitOfWork.Users.GetAllAsync();
					foreach (var u in allUsers.Where(u => u.CompanyId == clientUser.CompanyId && u.UserRole == UserRole.CompanyAdmin))
						await _emailService.SendEmailAfterPaymentRejection(u.Email, payment);
				}
			}

			// Save everything in one call
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}

	}

}
