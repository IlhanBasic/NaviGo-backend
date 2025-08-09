using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Queries.PickupChange;
using NaviGoApi.Application.DTOs.PickupChange;
using NaviGoApi.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.PickupChange
{
	public class GetPickupChangeByIdQueryHandler : IRequestHandler<GetPickupChangeByIdQuery, PickupChangeDto?>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public GetPickupChangeByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<PickupChangeDto?> Handle(GetPickupChangeByIdQuery request, CancellationToken cancellationToken)
		{
			var entity = await _unitOfWork.PickupChanges.GetByIdAsync(request.Id);
			if (entity == null)
				return null;

			return _mapper.Map<PickupChangeDto>(entity);
		}
	}
}
