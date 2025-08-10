using MediatR;
using NaviGoApi.Application.CQRS.Commands.Company;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Company
{
	public class DeleteCompanyCommandHandler : IRequestHandler<DeleteCompanyCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;

		public DeleteCompanyCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<Unit> Handle(DeleteCompanyCommand request, CancellationToken cancellationToken)
		{
			var existing = await _unitOfWork.Companies.GetByIdAsync(request.Id);
			if (existing != null)
			{
				await _unitOfWork.Companies.DeleteAsync(existing);
				await _unitOfWork.SaveChangesAsync();
			}

			return Unit.Value;
		}
	}
}
