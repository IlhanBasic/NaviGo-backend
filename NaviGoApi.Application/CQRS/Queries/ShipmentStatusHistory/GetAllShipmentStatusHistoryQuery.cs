using MediatR;
using NaviGoApi.Application.DTOs.ShipmentStatusHistory;
using System.Collections.Generic;

namespace NaviGoApi.Application.CQRS.Queries.ShipmentStatusHistory
{
	public class GetAllShipmentStatusHistoryQuery : IRequest<IEnumerable<ShipmentStatusHistoryDto>>
	{
	}
}
