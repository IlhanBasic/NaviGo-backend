using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.ForwarderOffer
{
	public class ForwarderOfferUpdateDto
	{
		public decimal? CommissionRate { get; set; }
		public ForwarderOfferStatus? ForwarderOfferStatus { get; set; }
		public string? RejectionReason { get; set; }
		public DateTime? ExpiresAt { get; set; }
		public decimal? DiscountRate { get; set; }
	}
}
