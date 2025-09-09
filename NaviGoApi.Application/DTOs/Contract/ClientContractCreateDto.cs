using NaviGoApi.Application.DTOs.Shipment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.Contract
{
	public class ClientContractCreateDto
	{
		public int RoutePriceId { get; set; }
		public int ForwarderOfferId { get; set; }

		public decimal PenaltyRatePerHour { get; set; }
		public decimal MaxPenaltyPercent { get; set; }
		public IEnumerable<ClientShipmentCreateDto> Shipments { get; set; }
	}
}
