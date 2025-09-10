using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.Contract;
using NaviGoApi.Application.Services;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Contract
{
	public class AddClientContractCommandHandler : IRequestHandler<AddClientContractCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IEmailService _emailService;

		public AddClientContractCommandHandler(
			IMapper mapper,
			IUnitOfWork unitOfWork,
			IHttpContextAccessor httpContextAccessor,
			IEmailService emailService)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
			_emailService = emailService;
		}

		public async Task<Unit> Handle(AddClientContractCommand request, CancellationToken cancellationToken)
		{
			if (_httpContextAccessor.HttpContext == null)
				throw new ValidationException("HttpContext is not available.");

			var userEmail = _httpContextAccessor.HttpContext.User
				.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

			if (string.IsNullOrEmpty(userEmail))
				throw new ValidationException("User email not found in token.");

			var currentUser = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException("Authenticated user not found.");

			if (currentUser.UserRole != UserRole.RegularUser && currentUser.UserRole != UserRole.CompanyAdmin)
				throw new ValidationException("You do not have permission to create a contract.");

			Domain.Entities.Company? clientCompany = null;
			if (currentUser.CompanyId != null)
			{
				clientCompany = await _unitOfWork.Companies.GetByIdAsync(currentUser.CompanyId.Value);
				if (clientCompany == null)
					throw new ValidationException("Client company does not exist.");

				if (clientCompany.CompanyType != CompanyType.Client)
					throw new ValidationException("Only client companies can initiate contracts.");
			}

			var routePrice = await _unitOfWork.RoutePrices.GetByIdAsync(request.ContractDto.RoutePriceId)
				?? throw new ValidationException("Specified RoutePrice does not exist.");

			var route = await _unitOfWork.Routes.GetByIdAsync(routePrice.RouteId)
				?? throw new ValidationException("Route from Contract doesn't exist.");

			var forwarderOffer = await _unitOfWork.ForwarderOffers.GetByIdAsync(request.ContractDto.ForwarderOfferId)
				?? throw new ValidationException("Specified ForwarderOffer does not exist.");

			var forwarder = await _unitOfWork.Companies.GetByIdAsync(forwarderOffer.ForwarderId)
				?? throw new ValidationException("Specified Forwarder does not exist.");

			var contract = new Domain.Entities.Contract
			{
				ClientId = currentUser.Id,
				ForwarderId = forwarderOffer.ForwarderId,
				RouteId = routePrice.RouteId,
				RoutePriceId = routePrice.Id,
				ForwarderOfferId = forwarderOffer.Id,
				PenaltyRatePerHour = request.ContractDto.PenaltyRatePerHour,
				MaxPenaltyPercent = request.ContractDto.MaxPenaltyPercent,
				ContractStatus = ContractStatus.Pending,
				ContractNumber = $"CTR-{DateTime.UtcNow:yyyyMMddHHmmss}-{currentUser.Id}",
				ContractDate = DateTime.UtcNow,
				Terms = $@"
				The parties hereby agree to comply with all obligations under this contract, including timely delivery, proper handling of goods, adherence to applicable laws and regulations, and fulfillment of any agreed service standards. 
				Failure to meet these obligations may result in penalties as outlined in this contract."
		};

			await _unitOfWork.Contracts.AddAsync(contract);
			await _unitOfWork.SaveChangesAsync();

			// Dodavanje pošiljki
			if (request.ContractDto.Shipments != null && request.ContractDto.Shipments.Any())
			{
				foreach (var shipmentDto in request.ContractDto.Shipments)
				{
					var shipment = new Domain.Entities.Shipment
					{
						CargoTypeId = shipmentDto.CargoTypeId,
						WeightKg = shipmentDto.WeightKg,
						Priority = shipmentDto.Priority,
						Description = shipmentDto.Description,
						ScheduledDeparture = DateTime.SpecifyKind(shipmentDto.ScheduledDeparture, DateTimeKind.Utc),
						ScheduledArrival = DateTime.SpecifyKind(shipmentDto.ScheduledArrival, DateTimeKind.Utc),
						ContractId = contract.Id,
						Status = ShipmentStatus.Scheduled,
						CreatedAt = DateTime.UtcNow
					};

					await _unitOfWork.Shipments.AddAsync(shipment);
				}

				await _unitOfWork.SaveChangesAsync();
			}

			// Slanje mejla svim adminima carrier kompanije
			var carrierCompanyId = route.CompanyId;
			var allUsers = await _unitOfWork.Users.GetAllAsync();
			var targetAdmins = allUsers
				.Where(u => u.CompanyId == carrierCompanyId && u.UserRole == UserRole.CompanyAdmin)
				.ToList();

			var emailDto = new ContractNotificationDto
			{
				ContractNumber = contract.ContractNumber,
				ClientName = clientCompany != null ? clientCompany.CompanyName : $"{currentUser.FirstName} {currentUser.LastName}",
				ForwarderName = forwarder.CompanyName,
				PenaltyRatePerHour = (double)contract.PenaltyRatePerHour,
				MaxPenaltyPercent = (double)contract.MaxPenaltyPercent,
				ContractDate = contract.ContractDate
			};

			var emailTasks = targetAdmins.Select(admin =>
				_emailService.SendEmailContractCreatedNotification(admin.Email, emailDto));

			await Task.WhenAll(emailTasks);

			return Unit.Value;
		}
	}

	public class ContractNotificationDto
	{
		public string ContractNumber { get; set; } = null!;
		public string ClientName { get; set; } = null!;
		public string ForwarderName { get; set; } = null!;
		public double PenaltyRatePerHour { get; set; }
		public double MaxPenaltyPercent { get; set; }
		public DateTime ContractDate { get; set; }
	}
}
