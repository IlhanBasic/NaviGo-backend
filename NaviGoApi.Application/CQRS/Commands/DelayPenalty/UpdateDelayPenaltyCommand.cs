using MediatR;
using NaviGoApi.Application.DTOs.DelayPenalty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.DelayPenalty
{
	public class UpdateDelayPenaltyCommand : IRequest<Unit>
	{
		public int Id { get; }
		public DelayPenaltyUpdateDto DelayPenaltyDto { get; }
		public UpdateDelayPenaltyCommand(int id, DelayPenaltyUpdateDto dto)
		{
			Id = id;
			DelayPenaltyDto = dto;
		}
	}
}
