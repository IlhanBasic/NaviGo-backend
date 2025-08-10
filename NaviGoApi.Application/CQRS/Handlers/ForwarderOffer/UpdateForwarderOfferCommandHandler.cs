using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.ForwarderOffer;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
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

		public async Task<Unit> Handle(UpdateForwarderOfferCommand request, CancellationToken cancellationToken)
		{
			var entity = await _unitOfWork.ForwarderOffers.GetByIdAsync(request.Id);
			if (entity == null)
				throw new KeyNotFoundException($"ForwarderOffer with id {request.Id} not found.");

			// Map update DTO to entity
			_mapper.Map(request.ForwarderOfferDto, entity);

			await _unitOfWork.ForwarderOffers.UpdateAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}
}
