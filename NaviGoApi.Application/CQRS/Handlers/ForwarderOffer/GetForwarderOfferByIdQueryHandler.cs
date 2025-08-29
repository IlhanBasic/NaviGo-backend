using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Queries.ForwarderOffer;
using NaviGoApi.Application.DTOs.ForwarderOffer;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace NaviGoApi.Application.CQRS.Handlers.ForwarderOffer
{
	public class GetForwarderOfferByIdQueryHandler : IRequestHandler<GetForwarderOfferByIdQuery, ForwarderOfferDto?>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public GetForwarderOfferByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<ForwarderOfferDto?> Handle(GetForwarderOfferByIdQuery request, CancellationToken cancellationToken)
		{
			if (_httpContextAccessor.HttpContext == null)
				throw new ValidationException("HttpContext is null.");

			var userEmail = _httpContextAccessor.HttpContext.User
				.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

			if (string.IsNullOrEmpty(userEmail))
				throw new ValidationException("User email not found in token.");

			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException("User not found.");

			if (user.UserStatus != Domain.Entities.UserStatus.Active)
				throw new ValidationException("User must be activated.");

			var entity = await _unitOfWork.ForwarderOffers.GetByIdAsync(request.Id);
			if (entity == null) return null;
			var company = await _unitOfWork.Companies.GetByIdAsync(entity.ForwarderId);
			var forwarderOfferdto = new ForwarderOfferDto
			{
				Id = entity.Id,
				CreatedAt = DateTime.UtcNow,
				ExpiresAt = DateTime.UtcNow,
				CommissionRate = entity.CommissionRate,
				DiscountRate = entity.DiscountRate,
				ForwarderId = entity.Id,
				ForwarderOfferStatus = entity.ForwarderOfferStatus.ToString(),
				RejectionReason = entity.RejectionReason,
				RouteId = entity.RouteId,
				ForwarderCompanyName = company != null ? company.CompanyName : ""
			};
			//return _mapper.Map<ForwarderOfferDto>(entity);
			return forwarderOfferdto;
		}
	}
}
