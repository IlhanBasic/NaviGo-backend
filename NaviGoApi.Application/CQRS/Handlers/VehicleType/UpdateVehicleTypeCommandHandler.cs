using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.VehicleType;
using NaviGoApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.VehicleType
{
	public class UpdateVehicleTypeCommandHandler : IRequestHandler<UpdateVehicleTypeCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public UpdateVehicleTypeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}
		public async Task<Unit> Handle(UpdateVehicleTypeCommand request, CancellationToken cancellationToken)
		{
			var existing = await _unitOfWork.VehicleTypes.GetByIdAsync(request.Id);
			if (existing == null)
				throw new KeyNotFoundException("Vehicle type not found.");

			var newTypeName = request.VehicleTypeDto.TypeName.Trim();

			bool existsWithSameName = await _unitOfWork.VehicleTypes
				.ExistsAsync(vt => vt.TypeName.ToLower() == newTypeName.ToLower() && vt.Id != request.Id);

			if (existsWithSameName)
			{
				throw new ValidationException($"Another vehicle type with name '{newTypeName}' already exists.");
			}

			_mapper.Map(request.VehicleTypeDto, existing);
			await _unitOfWork.VehicleTypes.UpdateAsync(existing);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}


	}
}
