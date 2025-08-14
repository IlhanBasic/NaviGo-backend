
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

		public int ClientId { get; set; }         // FK na User (klijent)
		public int ForwarderId { get; set; }      // FK na Company (špediter)
		public int RouteId { get; set; }           // FK na Route

		public string ContractNumber { get; set; } = null!;
		public DateTime ContractDate { get; set; }
		public string Terms { get; set; } = null!;

		public ContractStatus ContractStatus { get; set; }
		public decimal PenaltyRatePerHour { get; set; }
		public decimal MaxPenaltyPercent { get; set; }

		//public DateTime ValidUntil { get; set; }
		public DateTime? SignedDate { get; set; }

		// Navigaciona svojstva
		public User? Client { get; set; }
		public Company? Forwarder { get; set; }
		public Route? Route { get; set; }

		public ICollection<Shipment>? Shipments { get; set; }
		public Payment? Payment { get; set; }  // 1:1 sa Payment
	}
}
