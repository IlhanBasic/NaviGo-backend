using MediatR;

namespace NaviGoApi.Application.CQRS.Commands.CargoType
{
	public class DeleteCargoTypeCommand : IRequest<Unit>
	{
		public int Id { get; set; }

		public DeleteCargoTypeCommand(int id)
		{
			Id = id;
		}
	}
}
