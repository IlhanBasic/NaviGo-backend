using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using NaviGoApi.Application.CQRS.Commands.Route;
using NaviGoApi.Domain.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Route
{
	public class UpdateRouteCommandHandler : IRequestHandler<UpdateRouteCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly HttpClient _httpClient;
		private readonly string _orsApiKey;
		public UpdateRouteCommandHandler(IMapper mapper, IUnitOfWork unitOfWork, IConfiguration config, HttpClient httpClient)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpClient = httpClient;
			_orsApiKey = config["OpenRouteService:ApiKey"];
		}

		public async Task<Unit> Handle(UpdateRouteCommand request, CancellationToken cancellationToken)
		{
			var existingRoute = await _unitOfWork.Routes.GetByIdAsync(request.Id);
			if (existingRoute == null)
				throw new KeyNotFoundException($"Route with ID {request.Id} not found");

			// Proveri da li su se lokacije promenile (ili želiš da osvežiš geometriju)
			bool locationsChanged = existingRoute.StartLocationId != request.RouteDto.StartLocationId
									|| existingRoute.EndLocationId != request.RouteDto.EndLocationId;

			if (locationsChanged)
			{
				var startLocation = await _unitOfWork.Locations.GetByIdAsync(request.RouteDto.StartLocationId);
				var endLocation = await _unitOfWork.Locations.GetByIdAsync(request.RouteDto.EndLocationId);

				if (startLocation == null || endLocation == null)
					throw new Exception("Start or End location not found.");

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
				{
					throw new Exception($"ORS API returned status {response.StatusCode}: {jsonString}");
				}

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

				existingRoute.DistanceKm = distanceMeters / 1000.0;
				existingRoute.EstimatedDurationHours = durationSeconds / 3600.0;
				existingRoute.GeometryEncoded = geometryEncoded;

				// Takođe ažuriraj StartLocationId i EndLocationId jer su se promenili
				existingRoute.StartLocationId = request.RouteDto.StartLocationId;
				existingRoute.EndLocationId = request.RouteDto.EndLocationId;
			}

			// Mapiraj ostala polja iz DTO (osim onih koje si ručno ažurirao)
			_mapper.Map(request.RouteDto, existingRoute);

			await _unitOfWork.Routes.UpdateAsync(existingRoute);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}

	}
}
