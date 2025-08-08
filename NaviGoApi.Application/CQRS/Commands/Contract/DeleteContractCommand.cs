using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.Contract
{
	public class DeleteContractCommand:IRequest<Unit>
	{
        public int Id { get; set; }
        public DeleteContractCommand(int id)
        {
            Id = id;
        }
    }
}
