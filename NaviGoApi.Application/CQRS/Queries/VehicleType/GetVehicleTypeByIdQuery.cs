using MediatR;
using NaviGoApi.Application.DTOs.VehicleType;

namespace NaviGoApi.Application.CQRS.Queries.VehicleType
{
	public class GetVehicleTypeByIdQuery : IRequest<VehicleTypeDto>
	{
		public int Id { get; set; }

		public GetVehicleTypeByIdQuery(int id)
		{
			Id = id;
		}
	}
}
