using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.ForwarderOffer;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.ForwarderOffer
{
	public class AddForwarderOfferCommandHandler : IRequestHandler<AddForwarderOfferCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public AddForwarderOfferCommandHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(AddForwarderOfferCommand request, CancellationToken cancellationToken)
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

			if (user.UserRole != UserRole.CompanyAdmin)
				throw new ValidationException("Only CompanyAdmin can add a forwarder offer.");

			if (user.CompanyId != request.ForwarderOfferDto.ForwarderId)
				throw new ValidationException("User cannot create offers for other companies.");

			var acceptedOfferExists = await _unitOfWork.ForwarderOffers
				.GetByRouteIdAsync(request.ForwarderOfferDto.RouteId);

			if (acceptedOfferExists.Any(o => o.ForwarderOfferStatus == ForwarderOfferStatus.Accepted))
			{
				throw new ValidationException("A forwarder offer for this route has already been accepted.");
			}

			var forwarderExists = await _unitOfWork.Companies.GetByIdAsync(request.ForwarderOfferDto.ForwarderId);
			if (forwarderExists == null)
				throw new ValidationException("Forwarder does not exist.");
			if (forwarderExists.CompanyType == CompanyType.Carrier || forwarderExists.CompanyType == CompanyType.Client)
				throw new ValidationException("Forwarder cannot be Client or Carrier.");

			var routeExists = await _unitOfWork.Routes.GetByIdAsync(request.ForwarderOfferDto.RouteId);
			if (routeExists == null)
				throw new ValidationException("Route does not exist.");

			var existingOffer = await _unitOfWork.ForwarderOffers
				.GetByRouteIdAsync(request.ForwarderOfferDto.RouteId);

			if (existingOffer.Any(o => o.ForwarderId == request.ForwarderOfferDto.ForwarderId &&
									   o.ForwarderOfferStatus == ForwarderOfferStatus.Pending &&
									   o.ExpiresAt > DateTime.UtcNow))
			{
				throw new ValidationException("Active offer for this route already exists from this forwarder.");
			}

			var entity = _mapper.Map<Domain.Entities.ForwarderOffer>(request.ForwarderOfferDto);
			entity.CreatedAt = DateTime.UtcNow;

			if (entity.ExpiresAt <= entity.CreatedAt.AddHours(1))
				throw new ValidationException("Offer must be valid for at least 1 hour.");

			if (entity.CommissionRate + entity.DiscountRate > 100)
				throw new ValidationException("Sum of commission and discount cannot exceed 100%.");

			entity.ForwarderOfferStatus = ForwarderOfferStatus.Pending;
			entity.ExpiresAt= DateTime.SpecifyKind(entity.ExpiresAt, DateTimeKind.Utc);

			await _unitOfWork.ForwarderOffers.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}
}
