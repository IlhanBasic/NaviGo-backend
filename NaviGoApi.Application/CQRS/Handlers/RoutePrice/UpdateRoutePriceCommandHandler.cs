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
	public class UpdateRoutePriceCommandHandler : IRequestHandler<UpdateRoutePriceCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public UpdateRoutePriceCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(UpdateRoutePriceCommand request, CancellationToken cancellationToken)
		{
			var existing = await _unitOfWork.RoutePrices.GetByIdAsync(request.Id);
			if (existing == null)
				throw new KeyNotFoundException($"RoutePrice with ID {request.Id} not found");

			_mapper.Map(request.RoutePriceDto, existing);

			await _unitOfWork.RoutePrices.UpdateAsync(existing);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}

}
