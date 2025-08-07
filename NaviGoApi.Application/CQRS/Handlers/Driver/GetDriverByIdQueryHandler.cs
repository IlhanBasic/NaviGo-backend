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
	public class GetDriverByIdQueryHandler : IRequestHandler<GetDriverByIdQuery, DriverDto?>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public GetDriverByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<DriverDto?> Handle(GetDriverByIdQuery request, CancellationToken cancellationToken)
		{
			var driver = await _unitOfWork.Drivers.GetByIdAsync(request.Id);
			return driver == null ? null : _mapper.Map<DriverDto>(driver);
		}
	}
}
