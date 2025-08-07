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
	public class GetAllLocationQueryHandler : IRequestHandler<GetAllLocationQuery, IEnumerable<LocationDto>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
        public GetAllLocationQueryHandler(IUnitOfWork unitOfWork,IMapper mapper)
        {
            _unitOfWork = unitOfWork;
			_mapper = mapper;
        }
        public async Task<IEnumerable<LocationDto>> Handle(GetAllLocationQuery request, CancellationToken cancellationToken)
		{
			var locations = await _unitOfWork.Locations.GetAllAsync();
			return _mapper.Map<IEnumerable<LocationDto>>(locations);
		}
	}
}
