using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.Driver;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Driver
{
	public class AddDriverCommandHandler : IRequestHandler<AddDriverCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public AddDriverCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<Unit> Handle(AddDriverCommand request, CancellationToken cancellationToken)
		{
			var companyExists = await _unitOfWork.Companies.GetByIdAsync(request.DriverDto.CompanyId);
			if (companyExists == null)
				throw new ValidationException($"Company with ID {request.DriverDto.CompanyId} does not exist.");
			var driver = _mapper.Map<Domain.Entities.Driver>(request.DriverDto);
			await _unitOfWork.Drivers.AddAsync(driver);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}
}
