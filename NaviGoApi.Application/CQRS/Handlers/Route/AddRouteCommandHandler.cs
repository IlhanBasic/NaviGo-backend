using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.Route;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Route
{
	using System.ComponentModel.DataAnnotations;
	using System.Net.Http;
	using System.Net.Http.Json;
	using System.Security.Claims;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.Configuration;
	using NaviGoApi.Domain.Entities;
	using Newtonsoft.Json.Linq;

	public class AddRouteCommandHandler : IRequestHandler<AddRouteCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly HttpClient _httpClient;
		private readonly string _orsApiKey;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public AddRouteCommandHandler(IMapper mapper, IUnitOfWork unitOfWork, IConfiguration config, HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpClient = httpClient;
			_orsApiKey = config["OpenRouteService:ApiKey"];
			_httpContextAccessor= httpContextAccessor;
		}
		public async Task<Unit> Handle(AddRouteCommand request, CancellationToken cancellationToken)
		{
			var httpContext = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("HttpContext is not available.");

			var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
			if (string.IsNullOrWhiteSpace(userEmail))
				throw new ValidationException("User email not found in authentication token.");
			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException($"User with email '{userEmail}' not found.");
			if (user.UserStatus != UserStatus.Active)
				throw new ValidationException("Your account is not activated.");
			if (user.UserRole != UserRole.CompanyAdmin)
				throw new ValidationException("You are not allowed to add route.");
			var companyExists = await _unitOfWork.Companies.GetByIdAsync(request.RouteDto.CompanyId);
			if (companyExists == null)
				throw new ValidationException($"Company with Id {request.RouteDto.CompanyId} does not exist.");
			if (companyExists.CompanyStatus == Domain.Entities.CompanyStatus.Pending)
				throw new ValidationException("Pending company cannot create route.");
			if (companyExists.CompanyType != Domain.Entities.CompanyType.Carrier)
				throw new ValidationException($"This company doesn't have right to adding routes.");
			var startLocation = await _unitOfWork.Locations.GetByIdAsync(request.RouteDto.StartLocationId);
			var endLocation = await _unitOfWork.Locations.GetByIdAsync(request.RouteDto.EndLocationId);
			if (startLocation == null || endLocation == null)
				throw new ValidationException("Start or End location not found.");
			if (request.RouteDto.StartLocationId == request.RouteDto.EndLocationId)
				throw new ValidationException("Start and End locations cannot be the same.");
			if (request.RouteDto.AvailableFrom >= request.RouteDto.AvailableTo)
				throw new ValidationException("AvailableFrom must be earlier than AvailableTo.");
			var existingRoute = await _unitOfWork.Routes.DuplicateRoute(
				request.RouteDto.CompanyId,
				request.RouteDto.StartLocationId,
				request.RouteDto.EndLocationId
			);
			if (existingRoute)
				throw new ValidationException("A route with the same start and end locations already exists for this company.");
			var route = _mapper.Map<Domain.Entities.Route>(request.RouteDto);
			var requestBody = new
			{
				coordinates = new[]
				{
			new[] { startLocation.Longitude, startLocation.Latitude },
			new[] { endLocation.Longitude, endLocation.Latitude }
		}
			};

			var requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
			var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.openrouteservice.org/v2/directions/driving-car/json");
			requestMessage.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");
			requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _orsApiKey);

			var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
			var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
				throw new Exception($"ORS API returned status {response.StatusCode}: {jsonString}");

			var json = JObject.Parse(jsonString);

			if (json["routes"] == null || !json["routes"].Any())
				throw new Exception("No routes found in the response from ORS API.");

			var distanceToken = json["routes"][0]?["summary"]?["distance"];
			var durationToken = json["routes"][0]?["summary"]?["duration"];
			var geometryToken = json["routes"][0]?["geometry"];

			if (distanceToken == null || durationToken == null || geometryToken == null)
				throw new Exception("Distance, duration or geometry information missing in ORS API response.");

			double distanceMeters = distanceToken.Value<double>();
			double durationSeconds = durationToken.Value<double>();
			string geometryEncoded = geometryToken.Value<string>();

			route.DistanceKm = distanceMeters / 1000.0;
			route.EstimatedDurationHours = durationSeconds / 3600.0;
			route.GeometryEncoded = geometryEncoded;
			route.CreatedAt = DateTime.UtcNow;

			await _unitOfWork.Routes.AddAsync(route);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}

	}

}
