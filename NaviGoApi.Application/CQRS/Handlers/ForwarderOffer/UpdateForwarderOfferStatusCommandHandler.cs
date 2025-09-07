using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.ForwarderOffer;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

public class UpdateForwarderOfferStatusCommandHandler : IRequestHandler<UpdateForwarderOfferStatusCommand, Unit>
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IHttpContextAccessor _httpContextAccessor;

	public UpdateForwarderOfferStatusCommandHandler(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
	{
		_unitOfWork = unitOfWork;
		_httpContextAccessor = httpContextAccessor;
	}

	public async Task<Unit> Handle(UpdateForwarderOfferStatusCommand request, CancellationToken cancellationToken)
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
		Console.WriteLine(request.Id);
		var offer = await _unitOfWork.ForwarderOffers.GetByIdAsync(request.Id)
			?? throw new ValidationException("Forwarder offer not found.");
		var route = await _unitOfWork.Routes.GetByIdAsync(offer.RouteId);
		if (route == null)
			throw new ValidationException($"Route with ID {offer.RouteId} doesn't exists.");
		if (user.CompanyId == null)
			throw new ValidationException("User must have company.");
		if (user.UserRole != UserRole.CompanyAdmin && user.CompanyId != route.CompanyId)
			throw new ValidationException("Only the CompanyAdmin Carrier can change the status of the offer.");
		if (offer.ExpiresAt < DateTime.UtcNow && request.ForwarderOfferDto.NewStatus == ForwarderOfferStatus.Accepted)
			throw new ValidationException("This forwarder offer cannot be accepted because it's expired.");

		if (request.ForwarderOfferDto.NewStatus == ForwarderOfferStatus.Rejected &&
			string.IsNullOrWhiteSpace(request.ForwarderOfferDto.RejectionReason))
			throw new ValidationException("Rejection reason must be provided when rejecting an offer.");

		offer.ForwarderOfferStatus = request.ForwarderOfferDto.NewStatus;
		offer.RejectionReason = request.ForwarderOfferDto.NewStatus == ForwarderOfferStatus.Rejected
								? request.ForwarderOfferDto.RejectionReason
								: null;

		await _unitOfWork.ForwarderOffers.UpdateAsync(offer);
		await _unitOfWork.SaveChangesAsync();

		return Unit.Value;
	}
}
