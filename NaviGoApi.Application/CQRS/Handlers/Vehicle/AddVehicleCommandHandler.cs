using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.Vehicle;
using NaviGoApi.Application.DTOs.Vehicle;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
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

		public async Task<VehicleDto> Handle(AddVehicleCommand request, CancellationToken cancellationToken)
		{
			var vehicleEntity = _mapper.Map<Domain.Entities.Vehicle>(request.VehicleCreateDto);
			vehicleEntity.VehicleStatus = Domain.Entities.VehicleStatus.Free;
			await _unitOfWork.Vehicles.AddAsync(vehicleEntity);
			await _unitOfWork.SaveChangesAsync();

			return _mapper.Map<VehicleDto>(vehicleEntity);
		}
	}
}
