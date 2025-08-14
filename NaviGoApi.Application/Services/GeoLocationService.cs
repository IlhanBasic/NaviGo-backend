using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NaviGoApi.Application.Services
{
	public class GeoLocationService : IGeoLocationService
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<GeoLocationService> _logger;

		public GeoLocationService(HttpClient httpClient, ILogger<GeoLocationService> logger)
		{
			_httpClient = httpClient;
			_logger = logger;
		}

		public async Task<string?> GetRegionByIpAsync(string ipAddress)
		{
			if (string.IsNullOrWhiteSpace(ipAddress))
				return null;
			var ipOnly = ipAddress.Split(':')[0];
			string requestUri = $"http://ip-api.com/json/{ipOnly}?fields=status,countryCode,message";
			try
			{
				var response = await _httpClient.GetAsync(requestUri);
				if (!response.IsSuccessStatusCode)
				{
					_logger.LogWarning("GeoLocation API returned non-success status {StatusCode} for IP {Ip}", response.StatusCode, ipOnly);
					return null;
				}

				var content = await response.Content.ReadAsStringAsync();

				var result = JsonSerializer.Deserialize<IpApiResponse>(content, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				if (result == null || result.Status != "success")
				{
					_logger.LogWarning("GeoLocation API failed for IP {Ip}: {Message}", ipOnly, result?.Message);
					return null;
				}
				return result.CountryCode;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception while calling GeoLocation API for IP {Ip}", ipOnly);
				return null;
			}
		}
		private class IpApiResponse
		{
			public string? Status { get; set; }
			public string? CountryCode { get; set; }
			public string? Message { get; set; }
		}
	}
}
