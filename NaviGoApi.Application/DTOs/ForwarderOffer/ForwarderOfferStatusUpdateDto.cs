using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.ForwarderOffer
{
	public class ForwarderOfferStatusUpdateDto
	{
		public int ForwarderOfferId { get; set; }
		public ForwarderOfferStatus NewStatus { get; set; }
		public string? RejectionReason { get; set; }
	}
}
