using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Queries.RoutePrice;
using NaviGoApi.Application.DTOs.RoutePrice;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.RoutePrice
{
	public class GetRoutePriceByIdQueryHandler : IRequestHandler<GetRoutePriceByIdQuery, RoutePriceDto?>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public GetRoutePriceByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<RoutePriceDto?> Handle(GetRoutePriceByIdQuery request, CancellationToken cancellationToken)
		{
			var entity = await _unitOfWork.RoutePrices.GetByIdAsync(request.Id);
			if (entity == null) return null;
			return _mapper.Map<RoutePriceDto>(entity);
		}
	}

}
