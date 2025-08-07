using MediatR;
using NaviGoApi.Application.DTOs.Vehicle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.Vehicle
{
	public class UpdateVehicleCommand : IRequest<VehicleDto>
	{
		public VehicleUpdateDto VehicleUpdateDto { get; set; }

		public UpdateVehicleCommand(VehicleUpdateDto dto)
		{
			VehicleUpdateDto = dto;
		}
	}
}
