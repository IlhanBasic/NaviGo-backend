using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.PickupChange;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.PickupChange
{
	public class UpdatePickupChangeCommandHandler : IRequestHandler<UpdatePickupChangeCommand, Unit>
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
				throw new ValidationException($"PickupChange with Id {request.Id} not found.");
			if (_httpContextAccessor.HttpContext == null)
				throw new ValidationException("HttpContext is null.");
			var userEmail = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
			if (string.IsNullOrEmpty(userEmail))
				throw new ValidationException("User email not found in token.");
			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail);
			if (user == null)
				throw new ValidationException("User not found.");
			if((user.CompanyId == null && user.UserRole != UserRole.RegularUser) ||
				(user.CompanyId != null && user.UserRole!=UserRole.CompanyAdmin))
				throw new ValidationException("User not authorized to update this pickup change.");
			if (user.CompanyId != null)
			{
			var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);
				if (company == null)
					throw new ValidationException($"Company with ID {user.CompanyId.Value} doesn't exists.");
			if (company.CompanyType != CompanyType.Carrier )
				throw new ValidationException("User not authorized to update this pickup change.");
			}
			var shipment = await _unitOfWork.Shipments.GetByIdAsync(existingEntity.ShipmentId);
			if (shipment == null)
				throw new ValidationException("Associated shipment not found.");
			if (shipment.Status == ShipmentStatus.Delivered || shipment.Status == ShipmentStatus.Cancelled)
				throw new ValidationException("Shipment is finished so cannot change pickup.");
			_mapper.Map(request.PickupChangeDto, existingEntity);
			existingEntity.ChangeCount++;
			existingEntity.AdditionalFee = existingEntity.ChangeCount > 2 ? 50m : 0m;
			existingEntity.OldTime = DateTime.SpecifyKind(existingEntity.OldTime, DateTimeKind.Utc);
			existingEntity.NewTime = DateTime.SpecifyKind(existingEntity.NewTime, DateTimeKind.Utc);
			await _unitOfWork.PickupChanges.UpdateAsync(existingEntity);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}
}
