using AutoMapper;
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
	public class UpdatePaymentCommandHandler : IRequestHandler<UpdatePaymentCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public UpdatePaymentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
		{
			var payment = await _unitOfWork.Payments.GetByIdAsync(request.Id);
			if (payment == null)
				throw new KeyNotFoundException($"Payment with Id {request.Id} not found.");

			// Map update DTO onto existing entity (ignoring nulls could be handled here if needed)
			_mapper.Map(request.PaymentDto, payment);

			await _unitOfWork.Payments.UpdateAsync(payment);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}
}
