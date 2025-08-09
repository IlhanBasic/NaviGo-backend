using MediatR;
using NaviGoApi.Application.DTOs.DelayPenalty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Queries.DelayPenalty
{
	public class GetAllDelayPenaltiesQuery : IRequest<IEnumerable<DelayPenaltyDto>>
	{
	}
}
