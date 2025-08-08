using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.Payment
{
	public class DeletePaymentCommand:IRequest<Unit>
	{
        public int Id { get; set; }
        public DeletePaymentCommand(int id)
        {
            Id = id;
        }
    }
}
