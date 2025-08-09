using MediatR;
using NaviGoApi.Application.CQRS.Commands.PickupChange;
using NaviGoApi.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.PickupChange
{
	public class DeletePickupChangeCommandHandler : IRequestHandler<DeletePickupChangeCommand>
	{
		private readonly IUnitOfWork _unitOfWork;

		public DeletePickupChangeCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(DeletePickupChangeCommand request, CancellationToken cancellationToken)
		{
			var existingEntity = await _unitOfWork.PickupChanges.GetByIdAsync(request.Id);

			if (existingEntity == null)
			{
				throw new KeyNotFoundException($"PickupChange with Id {request.Id} not found.");
			}

			await _unitOfWork.PickupChanges.DeleteAsync(request.Id);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}
}
