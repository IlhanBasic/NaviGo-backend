using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.Company;
using NaviGoApi.Application.DTOs.Company;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
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
			var entity = _mapper.Map<Domain.Entities.Company>(request.CompanyDto);
			await _unitOfWork.Companies.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return _mapper.Map<CompanyDto>(entity);
		}
	}
}
