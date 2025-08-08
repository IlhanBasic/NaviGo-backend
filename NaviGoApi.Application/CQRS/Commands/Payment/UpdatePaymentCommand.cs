using MediatR;
using NaviGoApi.Application.DTOs.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.Payment
{

	public class UpdatePaymentCommand:IRequest<Unit>
	{
        public int Id { get; set; }
        public PaymentUpdateDto PaymentDto { get; set; }
        public UpdatePaymentCommand(PaymentUpdateDto dto)
        {
            PaymentDto = dto;
        }
    }
}
