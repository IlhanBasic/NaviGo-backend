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
			if (userCompany == null ||
				!(userCompany.CompanyType == CompanyType.Forwarder || userCompany.CompanyType == CompanyType.Carrier))
			{
				throw new ValidationException("Your company is not allowed to view shipment documents.");
			}
			
			var allDocuments = await _unitOfWork.ShipmentDocuments.GetAllAsync(request.Search);

			var shipmentIds = allDocuments.Select(d => d.ShipmentId).Distinct().ToList();
			var allShipments = await _unitOfWork.Shipments.GetAllAsync(); 

			var shipmentLookup = allShipments
				.Where(s => shipmentIds.Contains(s.Id))
				.ToDictionary(s => s.Id, s => s);

			var companyDocuments = allDocuments
				.Where(doc =>
					shipmentLookup.TryGetValue(doc.ShipmentId, out var shipment) &&
					shipment.Contract != null &&
					shipment.Contract.ForwarderId == user.CompanyId)
				.ToList();

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
