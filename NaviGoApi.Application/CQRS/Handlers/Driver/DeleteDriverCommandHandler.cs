using MediatR;
using NaviGoApi.Application.CQRS.Commands.Driver;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Driver
{
	public class DeleteDriverCommandHandler : IRequestHandler<DeleteDriverCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;

		public DeleteDriverCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(DeleteDriverCommand request, CancellationToken cancellationToken)
		{
			var driver = await _unitOfWork.Drivers.GetByIdAsync(request.Id);
			if (driver != null)
			{
				 await _unitOfWork.Drivers.DeleteAsync(driver);
				await _unitOfWork.SaveChangesAsync();
			}
			
			return Unit.Value;
		}
	}
}
