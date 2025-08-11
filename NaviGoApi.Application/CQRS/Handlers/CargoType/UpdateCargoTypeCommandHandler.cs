using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.CargoType;
using NaviGoApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.CargoType
{
	public class UpdateCargoTypeCommandHandler : IRequestHandler<UpdateCargoTypeCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public UpdateCargoTypeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<Unit> Handle(UpdateCargoTypeCommand request, CancellationToken cancellationToken)
		{
			var typeName = request.CargoTypeDto.TypeName.Trim();

			bool exists = await _unitOfWork.CargoTypes
				.ExistsAsync(vt => vt.TypeName.ToLower() == typeName.ToLower());

			if (exists)
			{
				throw new ValidationException($"Cargo type with name '{typeName}' already exists.");
			}
			var existing = await _unitOfWork.CargoTypes.GetByIdAsync(request.Id);
			if (existing != null)
			{
				_mapper.Map(request.CargoTypeDto, existing);
				await _unitOfWork.CargoTypes.UpdateAsync(existing);
				await _unitOfWork.SaveChangesAsync();
			}

			return Unit.Value;
		}
	}
}
