using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.DelayPenalty
{
	public class DeleteDelayPenaltyCommand : IRequest<Unit>
	{
		public int Id { get; }
		public DeleteDelayPenaltyCommand(int id)
		{
			Id = id;
		}
	}
}
