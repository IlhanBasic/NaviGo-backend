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
			var entity = _mapper.Map<Domain.Entities.ForwarderOffer>(request.ForwarderOfferDto);

			entity.ForwarderOfferStatus=Domain.Entities.ForwarderOfferStatus.Pending;
			await _unitOfWork.ForwarderOffers.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}

}
