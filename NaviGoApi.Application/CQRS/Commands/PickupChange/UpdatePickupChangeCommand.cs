using MediatR;
using NaviGoApi.Application.DTOs.PickupChange;

namespace NaviGoApi.Application.CQRS.Commands.PickupChange
{
	public class UpdatePickupChangeCommand : IRequest
	{
		public int Id { get; }
		public PickupChangeUpdateDto PickupChangeDto { get; }

		public UpdatePickupChangeCommand(int id, PickupChangeUpdateDto dto)
		{
			Id = id;
			PickupChangeDto = dto;
		}
	}
}
