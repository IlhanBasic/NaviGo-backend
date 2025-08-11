using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.VehicleMaintenance;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.VehicleMaintenance
{
	public class AddVehicleMaintenanceCommandHandler : IRequestHandler<AddVehicleMaintenanceCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
        public AddVehicleMaintenanceCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
			_unitOfWork = unitOfWork;
        }
		//      public async Task<Unit> Handle(AddVehicleMaintenanceCommand request, CancellationToken cancellationToken)
		//{
		//	var vehiclemaintenance = _mapper.Map<Domain.Entities.VehicleMaintenance>(request.Dto);
		//	if (vehiclemaintenance != null)
		//	{
		//		await _unitOfWork.VehicleMaintenances.AddAsync(vehiclemaintenance);
		//		await _unitOfWork.SaveChangesAsync();
		//	}
		//	return Unit.Value;
		//}
		public async Task<Unit> Handle(AddVehicleMaintenanceCommand request, CancellationToken cancellationToken)
		{
			var dto = request.Dto;

			// 1. Dohvati vozilo zajedno sa kompanijom
			var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(dto.VehicleId);
			if (vehicle == null)
			{
				throw new ValidationException($"Vehicle with ID {dto.VehicleId} does not exist.");
			}

			// 2. Dohvati korisnika koji prijavljuje kvar
			var user = await _unitOfWork.Users.GetByIdAsync(dto.ReportedByUserId);
			if (user == null)
			{
				throw new ValidationException($"User with ID {dto.ReportedByUserId} does not exist.");
			}

			// 3. Provera da li korisnik radi u istoj kompaniji kao vozilo i da li je CompanyAdmin
			if (user.CompanyId != vehicle.CompanyId)
			{
				throw new ValidationException("User must belong to the same company as the vehicle.");
			}

			if (user.UserRole != UserRole.CompanyAdmin)
			{
				throw new ValidationException("Only users with CompanyAdmin role can report vehicle maintenance.");
			}

			// 4. Mapiranje i snimanje
			var vehicleMaintenance = _mapper.Map<Domain.Entities.VehicleMaintenance>(dto);
			await _unitOfWork.VehicleMaintenances.AddAsync(vehicleMaintenance);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}

	}
}
