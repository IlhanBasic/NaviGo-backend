using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.ShipmentStatusHistory;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.ShipmentStatusHistory
{
	public class UpdateShipmentStatusHistoryCommandHandler : IRequestHandler<UpdateShipmentStatusHistoryCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public UpdateShipmentStatusHistoryCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(UpdateShipmentStatusHistoryCommand request, CancellationToken cancellationToken)
		{
			var existing = await _unitOfWork.ShipmentStatusHistories.GetByIdAsync(request.Id);
			if (existing == null)
				throw new KeyNotFoundException($"ShipmentStatusHistory with Id {request.Id} not found.");

			_mapper.Map(request.ShipmentStatusHistoryDto, existing);
			await _unitOfWork.ShipmentStatusHistories.UpdateAsync(existing);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}
}
