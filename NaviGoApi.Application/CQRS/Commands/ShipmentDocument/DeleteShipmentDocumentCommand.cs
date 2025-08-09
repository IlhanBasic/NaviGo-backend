using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.ShipmentDocument
{
	public class DeleteShipmentDocumentCommand:IRequest<Unit>
	{
        public int Id { get; set; }
        public DeleteShipmentDocumentCommand(int id)
        {
            Id= id;
        }
    }
}
