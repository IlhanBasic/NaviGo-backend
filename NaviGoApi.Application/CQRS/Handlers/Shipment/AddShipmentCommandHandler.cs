using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.Shipment;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Shipment
{
	public class AddShipmentCommandHandler : IRequestHandler<AddShipmentCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public AddShipmentCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(AddShipmentCommand request, CancellationToken cancellationToken)
		{
			var shipment = _mapper.Map<Domain.Entities.Shipment>(request.ShipmentDto);
			await _unitOfWork.Shipments.AddAsync(shipment);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}
}
