using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.RoutePrice
{
	
	public class DeleteRoutePriceCommand:IRequest<Unit>
	{
        public int Id { get; set; }
        public DeleteRoutePriceCommand(int id)
        {
            Id = id;   
        }
    }
}
