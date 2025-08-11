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
	public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, CompanyDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public UpdateCompanyCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<CompanyDto> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
		{
			var existing = await _unitOfWork.Companies.GetByIdAsync(request.Id);
			if (existing == null)
			{
				return null!;
			}
			var dto = request.CompanyDto;
			if (dto.CompanyType != CompanyType.Forwarder &&
				(dto.MaxCommissionRate.HasValue || dto.SaldoAmount.HasValue))
			{
				throw new ValidationException("Only Forwarder companies can have MaxCommissionRate and SaldoAmount set.");
			}

			_mapper.Map(request.CompanyDto, existing);
			await _unitOfWork.Companies.UpdateAsync(existing);
			await _unitOfWork.SaveChangesAsync();

			return _mapper.Map<CompanyDto>(existing);
		}
	}
}
