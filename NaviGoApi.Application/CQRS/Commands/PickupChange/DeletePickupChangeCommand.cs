using MediatR;

namespace NaviGoApi.Application.CQRS.Commands.PickupChange
{
	public class DeletePickupChangeCommand : IRequest
	{
		public int Id { get; }

		public DeletePickupChangeCommand(int id)
		{
			Id = id;
		}
	}
}
