using AutoMapper;
using NaviGoApi.Application.DTOs.Location;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Services
{
	public class LocationService : ILocationService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public LocationService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<LocationDto> GetOrCreateAsync(LocationCreateDto dto)
		{
			var existing = await _unitOfWork.Locations.GetByFullLocationAsync(dto.ZIP, dto.FullAddress, dto.City);
			if (existing != null)
				return _mapper.Map<LocationDto>(existing); 
			var location = _mapper.Map<Location>(dto);
			await _unitOfWork.Locations.AddAsync(location);
			await _unitOfWork.SaveChangesAsync();
			var locationDto = _mapper.Map<LocationDto>(location);
			return locationDto;
		}


	}

}
