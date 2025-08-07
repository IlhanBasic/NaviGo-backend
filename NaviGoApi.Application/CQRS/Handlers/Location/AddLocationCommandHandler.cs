using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.Location;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Location
{
	public class AddLocationCommandHandler : IRequestHandler<AddLocationCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
        public AddLocationCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
			_mapper = mapper;
        }
        public async Task<Unit> Handle(AddLocationCommand request, CancellationToken cancellationToken)
		{
			var location = _mapper.Map<global::NaviGoApi.Domain.Entities.Location>(request.LocationDto);
			await _unitOfWork.Locations.AddAsync(location);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}
}
