using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Services
{
	public class GeoLocationService : IGeoLocationService
	{
		private readonly HttpClient _httpClient;

		public GeoLocationService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public async Task<string?> GetRegionByIpAsync(string ipAddress)
		{
			if (string.IsNullOrWhiteSpace(ipAddress))
				return null;

			// ip-api.com URL, vraća JSON sa informacijama o IP adresi
			string requestUri = $"http://ip-api.com/json/{ipAddress}?fields=status,countryCode,message";

			try
			{
				var response = await _httpClient.GetAsync(requestUri);
				if (!response.IsSuccessStatusCode)
					return null;

				var content = await response.Content.ReadAsStringAsync();

				var result = JsonSerializer.Deserialize<IpApiResponse>(content, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				if (result == null || result.Status != "success")
					return null;

				// Vraća ISO 3166-1 alpha-2 country code, npr. "RS", "US"
				return result.CountryCode;
			}
			catch
			{
				// Loguj po potrebi
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
