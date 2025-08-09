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

			_mapper.Map(request.ShipmentDto, existing); // Mapuje izmene na postojeći entitet
			_unitOfWork.Shipments.Update(existing);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}
}
