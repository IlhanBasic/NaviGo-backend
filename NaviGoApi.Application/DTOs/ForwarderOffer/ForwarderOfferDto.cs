using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.ForwarderOffer
{
	public class ForwarderOfferDto
	{
		public int Id { get; set; }
		public int RouteId { get; set; }
		public int ForwarderId { get; set; }
		public decimal CommissionRate { get; set; }
		public string ForwarderOfferStatus { get; set; }  
		public string? RejectionReason { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime ExpiresAt { get; set; }
		public decimal DiscountRate { get; set; }
		public string? RouteName { get; set; }  // npr. StartLocation - EndLocation
		public string? ForwarderCompanyName { get; set; }
	}
}
