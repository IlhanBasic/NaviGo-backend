using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Queries.ForwarderOffer;
using NaviGoApi.Application.DTOs.ForwarderOffer;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Application.CQRS.Handlers.ForwarderOffer
{
	public class GetAllForwarderOfferQueryHandler : IRequestHandler<GetAllForwarderOfferQuery, IEnumerable<ForwarderOfferDto?>>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public GetAllForwarderOfferQueryHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<IEnumerable<ForwarderOfferDto?>> Handle(GetAllForwarderOfferQuery request, CancellationToken cancellationToken)
		{
			if (_httpContextAccessor.HttpContext == null)
				throw new ValidationException("HttpContext is null.");

			var userEmail = _httpContextAccessor.HttpContext.User
				.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

			if (string.IsNullOrEmpty(userEmail))
				throw new ValidationException("User email not found in token.");

			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException("User not found.");

			if (user.UserStatus != UserStatus.Active)
				throw new ValidationException("User must be activated.");

			if (user.CompanyId == null && user.UserRole != UserRole.SuperAdmin)
				throw new ValidationException("User must belong to a company.");

			Domain.Entities.Company? company = null;
			if (user.CompanyId != null)
			{
				company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value)
					?? throw new ValidationException($"Company with ID {user.CompanyId.Value} doesn't exist.");
			}

			IEnumerable<Domain.Entities.ForwarderOffer> entities;

			if (company != null && company.CompanyType == CompanyType.Forwarder)
			{
				entities = await _unitOfWork.ForwarderOffers.GetByForwarderIdAsync(company.Id);
			}
			else
			{
				entities = await _unitOfWork.ForwarderOffers.GetAllAsync(request.Search);
			}
			var allCompanies = await _unitOfWork.Companies.GetAllAsync();
			var companiesDict = allCompanies.ToDictionary(c => c.Id, c => c);

			var forwarderOffersDto = entities.Select(entity => new ForwarderOfferDto
			{
				Id = entity.Id,
				CreatedAt = entity.CreatedAt,
				ExpiresAt = entity.ExpiresAt,
				CommissionRate = entity.CommissionRate,
				DiscountRate = entity.DiscountRate,
				ForwarderId = entity.ForwarderId,
				ForwarderOfferStatus = entity.ForwarderOfferStatus.ToString(),
				RejectionReason = entity.RejectionReason,
				RouteId = entity.RouteId,
				ForwarderCompanyName = companiesDict.TryGetValue(entity.ForwarderId, out var forwarderCompany)
					? forwarderCompany.CompanyName
					: string.Empty
			}).ToList();

			return forwarderOffersDto;
		}

	}
}
