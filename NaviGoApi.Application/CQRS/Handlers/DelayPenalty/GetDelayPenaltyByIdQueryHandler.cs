using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Queries.DelayPenalty;
using NaviGoApi.Application.DTOs.DelayPenalty;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.DelayPenalty
{
	public class GetDelayPenaltyByIdQueryHandler : IRequestHandler<GetDelayPenaltyByIdQuery, DelayPenaltyDto?>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public GetDelayPenaltyByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<DelayPenaltyDto?> Handle(GetDelayPenaltyByIdQuery request, CancellationToken cancellationToken)
		{
			var entity = await _unitOfWork.DelayPenalties.GetByIdAsync(request.Id);
			if (entity == null)
				return null;
			return _mapper.Map<DelayPenaltyDto>(entity);
		}
	}
}
