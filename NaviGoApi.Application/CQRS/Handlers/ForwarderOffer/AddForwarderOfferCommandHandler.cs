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
	public class AddForwarderOfferCommandHandler : IRequestHandler<AddForwarderOfferCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public AddForwarderOfferCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(AddForwarderOfferCommand request, CancellationToken cancellationToken)
		{
			var forwarderExists = await _unitOfWork.Companies.GetByIdAsync(request.ForwarderOfferDto.ForwarderId);
			if (forwarderExists == null)
				throw new ValidationException("Forwarder does not exist.");

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
			entity.ForwarderOfferStatus=Domain.Entities.ForwarderOfferStatus.Pending;
			await _unitOfWork.ForwarderOffers.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}

}
