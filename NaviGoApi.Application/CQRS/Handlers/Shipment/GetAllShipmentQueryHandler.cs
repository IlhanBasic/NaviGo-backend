using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Queries.Shipment;
using NaviGoApi.Application.DTOs.Shipment;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Shipment
{
	public class GetAllShipmentQueryHandler : IRequestHandler<GetAllShipmentQuery, IEnumerable<ShipmentDto?>>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public GetAllShipmentQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<IEnumerable<ShipmentDto?>> Handle(GetAllShipmentQuery request, CancellationToken cancellationToken)
		{
			var shipments = await _unitOfWork.Shipments.GetAllAsync();
			return _mapper.Map<IEnumerable<ShipmentDto?>>(shipments);
		}
	}
}
