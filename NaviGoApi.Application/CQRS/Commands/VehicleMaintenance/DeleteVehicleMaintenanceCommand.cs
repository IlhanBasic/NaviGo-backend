using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.VehicleMaintenance
{
	public class DeleteVehicleMaintenanceCommand:IRequest<Unit>
	{
        public int Id { get; set; }
        public DeleteVehicleMaintenanceCommand(int id)
        {
            Id = id;
        }
    }
}
