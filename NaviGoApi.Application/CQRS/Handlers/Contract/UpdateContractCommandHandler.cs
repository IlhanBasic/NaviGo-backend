using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.Contract;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
			var dto = request.ContractDto;
			if (dto.SignedDate.HasValue &&
				(dto.SignedDate.Value > dto.ValidUntil))
				throw new ValidationException("Signed date must be between contract date and valid until.");
			if (dto.PenaltyRatePerHour < 0)
				throw new ValidationException("Penalty rate cannot be negative.");

			if (dto.MaxPenaltyPercent < 0 || dto.MaxPenaltyPercent > 100)
				throw new ValidationException("Max penalty percent must be between 0 and 100.");
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
