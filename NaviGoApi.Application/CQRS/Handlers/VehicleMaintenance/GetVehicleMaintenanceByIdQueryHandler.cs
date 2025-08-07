using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Queries.VehicleMaintenance;
using NaviGoApi.Application.DTOs.VehicleMaintenance;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.VehicleMaintenance
{
	public class GetVehicleMaintenanceByIdQueryHandler : IRequestHandler<GetVehicleMaintenanceByIdQuery, VehicleMaintenanceDto?>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
        public GetVehicleMaintenanceByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}
		public async Task<VehicleMaintenanceDto?> Handle(GetVehicleMaintenanceByIdQuery request, CancellationToken cancellationToken)
		{
			var vehiclemaintenance = await _unitOfWork.VehicleMaintenances.GetByIdAsync(request.Id);
			return _mapper.Map<VehicleMaintenanceDto>(vehiclemaintenance);
		}
	}
}
