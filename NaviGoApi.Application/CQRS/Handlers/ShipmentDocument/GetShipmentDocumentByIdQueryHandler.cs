using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Queries.ShipmentDocument;
using NaviGoApi.Application.DTOs.ShipmentDocument;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.ShipmentDocument
{
	public class GetShipmentDocumentByIdQueryHandler : IRequestHandler<GetShipmentDocumentByIdQuery, ShipmentDocumentDto?>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public GetShipmentDocumentByIdQueryHandler(
			IMapper mapper,
			IUnitOfWork unitOfWork,
			IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<ShipmentDocumentDto?> Handle(GetShipmentDocumentByIdQuery request, CancellationToken cancellationToken)
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
				throw new ValidationException("Only Company Admins are allowed to view shipment documents.");

			if (user.CompanyId == null)
				throw new ValidationException("User is not associated with any company.");

			var document = await _unitOfWork.ShipmentDocuments.GetByIdAsync(request.Id);
			if (document == null) return null;

			var shipment = await _unitOfWork.Shipments.GetByIdAsync(document.ShipmentId)
				?? throw new ValidationException("Shipment not found.");

			var contract = await _unitOfWork.Contracts.GetByIdAsync(shipment.ContractId)
				?? throw new ValidationException("Contract not found.");

			var forwarder = await _unitOfWork.Companies.GetByIdAsync(contract.ForwarderId)
				?? throw new ValidationException("Forwarder not found.");

			var route = await _unitOfWork.Routes.GetByIdAsync(contract.RouteId)
				?? throw new ValidationException("Route not found.");
			if (user.CompanyId != forwarder.Id && user.CompanyId != route.CompanyId)
				throw new ValidationException("You do not have permission to view this shipment document.");

			return _mapper.Map<ShipmentDocumentDto>(document);
		}
	}
}
