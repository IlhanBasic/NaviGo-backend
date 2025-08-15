
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Entities
{
	public enum ContractStatus
	{
		Pending = 0,    // Čeka na odobrenje
		Active = 1,     // Aktivan ugovor
		Completed = 2,  // Završeni ugovor
		Cancelled = 3   // Otkazan ugovor
	}
	public class Contract
	{
		public int Id { get; set; }

		public int ClientId { get; set; }    
		public int ForwarderId { get; set; }
		public int RouteId { get; set; }       

		public string ContractNumber { get; set; } = null!;
		public DateTime ContractDate { get; set; }
		public string Terms { get; set; } = null!;

		public ContractStatus ContractStatus { get; set; }
		public decimal PenaltyRatePerHour { get; set; }
		public decimal MaxPenaltyPercent { get; set; }

		//public DateTime ValidUntil { get; set; }
		public DateTime? SignedDate { get; set; }

		public User? Client { get; set; }
		public Company? Forwarder { get; set; }
		public Route? Route { get; set; }

		public ICollection<Shipment>? Shipments { get; set; }
		public Payment? Payment { get; set; }  // 1:1 sa Payment
		public int RoutePriceId { get; set; }
		public RoutePrice RoutePrice { get; set; }

		public int ForwarderOfferId { get; set; }
		public ForwarderOffer ForwarderOffer { get; set; }

	}
}
