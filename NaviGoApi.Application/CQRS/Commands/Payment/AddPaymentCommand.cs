using MediatR;
using NaviGoApi.Application.DTOs.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.Payment
{
	public class AddPaymentCommand:IRequest<Unit>
	{
        public PaymentCreateDto PaymentDto { get; set; }
        public AddPaymentCommand(PaymentCreateDto dto)
        {
            PaymentDto = dto;
        }
    }
}
