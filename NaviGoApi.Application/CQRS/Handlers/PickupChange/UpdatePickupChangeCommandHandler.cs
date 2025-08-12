using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.PickupChange;
using NaviGoApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.PickupChange
{
	public class UpdatePickupChangeCommandHandler : IRequestHandler<UpdatePickupChangeCommand>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public UpdatePickupChangeCommandHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(UpdatePickupChangeCommand request, CancellationToken cancellationToken)
		{
			var existingEntity = await _unitOfWork.PickupChanges.GetByIdAsync(request.Id);
			if (existingEntity == null)
			{
				throw new KeyNotFoundException($"PickupChange with Id {request.Id} not found.");
			}

			if (_httpContextAccessor.HttpContext == null)
				throw new ValidationException("HttpContext is null.");

			var userEmail = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
			if (string.IsNullOrEmpty(userEmail))
				throw new ValidationException("User email not found in token.");

			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail);
			if (user == null)
				throw new ValidationException("User not found.");

			if (existingEntity.ClientId != user.Id)
				throw new ValidationException("User not authorized to update this pickup change.");

			var shipment = await _unitOfWork.Shipments.GetByIdAsync(existingEntity.ShipmentId);
			if (shipment == null)
				throw new ValidationException("Associated shipment not found.");
			_mapper.Map(request.PickupChangeDto, existingEntity);
			existingEntity.ChangeCount++;
			if (existingEntity.ChangeCount > 2)
			{
				existingEntity.AdditionalFee = 50m; 
			}
			else
			{
				existingEntity.AdditionalFee = 0m;
			}

			await _unitOfWork.PickupChanges.UpdateAsync(existingEntity);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}

	}
}
