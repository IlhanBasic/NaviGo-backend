using MediatR;
using NaviGoApi.Application.DTOs.CargoType;

namespace NaviGoApi.Application.CQRS.Queries.CargoType
{
	public class GetCargoTypeByIdQuery : IRequest<CargoTypeDto>
	{
		public int Id { get; set; }

		public GetCargoTypeByIdQuery(int id)
		{
			Id = id;
		}
	}
}
