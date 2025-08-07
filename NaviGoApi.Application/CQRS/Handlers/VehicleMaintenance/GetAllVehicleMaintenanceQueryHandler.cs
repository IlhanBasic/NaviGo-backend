using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Queries.VehicleMaintenance;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.VehicleMaintenance
{
	public class GetAllVehicleMaintenanceQueryHandler : IRequestHandler<GetAllVehicleMaintenanceQuery, IEnumerable<Domain.Entities.VehicleMaintenance>>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
        public GetAllVehicleMaintenanceQueryHandler(IMapper mapper,IUnitOfWork unitOfWork)
        {
            _mapper= mapper;
			_unitOfWork= unitOfWork;
        }
        public async Task<IEnumerable<Domain.Entities.VehicleMaintenance>> Handle(GetAllVehicleMaintenanceQuery request, CancellationToken cancellationToken)
		{
			var vehiclemaintenances = await _unitOfWork.VehicleMaintenances.GetAllAsync();
			return _mapper.Map<IEnumerable<Domain.Entities.VehicleMaintenance>>(vehiclemaintenances);
		}
	}
}
