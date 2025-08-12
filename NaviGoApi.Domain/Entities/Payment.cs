using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Entities
{
	public enum PaymentStatus
	{
		Pending = 0, 
		Verified = 1,  
		Rejected = 2  
	}
	public class Payment
	{
		public int Id { get; set; }

		public int ContractId { get; set; } 
		public decimal Amount { get; set; }
		public DateTime PaymentDate { get; set; }
		public PaymentStatus PaymentStatus { get; set; }
		public string? ReceiptUrl { get; set; }
		public int ClientId { get; set; }
		public decimal? PenaltyAmount { get; set; }
		public bool IsRefunded { get; set; } = false;
		public DateTime? RefundDate { get; set; } 

		// Navigaciona svojstva
		public Contract? Contract { get; set; }
		public User? Client { get; set; }
	}
}
