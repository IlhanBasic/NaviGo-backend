using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Queries.Route;
using NaviGoApi.Application.DTOs.Route;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Route
{
	public class GetAllRouteQueryHandler : IRequestHandler<GetAllRouteQuery, IEnumerable<RouteDto?>>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		public GetAllRouteQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<IEnumerable<RouteDto?>> Handle(GetAllRouteQuery request, CancellationToken cancellationToken)
		{
			var routes = await _unitOfWork.Routes.GetAllAsync();
			return _mapper.Map<IEnumerable<RouteDto?>>(routes);
		}
	}
}
