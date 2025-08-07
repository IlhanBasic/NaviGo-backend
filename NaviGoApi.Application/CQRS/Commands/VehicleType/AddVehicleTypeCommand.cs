using MediatR;
using NaviGoApi.Application.DTOs.VehicleType;

namespace NaviGoApi.Application.CQRS.Commands.VehicleType
{
	public class AddVehicleTypeCommand : IRequest<Unit>
	{
		public VehicleTypeCreateDto VehicleTypeDto { get; set; }

		public AddVehicleTypeCommand(VehicleTypeCreateDto dto)
		{
			VehicleTypeDto = dto;
		}
	}
}
