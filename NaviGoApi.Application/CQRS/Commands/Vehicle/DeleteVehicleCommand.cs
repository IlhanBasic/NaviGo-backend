using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.Vehicle
{
	public class DeleteVehicleCommand : IRequest
	{
		public int VehicleId { get; set; }

		public DeleteVehicleCommand(int id)
		{
			VehicleId = id;
		}
	}
}
