using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Queries.CargoType;
using NaviGoApi.Application.DTOs.CargoType;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.CargoType
{
	public class GetAllCargoTypeQueryHandler : IRequestHandler<GetAllCargoTypeQuery, IEnumerable<CargoTypeDto>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public GetAllCargoTypeQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<IEnumerable<CargoTypeDto>> Handle(GetAllCargoTypeQuery request, CancellationToken cancellationToken)
		{
			var list = await _unitOfWork.CargoTypes.GetAllAsync();
			return _mapper.Map<IEnumerable<CargoTypeDto>>(list);
		}
	}
}
