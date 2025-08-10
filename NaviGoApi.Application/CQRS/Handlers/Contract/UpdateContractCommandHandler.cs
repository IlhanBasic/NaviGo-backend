using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.Contract;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Contract
{
	public class UpdateContractCommandHandler : IRequestHandler<UpdateContractCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		public UpdateContractCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}
		public async Task<Unit> Handle(UpdateContractCommand request, CancellationToken cancellationToken)
		{
			var existingContract = await _unitOfWork.Contracts.GetByIdAsync(request.Id);
			if (existingContract == null)
			{
				throw new KeyNotFoundException($"Contract with ID {request.Id} not found.");
			}

			_mapper.Map(request.ContractDto, existingContract);

			await _unitOfWork.Contracts.UpdateAsync(existingContract);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}

	}
}
