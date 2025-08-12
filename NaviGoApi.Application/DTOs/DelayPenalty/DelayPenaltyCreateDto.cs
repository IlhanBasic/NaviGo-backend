using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.DelayPenalty
{
public class DelayPenaltyCreateDto
    {
        public int ShipmentId { get; set; }
        public int DelayHours { get; set; }
        public decimal PenaltyAmount { get; set; }
        public DelayPenaltyStatus DelayPenaltiesStatus { get; set; }  
    }
}
