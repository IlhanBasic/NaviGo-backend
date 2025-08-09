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
	public class DeleteShipmentDocumentCommandHandler : IRequestHandler<DeleteShipmentDocumentCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;

		public DeleteShipmentDocumentCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(DeleteShipmentDocumentCommand request, CancellationToken cancellationToken)
		{
			await _unitOfWork.ShipmentDocuments.DeleteAsync(request.Id);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}
}
