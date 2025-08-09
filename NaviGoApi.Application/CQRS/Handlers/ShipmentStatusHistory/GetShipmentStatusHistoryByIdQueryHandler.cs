using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Queries.ShipmentStatusHistory;
using NaviGoApi.Application.DTOs.ShipmentStatusHistory;
using NaviGoApi.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.ShipmentStatusHistory
{
	public class GetShipmentStatusHistoryByIdQueryHandler : IRequestHandler<GetShipmentStatusHistoryByIdQuery, ShipmentStatusHistoryDto?>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public GetShipmentStatusHistoryByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<ShipmentStatusHistoryDto?> Handle(GetShipmentStatusHistoryByIdQuery request, CancellationToken cancellationToken)
		{
			var entity = await _unitOfWork.ShipmentStatusHistories.GetByIdAsync(request.Id);
			if (entity == null)
				return null;

			return _mapper.Map<ShipmentStatusHistoryDto>(entity);
		}
	}
}
