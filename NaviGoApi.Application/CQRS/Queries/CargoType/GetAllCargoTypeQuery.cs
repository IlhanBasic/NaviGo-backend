using MediatR;
using NaviGoApi.Application.DTOs.CargoType;
using System.Collections.Generic;

namespace NaviGoApi.Application.CQRS.Queries.CargoType
{
	public class GetAllCargoTypeQuery : IRequest<IEnumerable<CargoTypeDto>> { }
}
