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
		Task<ShipmentDocument?> GetByIdAsync(Guid id);
		Task AddAsync(ShipmentDocument document);
		Task UpdateAsync(ShipmentDocument document);
		Task DeleteAsync(int id);
	}
}
