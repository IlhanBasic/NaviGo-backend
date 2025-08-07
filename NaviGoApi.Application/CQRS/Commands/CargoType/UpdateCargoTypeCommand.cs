using MediatR;
using NaviGoApi.Application.DTOs.CargoType;

namespace NaviGoApi.Application.CQRS.Commands.CargoType
{
	public record UpdateCargoTypeCommand(int Id, CargoTypeUpdateDto CargoTypeDto) : IRequest<Unit>;
}
