using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.Payment
{
	public class PaymentCreateDto
	{
		public int ContractId { get; set; }
		public decimal Amount { get; set; }
		public DateTime PaymentDate { get; set; }
		public int ClientId { get; set; }
		public string? ReceiptUrl { get; set; }
		// PaymentStatus postaviti na Pending da ne saljemo sa fronta,
	}
}
