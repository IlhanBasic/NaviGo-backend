using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.PickupChange;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.PickupChange
{
	public class AddPickupChangeCommandHandler : IRequestHandler<AddPickupChangeCommand, int>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public AddPickupChangeCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<int> Handle(AddPickupChangeCommand request, CancellationToken cancellationToken)
		{
			var entity = _mapper.Map<Domain.Entities.PickupChange>(request.PickupChangeDto);

			await _unitOfWork.PickupChanges.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return entity.Id;
		}
	}
}
