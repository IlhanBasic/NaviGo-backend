
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Entities
{
	public enum ForwarderOfferStatus
	{
		Pending = 0,
		Accepted = 1,
		Rejected = 2
	}

	public class ForwarderOffer
	{
		public int Id { get; set; }
		public int RouteId { get; set; }
		public int ForwarderId { get; set; }
		public decimal CommissionRate { get; set; }
		public ForwarderOfferStatus ForwarderOfferStatus { get; set; }
		public string? RejectionReason { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime ExpiresAt { get; set; }
		public decimal DiscountRate { get; set; }

		// Navigaciona svojstva
		public Route? Route { get; set; }
		public Company? Forwarder { get; set; }
	}
}
