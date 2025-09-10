using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Queries.ShipmentDocument;
using NaviGoApi.Application.DTOs.ShipmentDocument;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.ShipmentDocument
{
	public class GetAllShipmentDocumentQueryHandler : IRequestHandler<GetAllShipmentDocumentQuery, IEnumerable<ShipmentDocumentDto?>>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public GetAllShipmentDocumentQueryHandler(
			IMapper mapper,
			IUnitOfWork unitOfWork,
			IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<IEnumerable<ShipmentDocumentDto?>> Handle(GetAllShipmentDocumentQuery request, CancellationToken cancellationToken)
		{
			var httpContext = _httpContextAccessor.HttpContext
				?? throw new ValidationException("HttpContext is null.");

			var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
			if (string.IsNullOrEmpty(userEmail))
				throw new ValidationException("User email not found in token.");

			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException("User not found.");

			if (user.UserStatus != UserStatus.Active)
				throw new ValidationException("User must be activated.");

			if (user.UserRole != UserRole.CompanyAdmin)
				throw new ValidationException("Only Company Admins can view shipment documents.");

			if (user.CompanyId == null)
				throw new ValidationException("You don't have a company.");

			var userCompany = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);
			if (userCompany == null)
				throw new ValidationException("Your company doesn't exist.");
			var allDocuments = await _unitOfWork.ShipmentDocuments.GetAllAsync(request.Search);
			var shipmentIds = allDocuments.Select(d => d.ShipmentId).Distinct().ToList();
			var allShipments = await _unitOfWork.Shipments.GetAllAsync();
			var shipmentLookup = allShipments
				.Where(s => shipmentIds.Contains(s.Id))
				.ToDictionary(s => s.Id, s => s);

			var companyDocuments = new List<Domain.Entities.ShipmentDocument>();

			foreach (var doc in allDocuments)
			{
				if (!shipmentLookup.TryGetValue(doc.ShipmentId, out var shipment))
					continue;

				var contract = await _unitOfWork.Contracts.GetByIdAsync(shipment.ContractId);
				if (contract == null) continue;

				bool canSee = false;

				switch (userCompany.CompanyType)
				{
					case CompanyType.Forwarder:
						canSee = contract.ForwarderId == user.CompanyId;
						break;

					case CompanyType.Carrier:
						if (shipment.VehicleId != null)
						{
							var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(shipment.VehicleId.Value);
							canSee = vehicle != null && vehicle.CompanyId == user.CompanyId;
						}
						break;
				}

				if (canSee)
				{
					companyDocuments.Add(doc);
				}
			}

			// Mapiranje u DTO
			return companyDocuments.Select(doc => new ShipmentDocumentDto
			{
				Id = doc.Id,
				ShipmentId = doc.ShipmentId,
				DocumentType = doc.DocumentType.ToString(),
				FileUrl = doc.FileUrl,
				UploadDate = doc.UploadDate,
				Verified = doc.Verified,
				VerifiedByUserId = doc.VerifiedByUserId
			}).ToList();
		}
	}
}
