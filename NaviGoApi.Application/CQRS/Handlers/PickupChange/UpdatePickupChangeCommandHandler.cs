using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.PickupChange;
using NaviGoApi.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.PickupChange
{
	public class UpdatePickupChangeCommandHandler : IRequestHandler<UpdatePickupChangeCommand>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public UpdatePickupChangeCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(UpdatePickupChangeCommand request, CancellationToken cancellationToken)
		{
			var existingEntity = await _unitOfWork.PickupChanges.GetByIdAsync(request.Id);

			if (existingEntity == null)
			{
				throw new KeyNotFoundException($"PickupChange with Id {request.Id} not found.");
			}

			// Map updated fields from DTO to existing entity
			_mapper.Map(request.PickupChangeDto, existingEntity);

			await _unitOfWork.PickupChanges.UpdateAsync(existingEntity);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}
}
