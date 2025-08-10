using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.ForwarderOffer;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.ForwarderOffer
{
	public class DeleteForwarderOfferCommandHandler : IRequestHandler<DeleteForwarderOfferCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;

		public DeleteForwarderOfferCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(DeleteForwarderOfferCommand request, CancellationToken cancellationToken)
		{
			var entity = await _unitOfWork.ForwarderOffers.GetByIdAsync(request.Id);
			if (entity == null)
				throw new KeyNotFoundException($"ForwarderOffer with id {request.Id} not found.");

			await _unitOfWork.ForwarderOffers.DeleteAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}
}
