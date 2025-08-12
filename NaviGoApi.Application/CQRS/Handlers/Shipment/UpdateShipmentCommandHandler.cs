using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.Shipment;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Shipment
{
	public class UpdateShipmentCommandHandler : IRequestHandler<UpdateShipmentCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public UpdateShipmentCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(UpdateShipmentCommand request, CancellationToken cancellationToken)
		{
			var existing = await _unitOfWork.Shipments.GetByIdAsync(request.Id);
			if (existing == null)
				throw new KeyNotFoundException($"Shipment with ID {request.Id} not found.");
			if (request.ShipmentDto.ActualDeparture.HasValue && request.ShipmentDto.ActualArrival.HasValue)
			{
				if (request.ShipmentDto.ActualDeparture > request.ShipmentDto.ActualArrival)
					throw new ArgumentException("ActualDeparture cannot be later than ActualArrival.");
			}
			_mapper.Map(request.ShipmentDto, existing);

			await _unitOfWork.Shipments.UpdateAsync(existing);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}
}
