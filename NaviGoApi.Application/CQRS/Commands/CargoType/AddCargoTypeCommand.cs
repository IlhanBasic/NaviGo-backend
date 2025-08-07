using MediatR;
using NaviGoApi.Application.DTOs.CargoType;

namespace NaviGoApi.Application.CQRS.Commands.CargoType
{
	public class AddCargoTypeCommand : IRequest<Unit>
	{
		public CargoTypeCreateDto CargoTypeDto { get; set; }

		public AddCargoTypeCommand(CargoTypeCreateDto dto)
		{
			CargoTypeDto = dto;
		}
	}
}
