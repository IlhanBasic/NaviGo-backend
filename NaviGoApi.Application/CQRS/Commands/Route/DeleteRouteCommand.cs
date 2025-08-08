using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.Route
{
	public class DeleteRouteCommand:IRequest<Unit>
	{
        public int Id { get; set; }
        public DeleteRouteCommand(int id)
        {
            Id= id;
        }
    }
}
