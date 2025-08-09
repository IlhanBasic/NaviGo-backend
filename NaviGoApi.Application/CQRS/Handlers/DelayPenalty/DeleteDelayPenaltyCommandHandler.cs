using MediatR;
using NaviGoApi.Application.CQRS.Commands.DelayPenalty;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.DelayPenalty
{
	public class DeleteDelayPenaltyCommandHandler : IRequestHandler<DeleteDelayPenaltyCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;

		public DeleteDelayPenaltyCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(DeleteDelayPenaltyCommand request, CancellationToken cancellationToken)
		{
			await _unitOfWork.DelayPenalties.DeleteAsync(request.Id);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}
}
