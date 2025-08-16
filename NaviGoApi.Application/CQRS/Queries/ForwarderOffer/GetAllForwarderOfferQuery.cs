using MediatR;
using NaviGoApi.Application.DTOs.ForwarderOffer;
using NaviGoApi.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Queries.ForwarderOffer
{
	public class GetAllForwarderOfferQuery:IRequest<IEnumerable<ForwarderOfferDto?>>
	{
        public ForwarderOfferSearchDto Search {  get; set; }
        public GetAllForwarderOfferQuery(ForwarderOfferSearchDto search)
        {
            Search = search;
        }
    }
}
