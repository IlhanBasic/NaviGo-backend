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
	public class UpdateVehicleMaintenanceCommandHandler : IRequestHandler<UpdateVehicleMaintenanceCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

        public UpdateVehicleMaintenanceCommandHandler(IMapper mapper,IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
			_unitOfWork = unitOfWork;
        }
        public async Task<Unit> Handle(UpdateVehicleMaintenanceCommand request, CancellationToken cancellationToken)
		{
			var vehiclemaintenance = await _unitOfWork.VehicleMaintenances.GetByIdAsync(request.Id);
			if (vehiclemaintenance != null)
			{
				await _unitOfWork.VehicleMaintenances.UpdateAsync(vehiclemaintenance);
				await _unitOfWork.SaveChangesAsync();
			}

			return Unit.Value;
		}
	}
}
