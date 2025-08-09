using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.ShipmentDocument;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.ShipmentDocument
{
	public class AddShipmentDocumentCommandHandler : IRequestHandler<AddShipmentDocumentCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public AddShipmentDocumentCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(AddShipmentDocumentCommand request, CancellationToken cancellationToken)
		{
			var entity = _mapper.Map<Domain.Entities.ShipmentDocument>(request.ShipmentDocumentDto);
			await _unitOfWork.ShipmentDocuments.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}
}
