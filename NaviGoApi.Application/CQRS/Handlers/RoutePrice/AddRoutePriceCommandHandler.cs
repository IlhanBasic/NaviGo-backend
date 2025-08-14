using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.RoutePrice;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.RoutePrice
{
	public class AddRoutePriceCommandHandler : IRequestHandler<AddRoutePriceCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public AddRoutePriceCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		//public async Task<Unit> Handle(AddRoutePriceCommand request, CancellationToken cancellationToken)
		//{
		//	var entity = _mapper.Map<Domain.Entities.RoutePrice>(request.RoutePriceDto);
		//	await _unitOfWork.RoutePrices.AddAsync(entity);
		//	await _unitOfWork.SaveChangesAsync();
		//	return Unit.Value;
		//}
		public async Task<Unit> Handle(AddRoutePriceCommand request, CancellationToken cancellationToken)
		{
			//var routeExists = await _unitOfWork.Routes.ExistsAsync(r => r.Id == request.RoutePriceDto.RouteId);
			//if (!routeExists)
			//	throw new ValidationException($"Route with ID {request.RoutePriceDto.RouteId} does not exist.");
			var routeExists = await _unitOfWork.Routes.GetByIdAsync(request.RoutePriceDto.RouteId);
			if(routeExists == null)
				throw new ValidationException($"Route with ID {request.RoutePriceDto.RouteId} does not exist.");
			//var vehicleTypeExists = await _unitOfWork.VehicleTypes.ExistsAsync(vt => vt.Id == request.RoutePriceDto.VehicleTypeId);
			//if (!vehicleTypeExists)
			//	throw new ValidationException($"Vehicle type with ID {request.RoutePriceDto.VehicleTypeId} does not exist.");
			var vehicleTypeExists = await _unitOfWork.VehicleTypes.GetByIdAsync(request.RoutePriceDto.VehicleTypeId);
			if(vehicleTypeExists == null)
				throw new ValidationException($"Vehicle type with ID {request.RoutePriceDto.VehicleTypeId} does not exist.");
			if (request.RoutePriceDto.PricePerKm < 0)
				throw new ValidationException("Price per km cannot be negative.");

			if (request.RoutePriceDto.MinimumPrice < 0)
				throw new ValidationException("Minimum price cannot be negative.");

			//var exists = await _unitOfWork.RoutePrices.ExistsAsync(rp =>
			//	rp.RouteId == request.RoutePriceDto.RouteId &&
			//	rp.VehicleTypeId == request.RoutePriceDto.VehicleTypeId);

			//if (exists)
			//	throw new ValidationException("Price for this route and vehicle type already exists.");
			var exists = await _unitOfWork.RoutePrices.DuplicateRoutePrice(request.RoutePriceDto.RouteId, request.RoutePriceDto.VehicleTypeId);
			if (exists != null)
				throw new ValidationException("Price for this route and vehicle type already exists.");
			var entity = _mapper.Map<Domain.Entities.RoutePrice>(request.RoutePriceDto);

			await _unitOfWork.RoutePrices.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}

	}

}
