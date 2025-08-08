using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Queries.Contract;
using NaviGoApi.Application.DTOs.Contract;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Contract
{
	internal class GetContractByIdQueryHandler:IRequestHandler<GetContractByIdQuery,ContractDto?>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		public GetContractByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<ContractDto?> Handle(GetContractByIdQuery request, CancellationToken cancellationToken)
		{
			var contract = await _unitOfWork.Contracts.GetByIdAsync(request.Id);
			if (contract == null)
			{
				return null;
			}

			return _mapper.Map<ContractDto>(contract);
		}

	}
}
