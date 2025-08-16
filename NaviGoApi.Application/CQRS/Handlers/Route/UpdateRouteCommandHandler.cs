using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NaviGoApi.Application.CQRS.Commands.Route;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Route
{
	public class UpdateRouteCommandHandler : IRequestHandler<UpdateRouteCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly HttpClient _httpClient;
		private readonly string _orsApiKey;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public UpdateRouteCommandHandler(IMapper mapper, IUnitOfWork unitOfWork, IConfiguration config, HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpClient = httpClient;
			_orsApiKey = config["OpenRouteService:ApiKey"];
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(UpdateRouteCommand request, CancellationToken cancellationToken)
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
			if (user.UserRole == UserRole.RegularUser)
				throw new ValidationException("You are not allowed to add company.");
			var existingRoute = await _unitOfWork.Routes.GetByIdAsync(request.Id);
			if (existingRoute == null)
				throw new ValidationException($"Route with ID {request.Id} not found.");
			var company = await _unitOfWork.Companies.GetByIdAsync(existingRoute.CompanyId);
			if (company == null)
				throw new ValidationException($"Company with ID {existingRoute.CompanyId} does not exist.");
			if (user.CompanyId != company.Id)
				throw new ValidationException("You work in another company.");
			if (existingRoute.CompanyId != company.Id)
				throw new ValidationException("This company didn't create this route.");
			if (company.CompanyType != Domain.Entities.CompanyType.Carrier)
				throw new ValidationException("This company doesn't have right to change this route.");
			if (request.RouteDto.StartLocationId == request.RouteDto.EndLocationId)
				throw new ValidationException("Start and End locations cannot be the same.");
			if (request.RouteDto.AvailableFrom >= request.RouteDto.AvailableTo)
				throw new ValidationException("AvailableFrom must be earlier than AvailableTo.");
			var exists = await _unitOfWork.Routes.DuplicateRouteUpdate(
				company.Id,
				request.RouteDto.StartLocationId,
				request.RouteDto.EndLocationId,
				request.Id
			);

			if (exists)
				throw new ValidationException("A route with the same start and end locations already exists for this company.");


			bool locationsChanged = existingRoute.StartLocationId != request.RouteDto.StartLocationId
									|| existingRoute.EndLocationId != request.RouteDto.EndLocationId;

			if (locationsChanged)
			{
				var startLocation = await _unitOfWork.Locations.GetByIdAsync(request.RouteDto.StartLocationId);
				var endLocation = await _unitOfWork.Locations.GetByIdAsync(request.RouteDto.EndLocationId);

				if (startLocation == null || endLocation == null)
					throw new ValidationException("Start or End location not found.");

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

				existingRoute.DistanceKm = distanceToken.Value<double>() / 1000.0;
				existingRoute.EstimatedDurationHours = durationToken.Value<double>() / 3600.0;
				existingRoute.GeometryEncoded = geometryToken.Value<string>();

				existingRoute.StartLocationId = request.RouteDto.StartLocationId;
				existingRoute.EndLocationId = request.RouteDto.EndLocationId;
			}

			// Mapiraj ostala polja osim onih koje smo već ručno ažurirali
			_mapper.Map(request.RouteDto, existingRoute);

			await _unitOfWork.Routes.UpdateAsync(existingRoute);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}
}
