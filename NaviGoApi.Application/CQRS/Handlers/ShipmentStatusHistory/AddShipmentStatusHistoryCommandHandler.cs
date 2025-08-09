using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.ShipmentStatusHistory;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.ShipmentStatusHistory
{
	public class AddShipmentStatusHistoryCommandHandler : IRequestHandler<AddShipmentStatusHistoryCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public AddShipmentStatusHistoryCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(AddShipmentStatusHistoryCommand request, CancellationToken cancellationToken)
		{
			var entity = _mapper.Map<Domain.Entities.ShipmentStatusHistory>(request.ShipmentStatusHistoryDto);
			await _unitOfWork.ShipmentStatusHistories.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}
}
