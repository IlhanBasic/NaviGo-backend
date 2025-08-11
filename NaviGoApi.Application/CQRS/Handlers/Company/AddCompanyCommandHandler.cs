using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.Company;
using NaviGoApi.Application.DTOs.Company;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Company
{
	public class AddCompanyCommandHandler : IRequestHandler<AddCompanyCommand, CompanyDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public AddCompanyCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<CompanyDto> Handle(AddCompanyCommand request, CancellationToken cancellationToken)
		{
			var dto = request.CompanyDto;
			if (dto.CompanyType != CompanyType.Forwarder && dto.MaxCommissionRate.HasValue)
			{
				throw new ValidationException("Only Forwarder companies can have MaxCommissionRate set.");
			}
			var exists = _unitOfWork.Companies.GetByPibAsync(request.CompanyDto.PIB);
			if (exists != null)
				throw new ValidationException($"Company with PIB: ${request.CompanyDto.PIB} already exists.");

			var entity = _mapper.Map<Domain.Entities.Company>(request.CompanyDto);
			await _unitOfWork.Companies.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return _mapper.Map<CompanyDto>(entity);
		}
	}
}
