using MediatR;
using NaviGoApi.Application.CQRS.Commands.CargoType;
using NaviGoApi.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.CargoType
{
	public class DeleteCargoTypeCommandHandler : IRequestHandler<DeleteCargoTypeCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;

		public DeleteCargoTypeCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(DeleteCargoTypeCommand request, CancellationToken cancellationToken)
		{
			var cargoType = await _unitOfWork.CargoTypes.GetByIdAsync(request.Id);
			if (cargoType != null)
			{
				await _unitOfWork.CargoTypes.DeleteAsync(cargoType);
				await _unitOfWork.SaveChangesAsync();
			}
			return Unit.Value;
		}
	}
}
