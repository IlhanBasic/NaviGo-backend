using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.CargoType;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.CargoType
{
	public class AddCargoTypeCommandHandler : IRequestHandler<AddCargoTypeCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public AddCargoTypeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<Unit> Handle(AddCargoTypeCommand request, CancellationToken cancellationToken)
		{
			var cargoType = _mapper.Map<Domain.Entities.CargoType>(request.CargoTypeDto);
			await _unitOfWork.CargoTypes.AddAsync(cargoType);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}
}
