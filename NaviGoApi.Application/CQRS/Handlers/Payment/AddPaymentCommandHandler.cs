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
	public class AddPaymentCommandHandler : IRequestHandler<AddPaymentCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public AddPaymentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(AddPaymentCommand request, CancellationToken cancellationToken)
		{
			var entity = _mapper.Map<Domain.Entities.Payment>(request.PaymentDto);
			await _unitOfWork.Payments.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}

}
