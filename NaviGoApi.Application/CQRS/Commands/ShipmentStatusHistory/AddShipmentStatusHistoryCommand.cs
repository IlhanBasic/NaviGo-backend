using MediatR;
using NaviGoApi.Application.DTOs.ShipmentStatusHistory;

namespace NaviGoApi.Application.CQRS.Commands.ShipmentStatusHistory
{
	public class AddShipmentStatusHistoryCommand : IRequest<Unit>
	{
		public ShipmentStatusHistoryCreateDto ShipmentStatusHistoryDto { get; set; }

		public AddShipmentStatusHistoryCommand(ShipmentStatusHistoryCreateDto dto)
		{
			ShipmentStatusHistoryDto = dto;
		}
	}
}
