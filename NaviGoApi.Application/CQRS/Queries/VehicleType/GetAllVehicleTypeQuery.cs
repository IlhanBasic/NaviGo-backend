using MediatR;
using System.Collections.Generic;
using NaviGoApi.Application.DTOs.VehicleType;

namespace NaviGoApi.Application.CQRS.Queries.VehicleType
{
	public class GetAllVehicleTypeQuery : IRequest<IEnumerable<VehicleTypeDto>>
	{
	}
}
