using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Queries.VehicleType;
using NaviGoApi.Application.DTOs.VehicleType;
using NaviGoApi.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.VehicleType
{
	public class GetVehicleTypeByIdQueryHandler : IRequestHandler<GetVehicleTypeByIdQuery, VehicleTypeDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public GetVehicleTypeByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<VehicleTypeDto> Handle(GetVehicleTypeByIdQuery request, CancellationToken cancellationToken)
		{
			var vehicleType = await _unitOfWork.VehicleTypes.GetByIdAsync(request.Id);
			return _mapper.Map<VehicleTypeDto>(vehicleType);
		}
	}
}
