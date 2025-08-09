using MediatR;
using NaviGoApi.Application.DTOs.ShipmentStatusHistory;

namespace NaviGoApi.Application.CQRS.Commands.ShipmentStatusHistory
{
	public class UpdateShipmentStatusHistoryCommand : IRequest<Unit>
	{
		public int Id { get; set; }
		public ShipmentStatusHistoryUpdateDto ShipmentStatusHistoryDto { get; set; }

		public UpdateShipmentStatusHistoryCommand(int id, ShipmentStatusHistoryUpdateDto dto)
		{
			Id = id;
			ShipmentStatusHistoryDto = dto;
		}
	}
}
