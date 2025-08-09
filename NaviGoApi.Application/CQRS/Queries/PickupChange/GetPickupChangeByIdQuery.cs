using MediatR;
using NaviGoApi.Application.DTOs.PickupChange;

namespace NaviGoApi.Application.CQRS.Queries.PickupChange
{
	public class GetPickupChangeByIdQuery : IRequest<PickupChangeDto?>
	{
		public int Id { get; }

		public GetPickupChangeByIdQuery(int id)
		{
			Id = id;
		}
	}
}
