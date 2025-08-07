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
	public class UpdateVehicleCommandHandler : IRequestHandler<UpdateVehicleCommand, VehicleDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public UpdateVehicleCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<VehicleDto> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
		{
			var existingVehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.VehicleUpdateDto.Id);
			if (existingVehicle == null)
			{
				// Handle not found, e.g. throw exception or return null
				throw new KeyNotFoundException($"Vehicle with Id {request.VehicleUpdateDto.Id} not found.");
			}

			_mapper.Map(request.VehicleUpdateDto, existingVehicle);

			_unitOfWork.Vehicles.Update(existingVehicle);
			await _unitOfWork.SaveChangesAsync();

			return _mapper.Map<VehicleDto>(existingVehicle);
		}
	}
}
