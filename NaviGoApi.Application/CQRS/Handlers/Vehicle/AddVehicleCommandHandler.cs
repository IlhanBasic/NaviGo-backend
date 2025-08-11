using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.Vehicle;
using NaviGoApi.Application.DTOs.Vehicle;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Vehicle
{
	public class AddVehicleCommandHandler : IRequestHandler<AddVehicleCommand, VehicleDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public AddVehicleCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		//public async Task<VehicleDto> Handle(AddVehicleCommand request, CancellationToken cancellationToken)
		//{
		//	var vehicleEntity = _mapper.Map<Domain.Entities.Vehicle>(request.VehicleCreateDto);
		//	vehicleEntity.VehicleStatus = Domain.Entities.VehicleStatus.Free;
		//	await _unitOfWork.Vehicles.AddAsync(vehicleEntity);
		//	await _unitOfWork.SaveChangesAsync();

		//	return _mapper.Map<VehicleDto>(vehicleEntity);
		//}
		public async Task<VehicleDto> Handle(AddVehicleCommand request, CancellationToken cancellationToken)
		{
			var dto = request.VehicleCreateDto;

			// Provera da li kompanija postoji
			var company = await _unitOfWork.Companies.GetByIdAsync(dto.CompanyId);
			if (company == null)
			{
				throw new ValidationException($"Company with ID {dto.CompanyId} does not exist.");
			}

			// Provera da li vehicle type postoji
			var vehicleType = await _unitOfWork.VehicleTypes.GetByIdAsync(dto.VehicleTypeId);
			if (vehicleType == null)
			{
				throw new ValidationException($"Vehicle type with ID {dto.VehicleTypeId} does not exist.");
			}

			// Provera da li current location postoji ako nije null
			if (dto.CurrentLocationId.HasValue)
			{
				var location = await _unitOfWork.Locations.GetByIdAsync(dto.CurrentLocationId.Value);
				if (location == null)
				{
					throw new ValidationException($"Location with ID {dto.CurrentLocationId.Value} does not exist.");
				}
			}

			// Provera duplikata registration number
			var existingVehicleWithReg = await _unitOfWork.Vehicles.GetByRegistrationNumberAsync(dto.RegistrationNumber);
			if (existingVehicleWithReg != null)
			{
				throw new ValidationException($"Registration number '{dto.RegistrationNumber}' is already assigned to another vehicle.");
			}

			var vehicleEntity = _mapper.Map<Domain.Entities.Vehicle>(dto);
			vehicleEntity.VehicleStatus = Domain.Entities.VehicleStatus.Free;

			await _unitOfWork.Vehicles.AddAsync(vehicleEntity);
			await _unitOfWork.SaveChangesAsync();

			return _mapper.Map<VehicleDto>(vehicleEntity);
		}

	}
}
