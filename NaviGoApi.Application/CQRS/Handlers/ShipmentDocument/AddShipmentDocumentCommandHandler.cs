using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.ShipmentDocument;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.ShipmentDocument
{
	public class AddShipmentDocumentCommandHandler : IRequestHandler<AddShipmentDocumentCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public AddShipmentDocumentCommandHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(AddShipmentDocumentCommand request, CancellationToken cancellationToken)
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
			var shipment = await _unitOfWork.Shipments.GetByIdAsync(request.ShipmentDocumentDto.ShipmentId);
			if (shipment == null || shipment.Contract==null || shipment.Contract.Forwarder == null || shipment.Contract.Route==null)
				throw new ValidationException("Shipment isn't valid.");
			if (user.Company.Id != shipment.Contract.Forwarder.Id &&
				user.Company.Id != shipment.Contract.Route.CompanyId)
				throw new ValidationException("User doesn't have permission to upload documents for this shipment.");
			var entity = _mapper.Map<Domain.Entities.ShipmentDocument>(request.ShipmentDocumentDto);
			entity.Verified = true;
			entity.VerifiedByUserId = user.Id;
			entity.UploadDate = DateTime.UtcNow;
			await _unitOfWork.ShipmentDocuments.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}

}
