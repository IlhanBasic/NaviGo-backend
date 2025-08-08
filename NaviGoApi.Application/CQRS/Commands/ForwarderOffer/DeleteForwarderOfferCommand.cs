using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.ForwarderOffer
{
	public class DeleteForwarderOfferCommand:IRequest<Unit>
	{
		public int Id { get; set; }
		
		public DeleteForwarderOfferCommand(int id)
        {
			Id = id;
		}
    }
}
