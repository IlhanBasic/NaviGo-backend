using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Services
{
	public interface IDelayPenaltyCalculationService
	{
		Task<DelayPenalty?> CalculateAndCreatePenaltyAsync(Shipment shipment);
	}


}
