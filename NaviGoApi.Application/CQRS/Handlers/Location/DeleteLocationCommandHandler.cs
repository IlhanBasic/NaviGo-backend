using MediatR;
using NaviGoApi.Application.CQRS.Commands.Location;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Location
{
	public class DeleteLocationCommandHandler : IRequestHandler<DeleteLocationCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
        public DeleteLocationCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<Unit> Handle(DeleteLocationCommand request, CancellationToken cancellationToken)
		{
			var location = await _unitOfWork.Locations.GetByIdAsync(request.Id);
			if (location != null)
			{
				await _unitOfWork.Locations.DeleteAsync(location.Id);
				await _unitOfWork.SaveChangesAsync();
			}
			return Unit.Value;
		}
	}
}
