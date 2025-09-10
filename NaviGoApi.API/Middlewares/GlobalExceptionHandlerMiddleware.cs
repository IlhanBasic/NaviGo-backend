using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Npgsql;
using MongoDB.Driver;
using Neo4j.Driver;

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
			catch (NpgsqlException pgEx)
			{
				_logger.LogError(pgEx, "PostgreSQL database error");
				context.Response.StatusCode = StatusCodes.Status500InternalServerError;
				context.Response.ContentType = "application/json";

				var response = new { message = "Database error (PostgreSQL): " + pgEx.Message };
				var json = System.Text.Json.JsonSerializer.Serialize(response);

				await context.Response.WriteAsync(json);
			}
			catch (MongoException mongoEx)
			{
				_logger.LogError(mongoEx, "MongoDB error");
				context.Response.StatusCode = StatusCodes.Status500InternalServerError;
				context.Response.ContentType = "application/json";

				var response = new { message = "Database error (MongoDB): " + mongoEx.Message };
				var json = System.Text.Json.JsonSerializer.Serialize(response);

				await context.Response.WriteAsync(json);
			}
			catch (Neo4jException neoEx)
			{
				_logger.LogError(neoEx, "Neo4j database error");
				context.Response.StatusCode = StatusCodes.Status500InternalServerError;
				context.Response.ContentType = "application/json";

				var response = new { message = "Database error (Neo4j): " + neoEx.Message };
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
