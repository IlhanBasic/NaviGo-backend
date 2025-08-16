using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.Shipment;
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
	public class DeleteShipmentCommandHandler : IRequestHandler<DeleteShipmentCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public DeleteShipmentCommandHandler(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(DeleteShipmentCommand request, CancellationToken cancellationToken)
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
				throw new ValidationException("You are not allowed to add route.");
			var shipment = await _unitOfWork.Shipments.GetByIdAsync(request.Id);
			if (shipment == null)
				throw new ValidationException($"Shipment with ID {request.Id} not found.");
			var contract = await _unitOfWork.Contracts.GetByIdAsync(shipment.ContractId);
			if (contract == null)
				throw new ValidationException($"Contract with ID {shipment.ContractId} not found.");
			var route = await _unitOfWork.Routes.GetByIdAsync(contract.RouteId);
			if (route == null)
				throw new ValidationException($"Route with ID {contract.RouteId} not found.");
			var company = await _unitOfWork.Companies.GetByIdAsync(route.CompanyId);
			if (company == null)
				throw new ValidationException($"Company with ID {route.CompanyId} not found.");
			if (company.CompanyType != CompanyType.Carrier)
				throw new ValidationException($"Company must be Carrier type for update Shipment.");
			if (user.CompanyId != company.Id)
				throw new ValidationException($"You cannot delete shipment for wrong company.");
			await _unitOfWork.Shipments.DeleteAsync(shipment);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}
}
