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
	public class GetRouteByIdQueryHandler : IRequestHandler<GetRouteByIdQuery, RouteDto?>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		public GetRouteByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<RouteDto?> Handle(GetRouteByIdQuery request, CancellationToken cancellationToken)
		{
			var route = await _unitOfWork.Routes.GetByIdAsync(request.Id);
			if (route == null)
				return null;

			return _mapper.Map<RouteDto>(route);
		}
	}
}
