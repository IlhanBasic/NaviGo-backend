using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Queries.ForwarderOffer;
using NaviGoApi.Application.DTOs.ForwarderOffer;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.ForwarderOffer
{
	public class GetForwarderOfferByIdQueryHandler : IRequestHandler<GetForwarderOfferByIdQuery, ForwarderOfferDto?>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public GetForwarderOfferByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<ForwarderOfferDto?> Handle(GetForwarderOfferByIdQuery request, CancellationToken cancellationToken)
		{
			var entity = await _unitOfWork.ForwarderOffers.GetByIdAsync(request.Id);
			if (entity == null) return null;
			return _mapper.Map<ForwarderOfferDto>(entity);
		}
	}
}
