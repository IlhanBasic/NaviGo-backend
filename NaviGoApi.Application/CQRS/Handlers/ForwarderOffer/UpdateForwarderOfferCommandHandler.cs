using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.ForwarderOffer;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.ForwarderOffer
{
	public class UpdateForwarderOfferCommandHandler : IRequestHandler<UpdateForwarderOfferCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public UpdateForwarderOfferCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		//public async Task<Unit> Handle(UpdateForwarderOfferCommand request, CancellationToken cancellationToken)
		//{
		//	var entity = await _unitOfWork.ForwarderOffers.GetByIdAsync(request.Id);
		//	if (entity == null)
		//		throw new KeyNotFoundException($"ForwarderOffer with id {request.Id} not found.");

		//	// Map update DTO to entity
		//	_mapper.Map(request.ForwarderOfferDto, entity);

		//	await _unitOfWork.ForwarderOffers.UpdateAsync(entity);
		//	await _unitOfWork.SaveChangesAsync();

		//	return Unit.Value;
		//}
		public async Task<Unit> Handle(UpdateForwarderOfferCommand request, CancellationToken cancellationToken)
		{
			var entity = await _unitOfWork.ForwarderOffers.GetByIdAsync(request.Id);
			if (entity == null)
				throw new ValidationException($"ForwarderOffer with id {request.Id} not found.");

			// Biznis provere
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

			// Map update DTO to entity
			_mapper.Map(request.ForwarderOfferDto, entity);

			await _unitOfWork.ForwarderOffers.UpdateAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}
}
