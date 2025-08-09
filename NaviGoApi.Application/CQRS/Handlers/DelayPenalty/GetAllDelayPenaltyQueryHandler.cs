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
	public class GetAllDelayPenaltiesQueryHandler : IRequestHandler<GetAllDelayPenaltiesQuery, IEnumerable<DelayPenaltyDto>>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public GetAllDelayPenaltiesQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<IEnumerable<DelayPenaltyDto>> Handle(GetAllDelayPenaltiesQuery request, CancellationToken cancellationToken)
		{
			var entities = await _unitOfWork.DelayPenalties.GetAllAsync();
			return _mapper.Map<IEnumerable<DelayPenaltyDto>>(entities);
		}
	}
}
