using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Queries.ShipmentStatusHistory;
using NaviGoApi.Application.DTOs.ShipmentStatusHistory;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.ShipmentStatusHistory
{
	public class GetAllShipmentStatusHistoryQueryHandler : IRequestHandler<GetAllShipmentStatusHistoryQuery, IEnumerable<ShipmentStatusHistoryDto>>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public GetAllShipmentStatusHistoryQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<IEnumerable<ShipmentStatusHistoryDto>> Handle(GetAllShipmentStatusHistoryQuery request, CancellationToken cancellationToken)
		{
			var entities = await _unitOfWork.ShipmentStatusHistories.GetAllAsync();
			return _mapper.Map<IEnumerable<ShipmentStatusHistoryDto>>(entities);
		}
	}
}
