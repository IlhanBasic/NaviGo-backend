using MediatR;
using NaviGoApi.Application.DTOs.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Queries.Payment
{
	public class GetPaymentByIdQuery:IRequest<PaymentDto?>
	{
        public int Id { get; set; }
        public GetPaymentByIdQuery(int id)
        {
            Id= id;
        }
    }
}
