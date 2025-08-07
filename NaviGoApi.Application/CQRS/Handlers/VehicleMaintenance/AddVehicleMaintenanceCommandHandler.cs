using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.VehicleMaintenance;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.VehicleMaintenance
{
	public class AddVehicleMaintenanceCommandHandler : IRequestHandler<AddVehicleMaintenanceCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
        public AddVehicleMaintenanceCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
			_unitOfWork = unitOfWork;
        }
        public async Task<Unit> Handle(AddVehicleMaintenanceCommand request, CancellationToken cancellationToken)
		{
			var vehiclemaintenance = _mapper.Map<Domain.Entities.VehicleMaintenance>(request.Dto);
			if (vehiclemaintenance != null)
			{
				await _unitOfWork.VehicleMaintenances.AddAsync(vehiclemaintenance);
				await _unitOfWork.SaveChangesAsync();
			}
			return Unit.Value;
		}
	}
}
