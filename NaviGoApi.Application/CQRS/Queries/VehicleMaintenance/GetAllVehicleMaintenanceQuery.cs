using MediatR;
using NaviGoApi.Application.DTOs.VehicleMaintenance;
using NaviGoApi.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Queries.VehicleMaintenance
{
	public class GetAllVehicleMaintenanceQuery : IRequest<IEnumerable<Domain.Entities.VehicleMaintenance>>
	{
        public VehicleMaintenanceSearchDto Search { get; set; }
        public GetAllVehicleMaintenanceQuery(VehicleMaintenanceSearchDto search)
        {
            Search= search;
        }
    }
}
