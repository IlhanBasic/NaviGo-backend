using MediatR;
using NaviGoApi.Application.CQRS.Commands.Payment;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Payment
{
	public class DeletePaymentCommandHandler : IRequestHandler<DeletePaymentCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;

		public DeletePaymentCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
		{
			var payment = await _unitOfWork.Payments.GetByIdAsync(request.Id);
			if (payment == null)
				throw new KeyNotFoundException($"Payment with Id {request.Id} not found.");

			await _unitOfWork.Payments.DeleteAsync(payment);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}
}
