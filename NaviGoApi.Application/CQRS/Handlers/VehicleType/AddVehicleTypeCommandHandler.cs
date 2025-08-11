using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.VehicleType;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
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
			var typeName = request.VehicleTypeDto.TypeName.Trim();

			bool exists = await _unitOfWork.VehicleTypes
				.ExistsAsync(vt => vt.TypeName.ToLower() == typeName.ToLower());

			if (exists)
			{
				throw new ValidationException($"Vehicle type with name '{typeName}' already exists.");
			}

			var vehicleType = _mapper.Map<global::NaviGoApi.Domain.Entities.VehicleType>(request.VehicleTypeDto);
			await _unitOfWork.VehicleTypes.AddAsync(vehicleType);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}

	}
}
