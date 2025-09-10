using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Queries.ShipmentStatusHistory;
using NaviGoApi.Application.DTOs.ShipmentStatusHistory;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

public class GetAllShipmentStatusHistoryQueryHandler : IRequestHandler<GetAllShipmentStatusHistoryQuery, IEnumerable<ShipmentStatusHistoryDto>>
{
	private readonly IMapper _mapper;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IHttpContextAccessor _httpContextAccessor;

	public GetAllShipmentStatusHistoryQueryHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
	{
		_mapper = mapper;
		_unitOfWork = unitOfWork;
		_httpContextAccessor = httpContextAccessor;
	}

	//public async Task<IEnumerable<ShipmentStatusHistoryDto>> Handle(GetAllShipmentStatusHistoryQuery request, CancellationToken cancellationToken)
	//{
	//	if (_httpContextAccessor.HttpContext == null)
	//		throw new ValidationException("HttpContext is null.");

	//	var userEmail = _httpContextAccessor.HttpContext.User
	//		.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

	//	if (string.IsNullOrEmpty(userEmail))
	//		throw new ValidationException("User email not found in token.");

	//	var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
	//		?? throw new ValidationException("User not found.");

	//	if (user.UserStatus != UserStatus.Active)
	//		throw new ValidationException("Your account is not activated.");

	//	// 1. Fetchuj sve ShipmentStatusHistory
	//	var allHistories = await _unitOfWork.ShipmentStatusHistories.GetAllAsync();

	//	// 2. Sakupi sve shipmentId-je
	//	var shipmentIds = allHistories.Select(h => h.ShipmentId).Distinct().ToList();
	//	var allShipments = await _unitOfWork.Shipments.GetAllAsync(); // fetch svih shipmenta
	//	var shipmentLookup = allShipments
	//		.Where(s => shipmentIds.Contains(s.Id))
	//		.ToDictionary(s => s.Id, s => s);

	//	// 3. Sakupi sve contractId-je iz shipment-a
	//	var contractIds = allShipments.Select(s => s.ContractId).Distinct().ToList();
	//	var allContracts = await _unitOfWork.Contracts.GetAllAsync();
	//	var contractLookup = allContracts
	//		.Where(c => contractIds.Contains(c.Id))
	//		.ToDictionary(c => c.Id, c => c);

	//	// 4. Sakupi sve rute koje su relevantne za Carrier
	//	var routeIds = allContracts.Select(c => c.RouteId).Distinct().ToList();
	//	var allRoutes = await _unitOfWork.Routes.GetAllAsync();
	//	var routeLookup = allRoutes
	//		.Where(r => routeIds.Contains(r.Id))
	//		.ToDictionary(r => r.Id, r => r);

	//	// 5. Filtriraj history po pravilu:
	//	var filteredHistories = new List<ShipmentStatusHistory>();

	//	foreach (var history in allHistories)
	//	{
	//		if (!shipmentLookup.TryGetValue(history.ShipmentId, out var shipment))
	//			continue;

	//		if (!contractLookup.TryGetValue(shipment.ContractId, out var contract))
	//			continue;

	//		bool canView = false;

	//		if (user.UserRole == UserRole.RegularUser)
	//		{
	//			// RegularUser vidi samo svoje shipment-e preko ugovora
	//			if (contract.ClientId == user.Id)
	//				canView = true;
	//		}
	//		else if (user.Company != null)
	//		{
	//			switch (user.Company.CompanyType)
	//			{
	//				case CompanyType.Carrier:
	//					if (routeLookup.TryGetValue(contract.RouteId, out var route))
	//					{
	//						if (route.CompanyId == user.CompanyId)
	//							canView = true;
	//					}
	//					break;

	//				case CompanyType.Forwarder:
	//					if (contract.ForwarderId == user.CompanyId)
	//						canView = true;
	//					break;

	//				case CompanyType.Client:
	//					if (contract.ClientId == user.Id)
	//						canView = true;
	//					break;
	//			}
	//		}

	//		if (canView)
	//			filteredHistories.Add(history);
	//	}

	//	return _mapper.Map<IEnumerable<ShipmentStatusHistoryDto>>(filteredHistories);
	//}
	public async Task<IEnumerable<ShipmentStatusHistoryDto>> Handle(GetAllShipmentStatusHistoryQuery request, CancellationToken cancellationToken)
	{
		if (_httpContextAccessor.HttpContext == null)
			throw new ValidationException("HttpContext is null.");

		var userEmail = _httpContextAccessor.HttpContext.User
			.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

		if (string.IsNullOrEmpty(userEmail))
			throw new ValidationException("User email not found in token.");

		var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
			?? throw new ValidationException("User not found.");

		if (user.UserStatus != UserStatus.Active)
			throw new ValidationException("Your account is not activated.");

		Company? userCompany = null;
		if (user.CompanyId.HasValue)
		{
			// Fetch kompanije jednom da izbegnemo N+1 problem
			userCompany = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);
			if (userCompany == null)
				throw new ValidationException("User must have a Company!");
		}

		// 1. Fetchuj sve ShipmentStatusHistory
		var allHistories = await _unitOfWork.ShipmentStatusHistories.GetAllAsync();

		// Debug log
		foreach (var h in allHistories)
		{
			Console.WriteLine($"Fetched ShipmentStatusHistory: Id={h.Id}, ShipmentId={h.ShipmentId}, Status={h.ShipmentStatus}, ChangedAt={h.ChangedAt}");
		}

		// 2. Fetchuj sve Shipment-e relevantne za ove istorije
		var shipmentIds = allHistories.Select(h => h.ShipmentId).Distinct().ToList();
		var allShipments = await _unitOfWork.Shipments.GetAllAsync();
		var shipmentLookup = allShipments
			.Where(s => shipmentIds.Contains(s.Id))
			.ToDictionary(s => s.Id, s => s);

		// 3. Fetchuj sve Contracts
		var contractIds = allShipments.Select(s => s.ContractId).Distinct().ToList();
		var allContracts = await _unitOfWork.Contracts.GetAllAsync();
		var contractLookup = allContracts
			.Where(c => contractIds.Contains(c.Id))
			.ToDictionary(c => c.Id, c => c);

		// 4. Fetchuj sve Routes
		var routeIds = allContracts.Select(c => c.RouteId).Distinct().ToList();
		var allRoutes = await _unitOfWork.Routes.GetAllAsync();
		var routeLookup = allRoutes
			.Where(r => routeIds.Contains(r.Id))
			.ToDictionary(r => r.Id, r => r);

		// 5. Filtriraj history
		var filteredHistories = new List<ShipmentStatusHistory>();

		foreach (var history in allHistories)
		{
			if (!shipmentLookup.TryGetValue(history.ShipmentId, out var shipment))
				continue;

			if (!contractLookup.TryGetValue(shipment.ContractId, out var contract))
				continue;

			bool canView = false;

			if (user.UserRole == UserRole.RegularUser)
			{
				if (contract.ClientId == user.Id)
					canView = true;
			}
			else if (userCompany != null)
			{
				switch (userCompany.CompanyType)
				{
					case CompanyType.Carrier:
						if (routeLookup.TryGetValue(contract.RouteId, out var route) &&
							route.CompanyId == userCompany.Id)
							canView = true;
						break;

					case CompanyType.Forwarder:
						if (contract.ForwarderId == userCompany.Id)
							canView = true;
						break;

					case CompanyType.Client:
						if (contract.ClientId == user.Id)
							canView = true;
						break;
				}
			}

			if (canView)
				filteredHistories.Add(history);
		}

		return _mapper.Map<IEnumerable<ShipmentStatusHistoryDto>>(filteredHistories);
	}

}
