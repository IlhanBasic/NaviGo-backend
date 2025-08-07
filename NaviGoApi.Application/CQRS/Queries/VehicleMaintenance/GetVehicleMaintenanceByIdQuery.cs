using MediatR;
using NaviGoApi.Application.DTOs.VehicleMaintenance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Queries.VehicleMaintenance
{
	public class GetVehicleMaintenanceByIdQuery: IRequest<VehicleMaintenanceDto?>	
	{
		public int Id { get; set; }

		public GetVehicleMaintenanceByIdQuery(int id)
		{
			Id = id;
		}
	}
}
