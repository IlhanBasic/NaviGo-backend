using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.CargoType;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.CargoType
{
	public class UpdateCargoTypeCommandHandler : IRequestHandler<UpdateCargoTypeCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public UpdateCargoTypeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(UpdateCargoTypeCommand request, CancellationToken cancellationToken)
		{
			var httpContext = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("HttpContext is not available.");

			var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
			if (string.IsNullOrWhiteSpace(userEmail))
				throw new ValidationException("User email not found in authentication token.");
			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException($"User with email '{userEmail}' not found.");
			if (user.UserRole != UserRole.SuperAdmin)
				throw new ValidationException("You are not allowed to update cargo type.");
			var typeName = request.CargoTypeDto.TypeName.Trim();
			var existing = await _unitOfWork.CargoTypes.GetByIdAsync(request.Id);
			var exists = await _unitOfWork.CargoTypes.GetByTypeName(typeName);
			if (exists != null && (existing !=null && existing.Id != exists.Id))
			{
				throw new ValidationException($"Cargo type with name '{typeName}' already exists.");
			}
			
			if (existing != null)
			{
				_mapper.Map(request.CargoTypeDto, existing);
				await _unitOfWork.CargoTypes.UpdateAsync(existing);
				await _unitOfWork.SaveChangesAsync();
			}

			return Unit.Value;
		}
	}
}
