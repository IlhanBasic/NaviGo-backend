using MediatR;
using NaviGoApi.Application.CQRS.Commands.RoutePrice;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.RoutePrice
{
	public class DeleteRoutePriceCommandHandler : IRequestHandler<DeleteRoutePriceCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;

		public DeleteRoutePriceCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(DeleteRoutePriceCommand request, CancellationToken cancellationToken)
		{
			await _unitOfWork.RoutePrices.DeleteAsync(request.Id);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}

}
