using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.Vehicle;
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
	public class DeleteVehicleCommandHandler : IRequestHandler<DeleteVehicleCommand>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public DeleteVehicleCommandHandler(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(DeleteVehicleCommand request, CancellationToken cancellationToken)
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
			if (user.UserRole != UserRole.CompanyAdmin)
				throw new ValidationException("You are not allowed to add vehicle.");

			var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.VehicleId);
			if (vehicle == null)
			{
				throw new ValidationException($"Vehicle with Id {request.VehicleId} not found.");
			}
			var company = await _unitOfWork.Companies.GetByIdAsync(vehicle.CompanyId);
			if (company == null)
			{
				throw new ValidationException($"Company with ID {vehicle.CompanyId} does not exist.");
			}
			if (company.CompanyStatus == Domain.Entities.CompanyStatus.Pending)
				throw new ValidationException("Pending company cannot delete vehicles.");
			if (user.CompanyId != company.Id)
				throw new ValidationException("You cannot delete vehicle for wrong company.");
			if (company.CompanyType != Domain.Entities.CompanyType.Carrier)
				throw new ValidationException("Only Carrier companies can have registrated vehicles on this platform");
			await _unitOfWork.Vehicles.DeleteAsync(vehicle);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}
}
