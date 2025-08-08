using MediatR;
using NaviGoApi.Application.DTOs.ForwarderOffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.ForwarderOffer
{
	public class UpdateForwarderOfferCommand:IRequest<Unit>
	{
        public int Id { get; set; }
        public ForwarderOfferUpdateDto ForwarderOfferDto { get; set; }
        public UpdateForwarderOfferCommand(int id, ForwarderOfferUpdateDto dto)
        {
            Id=id;
            ForwarderOfferDto=dto;
        }
    }
}
