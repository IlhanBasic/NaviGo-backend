using MediatR;

namespace NaviGoApi.Application.CQRS.Commands.VehicleType
{
	public class DeleteVehicleTypeCommand : IRequest<Unit>
	{
		public int Id { get; set; }

		public DeleteVehicleTypeCommand(int id)
		{
			Id = id;
		}
	}
}
