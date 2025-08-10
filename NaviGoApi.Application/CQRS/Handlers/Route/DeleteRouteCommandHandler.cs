using MediatR;
using NaviGoApi.Application.CQRS.Commands.Route;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Route
{
	public class DeleteRouteCommandHandler : IRequestHandler<DeleteRouteCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		public DeleteRouteCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(DeleteRouteCommand request, CancellationToken cancellationToken)
		{
			var route = await _unitOfWork.Routes.GetByIdAsync(request.Id);
			if (route == null)
				throw new KeyNotFoundException($"Route with ID {request.Id} not found");

			await _unitOfWork.Routes.DeleteAsync(route);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}

}
