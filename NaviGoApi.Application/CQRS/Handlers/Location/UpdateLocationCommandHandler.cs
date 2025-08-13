using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.Location;
using NaviGoApi.Application.DTOs.Location;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Location
{
	public class UpdateLocationCommandHandler : IRequestHandler<UpdateLocationCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
        public UpdateLocationCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<Unit> Handle(UpdateLocationCommand request, CancellationToken cancellationToken)
		{
			var location = await _unitOfWork.Locations.GetByIdAsync(request.Id);
			if(location != null)
			{
				_mapper.Map(request.LocationDto, location);
				var exists = await _unitOfWork.Locations.GetByFullLocationAsync(location.ZIP, location.FullAddress, location.City);
				if (exists != null)
					throw new ValidationException("Location with same ZIP, Address and City is already created.");
				await _unitOfWork.Locations.UpdateAsync(location);
				await _unitOfWork.SaveChangesAsync();
			}

			return Unit.Value;
		}
	}
}
