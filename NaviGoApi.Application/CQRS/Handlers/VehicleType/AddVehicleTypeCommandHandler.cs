using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.VehicleType;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.VehicleType
{
	public class AddVehicleTypeCommandHandler : IRequestHandler<AddVehicleTypeCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public AddVehicleTypeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<Unit> Handle(AddVehicleTypeCommand request, CancellationToken cancellationToken)
		{
			var vehicleType = _mapper.Map<global::NaviGoApi.Domain.Entities.VehicleType>(request.VehicleTypeDto);
			await _unitOfWork.VehicleTypes.AddAsync(vehicleType);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}
}
