using MediatR;
using NaviGoApi.Application.DTOs.ShipmentStatusHistory;

namespace NaviGoApi.Application.CQRS.Queries.ShipmentStatusHistory
{
	public class GetShipmentStatusHistoryByIdQuery : IRequest<ShipmentStatusHistoryDto?>
	{
		public int Id { get; set; }

		public GetShipmentStatusHistoryByIdQuery(int id)
		{
			Id = id;
		}
	}
}
