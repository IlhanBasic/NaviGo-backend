using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.Contract
{
	public class CarrierContractStatusUpdateDto
	{
		public int? DriverId { get; set; }
		public int? VehicleId { get; set; }
		public ContractStatus ContractStatus { get; set; }
	}
}
