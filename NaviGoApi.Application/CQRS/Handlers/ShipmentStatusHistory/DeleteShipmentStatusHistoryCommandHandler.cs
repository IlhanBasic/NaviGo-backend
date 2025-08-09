using MediatR;
using NaviGoApi.Application.CQRS.Commands.ShipmentStatusHistory;
using NaviGoApi.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.ShipmentStatusHistory
{
	public class DeleteShipmentStatusHistoryCommandHandler : IRequestHandler<DeleteShipmentStatusHistoryCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;

		public DeleteShipmentStatusHistoryCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(DeleteShipmentStatusHistoryCommand request, CancellationToken cancellationToken)
		{
			await _unitOfWork.ShipmentStatusHistories.DeleteAsync(request.Id);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}
}
