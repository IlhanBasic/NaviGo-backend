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
	public class GetAllShipmentDocumentQueryHandler : IRequestHandler<GetAllShipmentDocumentQuery, IEnumerable<ShipmentDocumentDto?>>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public GetAllShipmentDocumentQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<IEnumerable<ShipmentDocumentDto?>> Handle(GetAllShipmentDocumentQuery request, CancellationToken cancellationToken)
		{
			var entities = await _unitOfWork.ShipmentDocuments.GetAllAsync();
			return _mapper.Map<IEnumerable<ShipmentDocumentDto>>(entities);
		}
	}
}
