using NaviGoApi.Common.DTOs;
using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Interfaces
{
	public interface IShipmentDocumentRepository
	{
		Task<IEnumerable<ShipmentDocument>> GetAllAsync();
		Task<IEnumerable<ShipmentDocument>> GetAllAsync(ShipmentDocumentSearchDto shipmentDocumentSearch);

		Task<ShipmentDocument?> GetByIdAsync(int id);
		Task AddAsync(ShipmentDocument document);
		Task UpdateAsync(ShipmentDocument document);
		Task DeleteAsync(int id);
	}
}
