using MediatR;
using NaviGoApi.Application.DTOs.Vehicle;
using NaviGoApi.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Queries.Vehicle
{
	public class GetAllVehiclesQuery : IRequest<IEnumerable<VehicleDto>>
	{
		public VehicleSearchDto Search { get; set; }
		public GetAllVehiclesQuery(VehicleSearchDto search) 
		{ 
			Search = search;
		}
	}
}
