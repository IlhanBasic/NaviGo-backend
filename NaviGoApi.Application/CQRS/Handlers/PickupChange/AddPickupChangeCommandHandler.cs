using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.PickupChange;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace NaviGoApi.Application.CQRS.Handlers.PickupChange
{
	public class AddPickupChangeCommandHandler : IRequestHandler<AddPickupChangeCommand, int>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public AddPickupChangeCommandHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<int> Handle(AddPickupChangeCommand request, CancellationToken cancellationToken)
		{
			if (_httpContextAccessor.HttpContext == null)
				throw new ValidationException("HttpContext is null.");
			var userEmail = _httpContextAccessor.HttpContext.User
				.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
			if (string.IsNullOrEmpty(userEmail))
				throw new ValidationException("User email not found in token.");
			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException("User not found.");
			if (user.UserStatus != UserStatus.Active)
				throw new ValidationException("User must be activated.");
			if (user.UserRole != UserRole.RegularUser)
				throw new ValidationException("You are not allowed to add pickup change");
			var existingPickupChange = await _unitOfWork.PickupChanges
				.GetByShipmentAndClientAsync(request.PickupChangeDto.ShipmentId, user.Id);
			if (existingPickupChange != null)
				throw new ValidationException("You already have an active pickup change for this shipment.");
			var shipment = await _unitOfWork.Shipments.GetByIdAsync(request.PickupChangeDto.ShipmentId)
				?? throw new KeyNotFoundException("Shipment not found.");
			if (shipment.Status == ShipmentStatus.Delivered || shipment.Status == ShipmentStatus.Cancelled)
				throw new ValidationException("Shipment is finished so cannot change pickup.");
			if ((request.PickupChangeDto.NewTime - DateTime.UtcNow).TotalDays > 7)
				throw new ValidationException("Pickup change can be made at most 7 days in advance.");
			var entity = _mapper.Map<Domain.Entities.PickupChange>(request.PickupChangeDto);
			entity.ClientId = user.Id;
			entity.OldTime = shipment.ScheduledDeparture;
			entity.ChangeCount = 0;
			entity.AdditionalFee = 0;
			await _unitOfWork.PickupChanges.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return entity.Id;
		}
	}
}
