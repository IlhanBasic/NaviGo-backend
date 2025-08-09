using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Queries.PickupChange;
using NaviGoApi.Application.DTOs.PickupChange;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.PickupChange
{
	public class GetAllPickupChangesQueryHandler : IRequestHandler<GetAllPickupChangesQuery, IEnumerable<PickupChangeDto>>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public GetAllPickupChangesQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<IEnumerable<PickupChangeDto>> Handle(GetAllPickupChangesQuery request, CancellationToken cancellationToken)
		{
			var entities = await _unitOfWork.PickupChanges.GetAllAsync();
			return _mapper.Map<IEnumerable<PickupChangeDto>>(entities);
		}
	}
}
