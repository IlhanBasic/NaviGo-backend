using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.ShipmentStatusHistory;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.ShipmentStatusHistory
{
	public class UpdateShipmentStatusHistoryCommandHandler : IRequestHandler<UpdateShipmentStatusHistoryCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public UpdateShipmentStatusHistoryCommandHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(UpdateShipmentStatusHistoryCommand request, CancellationToken cancellationToken)
		{
			if (_httpContextAccessor.HttpContext == null)
				throw new ValidationException("HttpContext is null.");

			var userEmail = _httpContextAccessor.HttpContext.User
				.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

			if (string.IsNullOrEmpty(userEmail))
				throw new ValidationException("User email not found in token.");

			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException("User not found.");

			if (user.UserStatus != UserStatus.Active || user.UserRole != UserRole.CompanyAdmin)
				throw new ValidationException("Only active Company Admins can update shipment status.");

			if (user.CompanyId == null)
				throw new ValidationException("User is not associated with any company.");

			var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);
			if (company == null)
				throw new ValidationException("User is not associated with any company.");

			var existing = await _unitOfWork.ShipmentStatusHistories.GetByIdAsync(request.Id)
				?? throw new KeyNotFoundException($"ShipmentStatusHistory with Id {request.Id} not found.");

			var shipment = await _unitOfWork.Shipments.GetByIdAsync(existing.ShipmentId)
				?? throw new ValidationException("Shipment not found.");

			var contract = await _unitOfWork.Contracts.GetByIdAsync(shipment.ContractId)
				?? throw new ValidationException("Contract isn't valid.");

			var forwarder = await _unitOfWork.Companies.GetByIdAsync(contract.ForwarderId)
				?? throw new ValidationException("Forwarder isn't valid.");

			var route = await _unitOfWork.Routes.GetByIdAsync(contract.RouteId)
				?? throw new ValidationException("Route isn't valid.");
			if (company.Id != forwarder.Id && company.Id != route.CompanyId)
				throw new ValidationException("User is not authorized to change shipment status.");
			if (request.ShipmentStatusHistoryDto.ShipmentStatus != existing.ShipmentStatus)
			{
				var lastStatus = await _unitOfWork.ShipmentStatusHistories
					.GetLastStatusForShipmentAsync(existing.ShipmentId);

				if (lastStatus?.ShipmentStatus == ShipmentStatus.Delivered)
					throw new ValidationException("Cannot change status after delivery.");

				if (lastStatus?.ShipmentStatus == ShipmentStatus.Cancelled)
					throw new ValidationException("Cannot change status after cancellation.");

				existing.ShipmentStatus = request.ShipmentStatusHistoryDto.ShipmentStatus;
			}
			existing.Notes = request.ShipmentStatusHistoryDto.Notes;
			existing.ChangedAt = DateTime.UtcNow;
			existing.ChangedByUserId = user.Id;

			await _unitOfWork.ShipmentStatusHistories.UpdateAsync(existing);
			shipment.Status = existing.ShipmentStatus;
			await _unitOfWork.Shipments.UpdateAsync(shipment);

			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}
}
