using AutoMapper;
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
	public class AddShipmentDocumentCommandHandler : IRequestHandler<AddShipmentDocumentCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public AddShipmentDocumentCommandHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(AddShipmentDocumentCommand request, CancellationToken cancellationToken)
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
				throw new ValidationException("You are not allowed to add shipment documents.");
			if (user.CompanyId == null)
				throw new ValidationException("User doesn't work in any company.");
			var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);
			if (company == null)
				throw new ValidationException("User is not associated with any company.");
			if (company.CompanyType != CompanyType.Forwarder && company.CompanyType != CompanyType.Carrier)
				throw new ValidationException("You must work in Forwarder or Carrier company if you want to add shipment document.");
			var shipment = await _unitOfWork.Shipments.GetByIdAsync(request.ShipmentDocumentDto.ShipmentId);
			if(shipment == null)
				throw new ValidationException("Company isn't valid.");
			var contract = await _unitOfWork.Contracts.GetByIdAsync(shipment.ContractId);
			if(contract == null)
				throw new ValidationException("Contract isn't valid.");
			var forwarder = await _unitOfWork.Companies.GetByIdAsync(contract.ForwarderId);
			if(forwarder == null)
				throw new ValidationException("Forwarder isn't valid.");
			var route = await _unitOfWork.Routes.GetByIdAsync(contract.RouteId);
			if(route == null)
				throw new ValidationException("Route isn't valid.");
			if (company.Id != forwarder.Id &&
				company.Id != route.CompanyId)
				throw new ValidationException("User doesn't have permission to upload documents for this shipment.");
			var entity = _mapper.Map<Domain.Entities.ShipmentDocument>(request.ShipmentDocumentDto);
			entity.Verified = true;
			entity.VerifiedByUserId = user.Id;
			entity.UploadDate = DateTime.UtcNow;
			await _unitOfWork.ShipmentDocuments.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}

}
