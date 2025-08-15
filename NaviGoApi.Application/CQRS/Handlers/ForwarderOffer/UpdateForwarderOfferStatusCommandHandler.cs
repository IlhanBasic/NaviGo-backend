using MediatR;
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

	public UpdateForwarderOfferStatusCommandHandler(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async Task<Unit> Handle(UpdateForwarderOfferStatusCommand request, CancellationToken cancellationToken)
	{
		var offer = await _unitOfWork.ForwarderOffers.GetByIdAsync(request.ForwarderOfferDto.ForwarderOfferId)
			?? throw new ValidationException("Forwarder offer not found.");
		if (offer.ExpiresAt < DateTime.UtcNow && request.ForwarderOfferDto.NewStatus==ForwarderOfferStatus.Accepted)
			throw new ValidationException("This forwarder offer cannot be accepted because it's expired.");
		if (request.ForwarderOfferDto.NewStatus == ForwarderOfferStatus.Rejected && string.IsNullOrWhiteSpace(request.ForwarderOfferDto.RejectionReason))
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
