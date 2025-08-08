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
	public class GetAllForwarderOfferQueryHandler : IRequestHandler<GetAllForwarderOfferQuery, IEnumerable<ForwarderOfferDto?>>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public GetAllForwarderOfferQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<IEnumerable<ForwarderOfferDto?>> Handle(GetAllForwarderOfferQuery request, CancellationToken cancellationToken)
		{
			var entities = await _unitOfWork.ForwarderOffers.GetAllAsync();
			return _mapper.Map<IEnumerable<ForwarderOfferDto>>(entities);
		}
	}
}
