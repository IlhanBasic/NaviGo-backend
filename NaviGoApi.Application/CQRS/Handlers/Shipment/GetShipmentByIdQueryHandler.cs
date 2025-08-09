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
	public class GetShipmentByIdQueryHandler : IRequestHandler<GetShipmentByIdQuery, ShipmentDto?>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public GetShipmentByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<ShipmentDto?> Handle(GetShipmentByIdQuery request, CancellationToken cancellationToken)
		{
			var shipment = await _unitOfWork.Shipments.GetByIdAsync(request.Id);
			if (shipment == null)
				return null;

			return _mapper.Map<ShipmentDto>(shipment);
		}
	}
}
