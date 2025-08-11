//using AutoMapper;
//using MediatR;
//using Microsoft.Extensions.Configuration;
//using NaviGoApi.Application.CQRS.Commands.Route;
//using NaviGoApi.Domain.Interfaces;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.Linq;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;

//namespace NaviGoApi.Application.CQRS.Handlers.Route
//{
//	public class UpdateRouteCommandHandler : IRequestHandler<UpdateRouteCommand, Unit>
//	{
//		private readonly IMapper _mapper;
//		private readonly IUnitOfWork _unitOfWork;
//		private readonly HttpClient _httpClient;
//		private readonly string _orsApiKey;
//		public UpdateRouteCommandHandler(IMapper mapper, IUnitOfWork unitOfWork, IConfiguration config, HttpClient httpClient)
//		{
//			_mapper = mapper;
//			_unitOfWork = unitOfWork;
//			_httpClient = httpClient;
//			_orsApiKey = config["OpenRouteService:ApiKey"];
//		}

//		public async Task<Unit> Handle(UpdateRouteCommand request, CancellationToken cancellationToken)
//		{
//			var existingRoute = await _unitOfWork.Routes.GetByIdAsync(request.Id);
//			if (existingRoute == null)
//				throw new ValidationException($"Route with ID {request.Id} not found");

//			var companyExists = await _unitOfWork.Companies.GetByIdAsync(request.RouteDto.CompanyId);
//			if (companyExists == null)
//				throw new ValidationException($"Company with Id {request.RouteDto.CompanyId} does not exist.");

//			var startLocation = await _unitOfWork.Locations.GetByIdAsync(request.RouteDto.StartLocationId);
//			var endLocation = await _unitOfWork.Locations.GetByIdAsync(request.RouteDto.EndLocationId);

//			if (startLocation == null || endLocation == null)
//				throw new ValidationException("Start or End location not found.");

//			if (request.RouteDto.StartLocationId == request.RouteDto.EndLocationId)
//				throw new ValidationException("Start and End locations cannot be the same.");
//			if (request.RouteDto.AvailableFrom >= request.RouteDto.AvailableTo)
//				throw new ValidationException("AvailableFrom must be earlier than AvailableTo.");

//			if (request.RouteDto.BasePrice < 0)
//				throw new ValidationException("Base price cannot be negative.");

//			var exists = await _unitOfWork.Routes.ExistsAsync(r =>
//				r.CompanyId == request.Id &&
//				r.StartLocationId == request.RouteDto.StartLocationId &&
//				r.EndLocationId == request.RouteDto.EndLocationId);

//			if (exists != null)
//				throw new ValidationException("A route with the same start and end locations already exists for this company.");

//			bool locationsChanged = existingRoute.StartLocationId != request.RouteDto.StartLocationId
//									|| existingRoute.EndLocationId != request.RouteDto.EndLocationId;

//			if (locationsChanged)
//			{
//				var startLocation = await _unitOfWork.Locations.GetByIdAsync(request.RouteDto.StartLocationId);
//				var endLocation = await _unitOfWork.Locations.GetByIdAsync(request.RouteDto.EndLocationId);

//				if (startLocation == null || endLocation == null)
//					throw new Exception("Start or End location not found.");

//				var requestBody = new
//				{
//					coordinates = new[]
//					{
//				new[] { startLocation.Longitude, startLocation.Latitude },
//				new[] { endLocation.Longitude, endLocation.Latitude }
//			}
//				};

//				var requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);

//				var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.openrouteservice.org/v2/directions/driving-car/json");
//				requestMessage.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");
//				requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _orsApiKey);

//				var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
//				var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);

//				if (!response.IsSuccessStatusCode)
//				{
//					throw new Exception($"ORS API returned status {response.StatusCode}: {jsonString}");
//				}

//				var json = JObject.Parse(jsonString);

//				if (json["routes"] == null || !json["routes"].Any())
//					throw new Exception("No routes found in the response from ORS API.");

//				var distanceToken = json["routes"][0]?["summary"]?["distance"];
//				var durationToken = json["routes"][0]?["summary"]?["duration"];
//				var geometryToken = json["routes"][0]?["geometry"];

//				if (distanceToken == null || durationToken == null || geometryToken == null)
//					throw new Exception("Distance, duration or geometry information missing in ORS API response.");

//				double distanceMeters = distanceToken.Value<double>();
//				double durationSeconds = durationToken.Value<double>();
//				string geometryEncoded = geometryToken.Value<string>();

//				existingRoute.DistanceKm = distanceMeters / 1000.0;
//				existingRoute.EstimatedDurationHours = durationSeconds / 3600.0;
//				existingRoute.GeometryEncoded = geometryEncoded;

//				// Takođe ažuriraj StartLocationId i EndLocationId jer su se promenili
//				existingRoute.StartLocationId = request.RouteDto.StartLocationId;
//				existingRoute.EndLocationId = request.RouteDto.EndLocationId;
//			}

//			// Mapiraj ostala polja iz DTO (osim onih koje si ručno ažurirao)
//			_mapper.Map(request.RouteDto, existingRoute);

//			await _unitOfWork.Routes.UpdateAsync(existingRoute);
//			await _unitOfWork.SaveChangesAsync();

//			return Unit.Value;
//		}

//	}
//}
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using NaviGoApi.Application.CQRS.Commands.Route;
using NaviGoApi.Domain.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
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
				throw new ValidationException($"Route with ID {request.Id} not found.");

			var company = await _unitOfWork.Companies.GetByIdAsync(existingRoute.CompanyId);
			if (company == null)
				throw new ValidationException($"Company with ID {existingRoute.CompanyId} does not exist.");
			if (company.CompanyType != Domain.Entities.CompanyType.Carrier)
				throw new ValidationException("This company doesn't have right to change this route.");
			if (request.RouteDto.StartLocationId == request.RouteDto.EndLocationId)
				throw new ValidationException("Start and End locations cannot be the same.");

			if (request.RouteDto.AvailableFrom >= request.RouteDto.AvailableTo)
				throw new ValidationException("AvailableFrom must be earlier than AvailableTo.");

			if (request.RouteDto.BasePrice < 0)
				throw new ValidationException("Base price cannot be negative.");

			bool exists = await _unitOfWork.Routes.ExistsAsync(r =>
				r.Id != request.Id &&
				r.CompanyId == existingRoute.CompanyId &&
				r.StartLocationId == request.RouteDto.StartLocationId &&
				r.EndLocationId == request.RouteDto.EndLocationId);

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
