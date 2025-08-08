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
	public class GetAllRoutePriceQueryHandler : IRequestHandler<GetAllRoutePriceQuery, IEnumerable<RoutePriceDto?>>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public GetAllRoutePriceQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<IEnumerable<RoutePriceDto?>> Handle(GetAllRoutePriceQuery request, CancellationToken cancellationToken)
		{
			var entities = await _unitOfWork.RoutePrices.GetAllAsync();
			var dtos = _mapper.Map<IEnumerable<RoutePriceDto>>(entities);
			return dtos;
		}
	}

}
