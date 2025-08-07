using MediatR;
using NaviGoApi.Application.DTOs.VehicleMaintenance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Queries.VehicleMaintenance
{
	public class GetAllVehicleMaintenanceQuery : IRequest<IEnumerable<Domain.Entities.VehicleMaintenance>>
	{
	}
}
