using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.ShipmentStatusHistory;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

public class DeleteShipmentStatusHistoryCommandHandler : IRequestHandler<DeleteShipmentStatusHistoryCommand, Unit>
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IHttpContextAccessor _httpContextAccessor;

	public DeleteShipmentStatusHistoryCommandHandler(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
	{
		_unitOfWork = unitOfWork;
		_httpContextAccessor = httpContextAccessor;
	}

	public async Task<Unit> Handle(DeleteShipmentStatusHistoryCommand request, CancellationToken cancellationToken)
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
			throw new ValidationException("Your account is not activated.");
		if (user.UserRole != UserRole.CompanyAdmin)
			throw new ValidationException("Only Company Admins can delete shipment status.");
		if (user.CompanyId == null)
			throw new ValidationException("User is not associated with any company.");
		var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value)
			?? throw new ValidationException("Company not found.");
		var statusHistory = await _unitOfWork.ShipmentStatusHistories.GetByIdAsync(request.Id)
			?? throw new ValidationException("Shipment status history not found.");
		var shipment = await _unitOfWork.Shipments.GetByIdAsync(statusHistory.ShipmentId)
			?? throw new ValidationException("Shipment not found.");
		var contract = await _unitOfWork.Contracts.GetByIdAsync(shipment.ContractId)
			?? throw new ValidationException("Contract not found.");
		var forwarder = await _unitOfWork.Companies.GetByIdAsync(contract.ForwarderId)
			?? throw new ValidationException("Forwarder not found.");
		var route = await _unitOfWork.Routes.GetByIdAsync(contract.RouteId)
			?? throw new ValidationException("Route not found.");
		if (company.Id != forwarder.Id && company.Id != route.CompanyId)
			throw new ValidationException("User is not authorized to delete this shipment status.");
		if (statusHistory.ShipmentStatus == ShipmentStatus.Delivered ||
			statusHistory.ShipmentStatus == ShipmentStatus.Cancelled)
			throw new ValidationException("Cannot delete Delivered or Cancelled status.");
		await _unitOfWork.ShipmentStatusHistories.DeleteAsync(request.Id);
		await _unitOfWork.SaveChangesAsync();

		return Unit.Value;
	}
}
