using MediatR;
using NaviGoApi.Application.CQRS.Commands.VehicleType;
using NaviGoApi.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.VehicleType
{
	public class DeleteVehicleTypeCommandHandler : IRequestHandler<DeleteVehicleTypeCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;

		public DeleteVehicleTypeCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(DeleteVehicleTypeCommand request, CancellationToken cancellationToken)
		{
			var vehicleType = await _unitOfWork.VehicleTypes.GetByIdAsync(request.Id);
			if (vehicleType != null)
			{
				await _unitOfWork.VehicleTypes.DeleteAsync(request.Id);
				await _unitOfWork.SaveChangesAsync();
			}
			return Unit.Value;
		}
	}
}
