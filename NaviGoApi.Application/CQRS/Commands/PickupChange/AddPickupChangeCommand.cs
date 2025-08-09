using MediatR;
using NaviGoApi.Application.DTOs.PickupChange;

namespace NaviGoApi.Application.CQRS.Commands.PickupChange
{
	public class AddPickupChangeCommand : IRequest<int>
	{
		public PickupChangeCreateDto PickupChangeDto { get; }

		public AddPickupChangeCommand(PickupChangeCreateDto dto)
		{
			PickupChangeDto = dto;
		}
	}
}
