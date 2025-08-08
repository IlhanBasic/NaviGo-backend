using MediatR;
using NaviGoApi.Application.DTOs.ForwarderOffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Queries.ForwarderOffer
{
	public class GetForwarderOfferByIdQuery:IRequest<ForwarderOfferDto?>
	{
        public int Id { get; set; }
        public GetForwarderOfferByIdQuery(int id)
        {
            Id= id;
        }
    }
}
