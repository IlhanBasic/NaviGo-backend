using MediatR;
using NaviGoApi.Application.DTOs.ForwarderOffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.ForwarderOffer
{
	public class AddForwarderOfferCommand:IRequest<Unit>
	{
        public ForwarderOfferCreateDto ForwarderOfferDto { get; set; }
        public AddForwarderOfferCommand(ForwarderOfferCreateDto dto)
        {
			ForwarderOfferDto = dto;

		}
    }
}
