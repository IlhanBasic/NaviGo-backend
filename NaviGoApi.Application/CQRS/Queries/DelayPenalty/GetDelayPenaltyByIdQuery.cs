using MediatR;
using NaviGoApi.Application.DTOs.DelayPenalty;

namespace NaviGoApi.Application.CQRS.Queries.DelayPenalty
{
	public class GetDelayPenaltyByIdQuery : IRequest<DelayPenaltyDto?>
	{
		public int Id { get; }
		public GetDelayPenaltyByIdQuery(int id)
		{
			Id = id;
		}
	}
}
