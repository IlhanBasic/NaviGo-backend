using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.ShipmentStatusHistory;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
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

			if (user.Company == null)
				throw new ValidationException("User is not associated with any company.");

			var existing = await _unitOfWork.ShipmentStatusHistories.GetByIdAsync(request.Id)
				?? throw new KeyNotFoundException($"ShipmentStatusHistory with Id {request.Id} not found.");

			// Proveri da li user ima pravo pristupa ovoj pošiljci
			if (existing.Shipment?.Contract == null ||
				existing.Shipment.Contract.Forwarder == null ||
				existing.Shipment.Contract.Route == null)
				throw new ValidationException("Shipment is not linked with a valid contract.");

			if (user.Company.Id != existing.Shipment.Contract.Forwarder.Id &&
				user.Company.Id != existing.Shipment.Contract.Route.CompanyId)
				throw new ValidationException("User is not authorized to update shipment status history.");

			// Validacija statusa (ako se menja)
			if (request.ShipmentStatusHistoryDto.ShipmentStatus != existing.ShipmentStatus)
			{
				var lastStatus = await _unitOfWork.ShipmentStatusHistories
					.GetLastStatusForShipmentAsync(existing.ShipmentId);

				if (lastStatus?.ShipmentStatus == ShipmentStatus.Delivered)
					throw new InvalidOperationException("Cannot change status after delivery.");

				if (lastStatus?.ShipmentStatus == ShipmentStatus.Cancelled)
					throw new InvalidOperationException("Cannot change status after cancellation.");

				existing.ShipmentStatus = request.ShipmentStatusHistoryDto.ShipmentStatus;
			}

			// Notes može slobodno da se menja
			existing.Notes = request.ShipmentStatusHistoryDto.Notes;

			// Osveži vreme i korisnika izmene
			existing.ChangedAt = DateTime.UtcNow;
			existing.ChangedByUserId = user.Id;

			await _unitOfWork.ShipmentStatusHistories.UpdateAsync(existing);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}

	}
}
