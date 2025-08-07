using MediatR;
using NaviGoApi.Application.DTOs.VehicleMaintenance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.VehicleMaintenance
{
	public class UpdateVehicleMaintenanceCommand : IRequest<Unit>
	{
		public VehicleMaintenanceUpdateDto Dto { get; set; }
		public int Id;

		public UpdateVehicleMaintenanceCommand(int id,VehicleMaintenanceUpdateDto dto)
		{
			Id = id;
			Dto = dto;
		}
	}
}
