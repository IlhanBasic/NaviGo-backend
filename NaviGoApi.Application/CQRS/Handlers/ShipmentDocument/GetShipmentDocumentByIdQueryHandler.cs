using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Queries.ShipmentDocument;
using NaviGoApi.Application.DTOs.ShipmentDocument;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.ShipmentDocument
{

	public class GetShipmentDocumentByIdQueryHandler : IRequestHandler<GetShipmentDocumentByIdQuery, ShipmentDocumentDto?>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public GetShipmentDocumentByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<ShipmentDocumentDto?> Handle(GetShipmentDocumentByIdQuery request, CancellationToken cancellationToken)
		{
			var entity = await _unitOfWork.ShipmentDocuments.GetByIdAsync(request.Id);
			if (entity == null) return null;
			return _mapper.Map<ShipmentDocumentDto>(entity);
		}
	}
}
