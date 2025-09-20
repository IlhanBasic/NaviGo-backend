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
		private readonly IUnitOfWork _unitOfWork; private readonly IMapper _mapper; private readonly IHttpContextAccessor _httpContextAccessor; private readonly IEmailService _emailService; public UpdatePaymentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor, IEmailService emailService)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_httpContextAccessor = httpContextAccessor;
			_emailService = emailService;
		}
		public async Task<Unit> Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
		{
			var httpContext = _httpContextAccessor.HttpContext ??
			  throw new InvalidOperationException("HttpContext is not available.");
			var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
			if (string.IsNullOrWhiteSpace(userEmail))
				throw new ValidationException("User email not found in authentication token.");

			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail) ??
			  throw new ValidationException($"User with email '{userEmail}' not found.");

			if (user.UserStatus != UserStatus.Active)
				throw new ValidationException("User must be active to perform this operation.");
			if (user.CompanyId == null)
				throw new ValidationException("Only company users can update payments.");

			var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value) ??
			  throw new ValidationException("Company not found.");

			if (company.CompanyStatus != CompanyStatus.Approved)
				throw new ValidationException("Company must be approved to perform this action.");
			if (company.CompanyType != CompanyType.Forwarder && company.CompanyType != CompanyType.Carrier)
				throw new ValidationException("Only Forwarder or Carrier companies can update payments.");
			if (user.UserRole != UserRole.CompanyAdmin)
				throw new ValidationException("You must be a company admin to update payments.");

			var payment = await _unitOfWork.Payments.GetByIdAsync(request.Id) ??
			  throw new ValidationException($"Payment with Id {request.Id} not found.");

			payment.PaymentStatus = request.PaymentDto.PaymentStatus;

			var contract = await _unitOfWork.Contracts.GetByIdAsync(payment.ContractId) ??
			  throw new ValidationException("Contract must exist before this action.");

			var shipments = (await _unitOfWork.Shipments.GetByContractIdAsync(contract.Id)).ToList();
			if (!shipments.Any())
				throw new ValidationException("This Contract doesn't have any shipments.");

			if (request.PaymentDto.PaymentStatus == PaymentStatus.Verified)
			{
				// Proveri da li svi imaju dodeljene vozilo i vozača
				foreach (var s in shipments)
				{
					if (s.DriverId == null) throw new ValidationException($"Shipment {s.Id} must have a driver assigned.");
					if (s.VehicleId == null) throw new ValidationException($"Shipment {s.Id} must have a vehicle assigned.");
				}

				var driverIds = shipments.Select(s => s.DriverId!.Value).Distinct();
				var vehicleIds = shipments.Select(s => s.VehicleId!.Value).Distinct();

				var drivers = await _unitOfWork.Drivers.GetAllAsync();
				var vehicles = await _unitOfWork.Vehicles.GetAllAsync();

				foreach (var dId in driverIds)
				{
					var drv = drivers.FirstOrDefault(d => d.Id == dId);
					if (drv != null)
					{
						drv.DriverStatus = DriverStatus.OnRoute;
						await _unitOfWork.Drivers.UpdateAsync(drv);
					}
				}

				foreach (var vId in vehicleIds)
				{
					var veh = vehicles.FirstOrDefault(v => v.Id == vId);
					if (veh != null)
					{
						veh.VehicleStatus = VehicleStatus.OnRoute;
						await _unitOfWork.Vehicles.UpdateAsync(veh);
					}
				}

				foreach (var s in shipments)
				{
					s.Status = ShipmentStatus.InTransit;
					await _unitOfWork.Shipments.UpdateAsync(s);
				}

				await _unitOfWork.Contracts.UpdateAsync(contract);
			}

			// Uvek updateuj payment status u bazi pre slanja mejlova
			await _unitOfWork.Payments.UpdateAsync(payment);
			await _unitOfWork.SaveChangesAsync();

			// Slanje mejlova
			var clientUser = await _unitOfWork.Users.GetByIdAsync(contract.ClientId);
			if (clientUser == null) throw new ValidationException("Client must exist in contract.");

			if (request.PaymentDto.PaymentStatus == PaymentStatus.Verified)
			{
				if (clientUser.UserRole == UserRole.RegularUser)
				{
					await _emailService.SendEmailAfterPaymentAcception(clientUser.Email, payment);
				}
				else if (clientUser.CompanyId != null && clientUser.UserRole == UserRole.CompanyAdmin)
				{
					var allUsers = await _unitOfWork.Users.GetAllAsync();
					foreach (var u in allUsers.Where(u => u.CompanyId == clientUser.CompanyId && u.UserRole == UserRole.CompanyAdmin))
					{
						await _emailService.SendEmailAfterPaymentAcception(u.Email, payment);
					}
				}
			}
            //else if (request.PaymentDto.PaymentStatus == PaymentStatus.Rejected)
            //{
            //	if (clientUser.UserRole == UserRole.RegularUser || clientUser.CompanyId != null)
            //	{
            //		if (clientUser.UserRole == UserRole.RegularUser)
            //		{
            //			await _emailService.SendEmailAfterPaymentRejection(clientUser.Email, payment);
            //		}
            //		else
            //		{
            //			var allUsers = await _unitOfWork.Users.GetAllAsync();
            //			foreach (var u in allUsers.Where(u => u.CompanyId == clientUser.CompanyId && u.UserRole == UserRole.CompanyAdmin))
            //			{
            //				await _emailService.SendEmailAfterPaymentRejection(u.Email, payment);
            //			}
            //		}
            //	}
            //}
            else if (request.PaymentDto.PaymentStatus == PaymentStatus.Rejected)
            {
                // Oslobodi sve vozače i vozila
                var driverIds = shipments.Select(s => s.DriverId).Where(id => id != null).Select(id => id!.Value).Distinct();
                var vehicleIds = shipments.Select(s => s.VehicleId).Where(id => id != null).Select(id => id!.Value).Distinct();

                var drivers = await _unitOfWork.Drivers.GetAllAsync();
                var vehicles = await _unitOfWork.Vehicles.GetAllAsync();

                foreach (var dId in driverIds)
                {
                    var drv = drivers.FirstOrDefault(d => d.Id == dId);
                    if (drv != null)
                    {
                        drv.DriverStatus = DriverStatus.Available; // oslobodi vozača
                        await _unitOfWork.Drivers.UpdateAsync(drv);
                    }
                }

                foreach (var vId in vehicleIds)
                {
                    var veh = vehicles.FirstOrDefault(v => v.Id == vId);
                    if (veh != null)
                    {
                        veh.VehicleStatus = VehicleStatus.Free; // oslobodi vozilo
                        await _unitOfWork.Vehicles.UpdateAsync(veh);
                    }
                }

                // Poništi status svih shipment-a
                foreach (var s in shipments)
                {
                    s.Status = ShipmentStatus.Cancelled; // ili neki default status
                    s.DriverId = null; // ukloni vozača
                    s.VehicleId = null; // ukloni vozilo
                    await _unitOfWork.Shipments.UpdateAsync(s);
                }

                // Poništi ugovor
                contract.ContractStatus = ContractStatus.Cancelled;
                await _unitOfWork.Contracts.UpdateAsync(contract);

                // Sačuvaj promene pre slanja mejlova
                await _unitOfWork.SaveChangesAsync();

                // Slanje mejlova
                if (clientUser.UserRole == UserRole.RegularUser)
                {
                    await _emailService.SendEmailAfterPaymentRejection(clientUser.Email, payment);
                }
                else
                {
                    var allUsers = await _unitOfWork.Users.GetAllAsync();
                    foreach (var u in allUsers.Where(u => u.CompanyId == clientUser.CompanyId && u.UserRole == UserRole.CompanyAdmin))
                    {
                        await _emailService.SendEmailAfterPaymentRejection(u.Email, payment);
                    }
                }
            }

            return Unit.Value;
		}

	}
}