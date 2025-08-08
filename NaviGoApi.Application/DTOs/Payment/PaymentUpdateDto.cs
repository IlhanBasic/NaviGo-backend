using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.Payment
{
	public class PaymentUpdateDto
	{
		public decimal Amount { get; set; }
		public DateTime PaymentDate { get; set; }
		public PaymentStatus PaymentStatus { get; set; }
		public string? ReceiptUrl { get; set; }
	}
}
