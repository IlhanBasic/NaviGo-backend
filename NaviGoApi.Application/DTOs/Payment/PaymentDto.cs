using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.Payment
{
	public class PaymentDto
	{
		public int Id { get; set; }
		public int ContractId { get; set; }
		public decimal Amount { get; set; }
		public DateTime PaymentDate { get; set; }
		public PaymentStatus PaymentStatus { get; set; }
		public string? ReceiptUrl { get; set; }
		public int ClientId { get; set; }
	}
}
