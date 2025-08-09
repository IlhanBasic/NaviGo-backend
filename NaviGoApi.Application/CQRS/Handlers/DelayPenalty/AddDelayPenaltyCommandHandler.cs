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
	public class AddDelayPenaltyCommandHandler : IRequestHandler<AddDelayPenaltyCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public AddDelayPenaltyCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(AddDelayPenaltyCommand request, CancellationToken cancellationToken)
		{
			var entity = _mapper.Map<Domain.Entities.DelayPenalty>(request.DelayPenaltyDto);
			await _unitOfWork.DelayPenalties.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}

}
