using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.ShipmentDocument;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.ShipmentDocument
{
	public class DeleteShipmentDocumentCommandHandler : IRequestHandler<DeleteShipmentDocumentCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public DeleteShipmentDocumentCommandHandler(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(DeleteShipmentDocumentCommand request, CancellationToken cancellationToken)
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
				throw new ValidationException("Only Company Admins can delete shipment documents.");

			var shipmentDocument = await _unitOfWork.ShipmentDocuments.GetByIdAsync(request.Id);
			if (shipmentDocument == null)
				throw new ValidationException($"ShipmentDocument with ID {request.Id} not found.");

			var shipment = await _unitOfWork.Shipments.GetByIdAsync(shipmentDocument.ShipmentId);
			if (shipment == null)
				throw new ValidationException($"Shipment with ID {shipmentDocument.ShipmentId} not found.");

			var contract = await _unitOfWork.Routes.GetByIdAsync(shipment.ContractId);
			if (contract == null)
				throw new ValidationException($"Contract with ID {shipment.ContractId} not found.");

			var company = await _unitOfWork.Companies.GetByIdAsync(contract.CompanyId);
			if (company == null)
				throw new ValidationException($"Company with ID {contract.CompanyId} not found.");

			if (company.CompanyType != CompanyType.Carrier && company.CompanyType != CompanyType.Forwarder)
				throw new ValidationException("Only Carrier or Forwarder companies can own shipment documents.");

			if (company.Id != user.CompanyId)
				throw new ValidationException("You are not allowed to delete documents from another company.");

			await _unitOfWork.ShipmentDocuments.DeleteAsync(request.Id);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}

	}
}
