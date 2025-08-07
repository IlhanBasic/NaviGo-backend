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
	public class GetVehicleByIdQueryHandler : IRequestHandler<GetVehicleByIdQuery, VehicleDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public GetVehicleByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<VehicleDto> Handle(GetVehicleByIdQuery request, CancellationToken cancellationToken)
		{
			var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.VehicleId);
			if (vehicle == null)
			{
				throw new KeyNotFoundException($"Vehicle with Id {request.VehicleId} not found.");
			}
			return _mapper.Map<VehicleDto>(vehicle);
		}
	}
}
