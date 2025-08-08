using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Queries.Payment;
using NaviGoApi.Application.DTOs.Payment;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Payment
{
	public class GetAllPaymentQueryHandler : IRequestHandler<GetAllPaymentQuery, IEnumerable<PaymentDto?>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public GetAllPaymentQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<IEnumerable<PaymentDto?>> Handle(GetAllPaymentQuery request, CancellationToken cancellationToken)
		{
			var payments = await _unitOfWork.Payments.GetAllAsync();
			return _mapper.Map<IEnumerable<PaymentDto>>(payments);
		}
	}
}
