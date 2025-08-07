using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Queries.Vehicle;
using NaviGoApi.Application.DTOs.Vehicle;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Vehicle
{
	public class GetAllVehiclesQueryHandler : IRequestHandler<GetAllVehiclesQuery, IEnumerable<VehicleDto>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public GetAllVehiclesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<IEnumerable<VehicleDto>> Handle(GetAllVehiclesQuery request, CancellationToken cancellationToken)
		{
			var vehicles = await _unitOfWork.Vehicles.GetAllAsync();
			return _mapper.Map<IEnumerable<VehicleDto>>(vehicles);
		}
	}
}
