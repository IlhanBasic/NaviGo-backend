using AutoMapper;
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
	public class UpdateDelayPenaltyCommandHandler : IRequestHandler<UpdateDelayPenaltyCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public UpdateDelayPenaltyCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(UpdateDelayPenaltyCommand request, CancellationToken cancellationToken)
		{
			var existing = await _unitOfWork.DelayPenalties.GetByIdAsync(request.Id);
			if (existing == null)
			{
				throw new KeyNotFoundException("DelayPenalty not found");
			}

			_mapper.Map(request.DelayPenaltyDto, existing);
			await _unitOfWork.DelayPenalties.UpdateAsync(existing);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}
}
