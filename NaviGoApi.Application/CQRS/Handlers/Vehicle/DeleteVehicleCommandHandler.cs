using MediatR;
using NaviGoApi.Application.CQRS.Commands.Vehicle;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Vehicle
{
	public class DeleteVehicleCommandHandler : IRequestHandler<DeleteVehicleCommand>
	{
		private readonly IUnitOfWork _unitOfWork;

		public DeleteVehicleCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(DeleteVehicleCommand request, CancellationToken cancellationToken)
		{
			var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.VehicleId);
			if (vehicle == null)
			{
				throw new KeyNotFoundException($"Vehicle with Id {request.VehicleId} not found.");
			}

			await _unitOfWork.Vehicles.DeleteAsync(vehicle);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}
}
