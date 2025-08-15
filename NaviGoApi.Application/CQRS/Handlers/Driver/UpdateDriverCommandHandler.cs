using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Driver
{
	public class UpdateDriverCommandHandler : IRequestHandler<UpdateDriverCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public UpdateDriverCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<Unit> Handle(UpdateDriverCommand request, CancellationToken cancellationToken)
		{
			var existingDriver = await _unitOfWork.Drivers.GetByIdAsync(request.Id);
			if (existingDriver == null)
			{
				throw new KeyNotFoundException($"Driver with ID {request.Id} not found.");
			}
			if (request.DriverDto.DriverStatus == DriverStatus.OnRoute)
			{
				var allShipments = await _unitOfWork.Shipments.GetAllAsync();
				var assignedShipments = allShipments
					.Where(s => s.DriverId == existingDriver.Id &&
								(s.Status == ShipmentStatus.Scheduled || s.Status == ShipmentStatus.InTransit))
					.ToList();

				if (!assignedShipments.Any())
				{
					throw new ValidationException("Driver cannot be set to OnRoute because no shipment/route is assigned.");
				}
			}
			_mapper.Map(request.DriverDto, existingDriver);
			await _unitOfWork.Drivers.UpdateAsync(existingDriver);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}
}
