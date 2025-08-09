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
	public class DeleteShipmentCommandHandler : IRequestHandler<DeleteShipmentCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;

		public DeleteShipmentCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(DeleteShipmentCommand request, CancellationToken cancellationToken)
		{
			var shipment = await _unitOfWork.Shipments.GetByIdAsync(request.Id);
			if (shipment == null)
				throw new KeyNotFoundException($"Shipment with ID {request.Id} not found.");

			_unitOfWork.Shipments.Delete(shipment);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}
}
