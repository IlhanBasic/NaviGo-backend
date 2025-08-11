using System.ComponentModel.DataAnnotations;

namespace NaviGoApi.API.Middlewares
{
	public class GlobalExceptionHandlerMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

		public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task Invoke(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (ValidationException vex)
			{
				context.Response.StatusCode = StatusCodes.Status400BadRequest;
				context.Response.ContentType = "application/json";

				var response = new { message = vex.Message };
				var json = System.Text.Json.JsonSerializer.Serialize(response);

				await context.Response.WriteAsync(json);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unhandled exception");

				context.Response.StatusCode = StatusCodes.Status500InternalServerError;
				context.Response.ContentType = "application/json";

				var response = new { message = "An unexpected error occurred." };
				var json = System.Text.Json.JsonSerializer.Serialize(response);

				await context.Response.WriteAsync(json);
			}
		}
	}

}
