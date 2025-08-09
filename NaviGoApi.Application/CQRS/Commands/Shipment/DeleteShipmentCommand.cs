using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.Shipment
{
	public class DeleteShipmentCommand:IRequest<Unit>
	{
        public int Id { get; set; }
        public DeleteShipmentCommand(int id)
        {
            Id = id;
        }
    }
}
