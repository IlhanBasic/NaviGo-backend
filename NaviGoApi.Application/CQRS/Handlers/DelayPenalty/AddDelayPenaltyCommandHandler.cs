using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.DelayPenalty;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.DelayPenalty
{
	public class AddDelayPenaltyCommandHandler : IRequestHandler<AddDelayPenaltyCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public AddDelayPenaltyCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(AddDelayPenaltyCommand request, CancellationToken cancellationToken)
		{
			var shipment = await _unitOfWork.Shipments.GetByIdAsync(request.DelayPenaltyDto.ShipmentId);
			if (shipment == null)
				throw new ValidationException($"Shipment with ID {request.DelayPenaltyDto.ShipmentId} does not exist.");
			var existingPenalty = await _unitOfWork.DelayPenalties.GetByShipmentIdAsync(request.DelayPenaltyDto.ShipmentId);
			if (existingPenalty != null)
				throw new ValidationException($"Delay penalty for shipment ID {request.DelayPenaltyDto.ShipmentId} already exists.");
			if (request.DelayPenaltyDto.DelayHours < 0)
				throw new ValidationException("Delay hours cannot be negative.");
			if (request.DelayPenaltyDto.PenaltyAmount < 0)
				throw new ValidationException("Penalty amount cannot be negative.");
			
			if (shipment.Status != ShipmentStatus.Delayed)
			{
				shipment.Status = ShipmentStatus.Delayed;
				await _unitOfWork.Shipments.UpdateAsync(shipment);
			}
			var entity = _mapper.Map<Domain.Entities.DelayPenalty>(request.DelayPenaltyDto);
			entity.CalculatedAt = DateTime.UtcNow;
			await _unitOfWork.DelayPenalties.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}

	}

}
