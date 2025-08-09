using MediatR;

namespace NaviGoApi.Application.CQRS.Commands.ShipmentStatusHistory
{
	public class DeleteShipmentStatusHistoryCommand : IRequest<Unit>
	{
		public int Id { get; set; }

		public DeleteShipmentStatusHistoryCommand(int id)
		{
			Id = id;
		}
	}
}
