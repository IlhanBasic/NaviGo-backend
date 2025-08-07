using MediatR;
using NaviGoApi.Application.DTOs.Vehicle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Queries.Vehicle
{
	public class GetVehicleByIdQuery : IRequest<VehicleDto>
	{
		public int VehicleId { get; set; }

		public GetVehicleByIdQuery(int id)
		{
			VehicleId = id;
		}
	}
}
