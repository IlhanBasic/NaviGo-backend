using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.Location
{
	public class DeleteLocationCommand:IRequest<Unit>
	{
        public int Id { get; set; }
        public DeleteLocationCommand(int id)
        {
            Id=id;
        }
    }
}
