using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Queries.CargoType;
using NaviGoApi.Application.DTOs.CargoType;
using NaviGoApi.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.CargoType
{
	public class GetCargoTypeByIdQueryHandler : IRequestHandler<GetCargoTypeByIdQuery, CargoTypeDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public GetCargoTypeByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<CargoTypeDto> Handle(GetCargoTypeByIdQuery request, CancellationToken cancellationToken)
		{
			var cargoType = await _unitOfWork.CargoTypes.GetByIdAsync(request.Id);
			return _mapper.Map<CargoTypeDto>(cargoType);
		}
	}
}
