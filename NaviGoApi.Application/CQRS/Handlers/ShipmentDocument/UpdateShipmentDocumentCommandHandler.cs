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
	public class UpdateShipmentDocumentCommandHandler : IRequestHandler<UpdateShipmentDocumentCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public UpdateShipmentDocumentCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(UpdateShipmentDocumentCommand request, CancellationToken cancellationToken)
		{
			var existingEntity = await _unitOfWork.ShipmentDocuments.GetByIdAsync(request.Id);
			if (existingEntity == null)
			{
				// opcionalno: baci exception ili samo vrati Unit.Value ako nema update-a
				return Unit.Value;
			}

			// mapiraj polja sa DTO na entitet (osveži postojeći entitet)
			_mapper.Map(request.ShipmentDocumentDto, existingEntity);

			await _unitOfWork.ShipmentDocuments.UpdateAsync(existingEntity);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}
}
