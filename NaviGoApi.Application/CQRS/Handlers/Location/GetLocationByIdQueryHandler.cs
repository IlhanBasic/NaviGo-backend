using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Queries.Location;
using NaviGoApi.Application.DTOs.Location;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Location
{
	public class GetLocationByIdQueryHandler : IRequestHandler<GetLocationByIdQuery, LocationDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
        public GetLocationByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}
		public async Task<LocationDto> Handle(GetLocationByIdQuery request, CancellationToken cancellationToken)
		{
			var location = await _unitOfWork.Locations.GetByIdAsync(request.Id);
			return _mapper.Map<LocationDto>(location);
		}
	}
}
