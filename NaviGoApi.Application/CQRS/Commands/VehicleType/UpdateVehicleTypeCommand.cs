using MediatR;
using NaviGoApi.Application.DTOs.VehicleType;

namespace NaviGoApi.Application.CQRS.Commands.VehicleType
{
	public record UpdateVehicleTypeCommand(int Id, VehicleTypeUpdateDto VehicleTypeDto) : IRequest<Unit>;

}
