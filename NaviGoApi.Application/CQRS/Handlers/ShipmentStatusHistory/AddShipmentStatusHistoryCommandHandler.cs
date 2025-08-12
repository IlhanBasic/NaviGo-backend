using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.ShipmentStatusHistory;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace NaviGoApi.Application.CQRS.Handlers.ShipmentStatusHistory
{
	public class AddShipmentStatusHistoryCommandHandler : IRequestHandler<AddShipmentStatusHistoryCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public AddShipmentStatusHistoryCommandHandler(
			IMapper mapper,
			IUnitOfWork unitOfWork,
			IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(AddShipmentStatusHistoryCommand request, CancellationToken cancellationToken)
		{
			// 🔹 1. Validacija HttpContext-a
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

			// 🔹 2. Provera pošiljke
			var shipment = await _unitOfWork.Shipments.GetByIdAsync(request.ShipmentStatusHistoryDto.ShipmentId)
				?? throw new KeyNotFoundException("Shipment not found.");

			if (shipment.Contract == null || shipment.Contract.Forwarder == null || shipment.Contract.Route == null)
				throw new ValidationException("Shipment is not linked with a valid contract.");

			// 🔹 3. Provera da li user ima pravo da menja status
			if (user.Company.Id != shipment.Contract.Forwarder.Id &&
				user.Company.Id != shipment.Contract.Route.CompanyId)
				throw new ValidationException("User is not authorized to change shipment status.");

			// 🔹 4. Preuzimanje poslednjeg statusa
			var lastStatus = await _unitOfWork.ShipmentStatusHistories
				.GetLastStatusForShipmentAsync(shipment.Id);

			// 🔹 5. Validacija prelaza statusa
			if (lastStatus != null)
			{
				if (lastStatus.ShipmentStatus == ShipmentStatus.Delivered)
					throw new InvalidOperationException("Cannot change status after delivery.");

				if (lastStatus.ShipmentStatus == ShipmentStatus.Cancelled)
					throw new InvalidOperationException("Cannot change status after cancellation.");
			}

			// 🔹 6. Kreiranje novog statusa
			var entity = _mapper.Map<Domain.Entities.ShipmentStatusHistory>(request.ShipmentStatusHistoryDto);
			entity.ChangedAt = DateTime.UtcNow;
			entity.ChangedByUserId = user.Id; // automatski iz tokena

			await _unitOfWork.ShipmentStatusHistories.AddAsync(entity);

			// 🔹 7. Ažuriranje glavnog Shipment statusa
			shipment.Status = entity.ShipmentStatus;
			await _unitOfWork.Shipments.UpdateAsync(shipment);

			// 🔹 8. Snimanje promena
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}
}
