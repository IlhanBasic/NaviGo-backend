using MediatR;
using NaviGoApi.Application.DTOs.DelayPenalty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.DelayPenalty
{
	public class AddDelayPenaltyCommand : IRequest<Unit>
	{
		public DelayPenaltyCreateDto DelayPenaltyDto { get; }
		public AddDelayPenaltyCommand(DelayPenaltyCreateDto dto)
		{
			DelayPenaltyDto = dto;
		}
	}
}
