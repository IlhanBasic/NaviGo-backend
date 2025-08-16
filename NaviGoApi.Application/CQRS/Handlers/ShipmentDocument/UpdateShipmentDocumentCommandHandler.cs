using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.ShipmentDocument;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.ShipmentDocument
{
	public class UpdateShipmentDocumentCommandHandler : IRequestHandler<UpdateShipmentDocumentCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public UpdateShipmentDocumentCommandHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(UpdateShipmentDocumentCommand request, CancellationToken cancellationToken)
		{
			if (_httpContextAccessor.HttpContext == null)
				throw new ValidationException("HttpContext is null.");
			var userEmail = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
			if (string.IsNullOrEmpty(userEmail))
				throw new ValidationException("User email not found in token.");
			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException("User not found.");
			if (user.UserStatus != UserStatus.Active)
				throw new ValidationException("Your account is not activated.");
			if (user.UserRole != UserRole.CompanyAdmin)
				throw new ValidationException("Only Company Admins can update shipment documents.");
			if (user.CompanyId == null)
				throw new ValidationException("User is not associated with any company.");
			var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value)
				?? throw new ValidationException("Company not found.");
			var shipmentDocument = await _unitOfWork.ShipmentDocuments.GetByIdAsync(request.Id)
				?? throw new ValidationException($"Shipment Document with ID {request.Id} doesn't exist.");
			var shipment = await _unitOfWork.Shipments.GetByIdAsync(shipmentDocument.ShipmentId)
				?? throw new ValidationException($"Shipment with ID {shipmentDocument.ShipmentId} not found.");
			var contract = await _unitOfWork.Contracts.GetByIdAsync(shipment.ContractId)
				?? throw new ValidationException($"Contract with ID {shipment.ContractId} not found.");
			var forwarder = await _unitOfWork.Companies.GetByIdAsync(contract.ForwarderId)
				?? throw new ValidationException($"Forwarder company with ID {contract.ForwarderId} not found.");
			var route = await _unitOfWork.Routes.GetByIdAsync(contract.RouteId)
				?? throw new ValidationException($"Route with ID {contract.RouteId} not found.");
			if (company.Id != forwarder.Id && company.Id != route.CompanyId)
				throw new ValidationException("You are not allowed to update documents for this shipment.");
			_mapper.Map(request.ShipmentDocumentDto, shipmentDocument);
			shipmentDocument.Verified = true;
			shipmentDocument.VerifiedByUserId = user.Id;
			await _unitOfWork.ShipmentDocuments.UpdateAsync(shipmentDocument);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}

	}
}
