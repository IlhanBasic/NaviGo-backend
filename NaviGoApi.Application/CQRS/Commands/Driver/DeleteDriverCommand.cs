using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.Driver
{
	public class DeleteDriverCommand:IRequest<Unit>
	{
        public int Id { get; set; }
        public DeleteDriverCommand(int id)
        {
            Id = id;
        }
    }
}
