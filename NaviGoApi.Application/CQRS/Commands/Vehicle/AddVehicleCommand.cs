using MediatR;
using NaviGoApi.Application.DTOs.Vehicle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.Vehicle
{
	public class AddVehicleCommand : IRequest<VehicleDto>
	{
		public VehicleCreateDto VehicleCreateDto { get; set; }

		public AddVehicleCommand(VehicleCreateDto dto)
		{
			VehicleCreateDto = dto;
		}
	}
}
