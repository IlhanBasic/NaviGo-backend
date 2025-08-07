using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Queries.VehicleType;
using NaviGoApi.Application.DTOs.VehicleType;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.VehicleType
{
	public class GetAllVehicleTypeQueryHandler : IRequestHandler<GetAllVehicleTypeQuery, IEnumerable<VehicleTypeDto>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public GetAllVehicleTypeQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<IEnumerable<VehicleTypeDto>> Handle(GetAllVehicleTypeQuery request, CancellationToken cancellationToken)
		{
			var list = await _unitOfWork.VehicleTypes.GetAllAsync();
			return _mapper.Map<IEnumerable<VehicleTypeDto>>(list);
		}
	}
}
