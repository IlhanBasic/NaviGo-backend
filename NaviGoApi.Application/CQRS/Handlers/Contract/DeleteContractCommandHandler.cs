using MediatR;
using NaviGoApi.Application.CQRS.Commands.Contract;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Contract
{
	public class DeleteContractCommandHandler : IRequestHandler<DeleteContractCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		public DeleteContractCommandHandler(IUnitOfWork unitOfWork)
        {
			_unitOfWork=unitOfWork;

		}
		public async Task<Unit> Handle(DeleteContractCommand request, CancellationToken cancellationToken)
		{
			var contract = await _unitOfWork.Contracts.GetByIdAsync(request.Id);
			if (contract == null)
			{
				throw new KeyNotFoundException($"Contract with ID {request.Id} not found.");
			}

			_unitOfWork.Contracts.Delete(contract);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}

	}
}
