using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Queries.Company;
using NaviGoApi.Application.DTOs.Company;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Company
{
	public class GetAllCompaniesQueryHandler : IRequestHandler<GetAllCompaniesQuery, IEnumerable<CompanyDto>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public GetAllCompaniesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<IEnumerable<CompanyDto>> Handle(GetAllCompaniesQuery request, CancellationToken cancellationToken)
		{
			var list = await _unitOfWork.Companies.GetAllAsync();
			return _mapper.Map<IEnumerable<CompanyDto>>(list);
		}
	}
}
