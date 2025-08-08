using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.RoutePrice;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.RoutePrice
{
	public class AddRoutePriceCommandHandler : IRequestHandler<AddRoutePriceCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public AddRoutePriceCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(AddRoutePriceCommand request, CancellationToken cancellationToken)
		{
			var entity = _mapper.Map<Domain.Entities.RoutePrice>(request.RoutePriceDto);
			await _unitOfWork.RoutePrices.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}

}
