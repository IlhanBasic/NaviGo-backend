using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.ShipmentDocument;
using NaviGoApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.ShipmentDocument
{
	public class UpdateShipmentDocumentCommandHandler : IRequestHandler<UpdateShipmentDocumentCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public UpdateShipmentDocumentCommandHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(UpdateShipmentDocumentCommand request, CancellationToken cancellationToken)
		{
			if (_httpContextAccessor.HttpContext == null)
				throw new ValidationException("HttpContext is null.");

			var userEmail = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
			if (string.IsNullOrEmpty(userEmail))
				throw new ValidationException("User email not found in token.");

			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail);
			if (user == null)
				throw new ValidationException("User not found.");

			if (user.Company == null)
				throw new ValidationException("User is not associated with any company.");

			// Prvo dohvati shipment dokument po ID-u iz request.Id
			var shipmentDocument = await _unitOfWork.ShipmentDocuments.GetByIdAsync(request.Id);
			if (shipmentDocument == null)
				throw new ValidationException($"Shipment Document with ID {request.Id} doesn't exist.");

			// Iz dokumenta dohvati shipment
			var shipment = await _unitOfWork.Shipments.GetByIdAsync(shipmentDocument.ShipmentId);
			if (shipment == null || shipment.Contract == null || shipment.Contract.Forwarder == null || shipment.Contract.Route == null)
				throw new ValidationException("Shipment isn't valid.");

			// Provera dozvole: da li korisnikova kompanija pripada forwarderu ili ruti u ugovoru
			if (user.Company.Id != shipment.Contract.Forwarder.Id &&
				user.Company.Id != shipment.Contract.Route.CompanyId)
				throw new ValidationException("User doesn't have permission to update shipment documents.");

			// Ažuriraj polja
			shipmentDocument.UploadDate = DateTime.UtcNow;
			shipmentDocument.Verified = true;

			// Mapiraj ostala polja iz DTO na entitet
			_mapper.Map(request.ShipmentDocumentDto, shipmentDocument);

			// Snimi promene
			await _unitOfWork.ShipmentDocuments.UpdateAsync(shipmentDocument);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}
}
