using MediatR;
using NaviGoApi.Application.DTOs.VehicleMaintenance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.VehicleMaintenance
{
	public class AddVehicleMaintenanceCommand: IRequest<Unit>
	{
		public VehicleMaintenanceCreateDto Dto { get; set; }

		public AddVehicleMaintenanceCommand(VehicleMaintenanceCreateDto dto)
		{
			Dto = dto;
		}
	}
}
