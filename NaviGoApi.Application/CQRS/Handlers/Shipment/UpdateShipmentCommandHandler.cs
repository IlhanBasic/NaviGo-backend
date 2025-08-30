using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.Shipment;
using NaviGoApi.Application.Services;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Shipment
{
	public class UpdateShipmentCommandHandler : IRequestHandler<UpdateShipmentCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDelayPenaltyCalculationService _delayPenaltyService;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public UpdateShipmentCommandHandler(
			IMapper mapper,
			IUnitOfWork unitOfWork,
			IDelayPenaltyCalculationService delayPenaltyCalculation,
			IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_delayPenaltyService = delayPenaltyCalculation;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(UpdateShipmentCommand request, CancellationToken cancellationToken)
		{
			var httpContext = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("HttpContext is not available.");

			var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
			if (string.IsNullOrWhiteSpace(userEmail))
				throw new ValidationException("User email not found in authentication token.");

			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException($"User with email '{userEmail}' not found.");

			if (user.UserStatus != UserStatus.Active)
				throw new ValidationException("Your account is not activated.");

			if (user.UserRole != UserRole.CompanyAdmin)
				throw new ValidationException("You are not allowed to update shipment.");

			var existing = await _unitOfWork.Shipments.GetByIdAsync(request.Id);
			if (existing == null)
				throw new ValidationException($"Shipment with ID {request.Id} not found.");

			var contract = await _unitOfWork.Contracts.GetByIdAsync(existing.ContractId);
			if (contract == null)
				throw new ValidationException($"Contract with ID {existing.ContractId} not found.");

			var route = await _unitOfWork.Routes.GetByIdAsync(contract.RouteId);
			if (route == null)
				throw new ValidationException($"Route with ID {contract.RouteId} not found.");

			var company = await _unitOfWork.Companies.GetByIdAsync(route.CompanyId);
			if (company == null)
				throw new ValidationException($"Company with ID {route.CompanyId} not found.");

			if (company.CompanyType != CompanyType.Carrier)
				throw new ValidationException($"Company must be Carrier type for update Shipment.");

			if (user.CompanyId != company.Id)
				throw new ValidationException($"You cannot update shipment for wrong company.");

			if (request.ShipmentDto.ActualDeparture.HasValue && request.ShipmentDto.ActualArrival.HasValue)
			{
				if (request.ShipmentDto.ActualDeparture > request.ShipmentDto.ActualArrival)
					throw new ValidationException("ActualDeparture cannot be later than ActualArrival.");
			}

			var newStatus = request.ShipmentDto.Status;
			var currentStatus = existing.Status;

			// Validacija prelaza
			if (newStatus != currentStatus && !IsValidStatusTransition(currentStatus, newStatus))
				throw new ValidationException($"Cannot change status from {currentStatus} to {newStatus}.");

			// Mapiraj izmene
			_mapper.Map(request.ShipmentDto, existing);

			existing.ScheduledArrival = DateTime.SpecifyKind(existing.ScheduledArrival, DateTimeKind.Utc);
			existing.ScheduledDeparture = DateTime.SpecifyKind(existing.ScheduledDeparture, DateTimeKind.Utc);

			await _unitOfWork.Shipments.UpdateAsync(existing);
			await _unitOfWork.SaveChangesAsync();

			// Računaj penal samo ako je shipment sada Delivered
			if (existing.Status == ShipmentStatus.Delivered)
			{
				await _delayPenaltyService.CalculateAndCreatePenaltyAsync(existing);
				await _unitOfWork.SaveChangesAsync();
			}

			return Unit.Value;
		}

		private bool IsValidStatusTransition(ShipmentStatus current, ShipmentStatus next)
		{
			// dozvoli update kada je status isti
			if (current == next)
				return true;

			return current switch
			{
				ShipmentStatus.Scheduled => next == ShipmentStatus.InTransit || next == ShipmentStatus.Cancelled,
				ShipmentStatus.InTransit => next == ShipmentStatus.Delivered || next == ShipmentStatus.Delayed || next == ShipmentStatus.Cancelled,
				ShipmentStatus.Delayed => next == ShipmentStatus.Delivered || next == ShipmentStatus.Cancelled,
				ShipmentStatus.Delivered => false,
				ShipmentStatus.Cancelled => false,
				_ => false
			};
		}
	}
}
