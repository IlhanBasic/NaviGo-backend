using MediatR;
using NaviGoApi.Application.DTOs.ForwarderOffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.ForwarderOffer
{
	public class UpdateForwarderOfferStatusCommand : IRequest<Unit>
	{
		public ForwarderOfferStatusUpdateDto ForwarderOfferDto { get; set; }
		public int Id { get; set; }
        public UpdateForwarderOfferStatusCommand(int id , ForwarderOfferStatusUpdateDto forwarderOffer)
        {
			ForwarderOfferDto= forwarderOffer;
			Id= id;
		}
    }

}
