using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Queries.Driver;
using NaviGoApi.Application.DTOs.Driver;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Driver
{
	public class GetAllDriverQueryHandler : IRequestHandler<GetAllDriverQuery, IEnumerable<DriverDto?>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public GetAllDriverQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<IEnumerable<DriverDto?>> Handle(GetAllDriverQuery request, CancellationToken cancellationToken)
		{
			var drivers = await _unitOfWork.Drivers.GetAllAsync();
			return _mapper.Map<IEnumerable<DriverDto?>>(drivers);
		}
	}
}
