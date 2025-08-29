using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.ForwarderOffer;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.ForwarderOffer
{
	public class UpdateForwarderOfferCommandHandler : IRequestHandler<UpdateForwarderOfferCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public UpdateForwarderOfferCommandHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(UpdateForwarderOfferCommand request, CancellationToken cancellationToken)
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
				throw new ValidationException("Only CompanyAdmin can update a forwarder offer.");

			var entity = await _unitOfWork.ForwarderOffers.GetByIdAsync(request.Id);
			if (entity == null)
				throw new ValidationException($"ForwarderOffer with id {request.Id} not found.");

			if (user.CompanyId != entity.ForwarderId)
				throw new ValidationException("User cannot update offers from other companies.");
			if (entity.ExpiresAt <= DateTime.UtcNow)
				throw new ValidationException("Cannot update an expired offer.");

			if (entity.ForwarderOfferStatus == ForwarderOfferStatus.Accepted &&
				request.ForwarderOfferDto.ForwarderOfferStatus != ForwarderOfferStatus.Accepted)
				throw new ValidationException("Cannot change status of an accepted offer.");

			if (request.ForwarderOfferDto.CommissionRate.HasValue &&
				request.ForwarderOfferDto.DiscountRate.HasValue &&
				request.ForwarderOfferDto.CommissionRate.Value + request.ForwarderOfferDto.DiscountRate.Value > 100)
				throw new ValidationException("Sum of commission and discount cannot exceed 100%.");

			if (request.ForwarderOfferDto.ExpiresAt.HasValue &&
				request.ForwarderOfferDto.ExpiresAt <= DateTime.UtcNow)
				throw new ValidationException("ExpiresAt must be a future date.");
			_mapper.Map(request.ForwarderOfferDto, entity);
			entity.ExpiresAt = DateTime.SpecifyKind(entity.ExpiresAt, DateTimeKind.Utc);
			await _unitOfWork.ForwarderOffers.UpdateAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}
}
