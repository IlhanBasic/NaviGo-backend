using MediatR;
using NaviGoApi.Application.DTOs.PickupChange;
using System.Collections.Generic;

namespace NaviGoApi.Application.CQRS.Queries.PickupChange
{
	public class GetAllPickupChangesQuery : IRequest<IEnumerable<PickupChangeDto>>
	{
	}
}
