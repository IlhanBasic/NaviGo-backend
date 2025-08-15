using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using NaviGoApi.API.Middlewares;
using NaviGoApi.Application.CQRS.Queries.User;
using NaviGoApi.Application.MappingProfiles;
using NaviGoApi.Application.Services;
using NaviGoApi.Application.Settings;
using NaviGoApi.Application.Validators.Location;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Infrastructure.MongoDB.Repositories;
using NaviGoApi.Infrastructure.Neo4j.Repositories;
using NaviGoApi.Infrastructure.Postgresql.Persistence;
using NaviGoApi.Infrastructure.Postgresql.Repositories;
using Neo4j.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
//POSTGRESQL
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
//INTERFACES + REPOSITORIES FOR POSTGRESQL
//builder.Services.AddScoped<IUserRepository, UserRepository>();
//builder.Services.AddScoped<ICargoTypeRepository, CargoTypeRepository>();
//builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
//builder.Services.AddScoped<IDelayPenaltyRepository, DelayPenalityRepository>();
//builder.Services.AddScoped<IDriverRepository, DriverRepository>();
//builder.Services.AddScoped<IForwarderOfferRepository, ForwarderOfferRepository>();
//builder.Services.AddScoped<ILocationRepository, LocationRepository>();
//builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
//builder.Services.AddScoped<IPickupChangeRepository, PickupChangeRepository>();
//builder.Services.AddScoped<IRouteRepository, RouteRepository>();
//builder.Services.AddScoped<IRoutePriceRepository, RoutePriceRepository>();
//builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
//builder.Services.AddScoped<IShipmentDocumentRepository, ShipmentDocumentRepository>();
//builder.Services.AddScoped<IShipmentStatusHistoryRepository, ShipmentStatusHistoryRepository>();
//builder.Services.AddScoped<IVehicleMaintenanceRepository, VehicleMaintenanceRepository>();
//builder.Services.AddScoped<IVehicleTypeRepository, VehicleTypeRepository>();
//builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
//builder.Services.AddScoped<IContractRepository, ContractRepository>();
//builder.Services.AddScoped<IUserLocationRepository, UserLocationRepository>();
//MONGODB
builder.Services.AddSingleton<IMongoClient>(sp =>
{
	var config = sp.GetRequiredService<IConfiguration>();
	var connectionString = config["MongoDbSettings:ConnectionString"];
	return new MongoClient(connectionString);
});
//INTERFACES + REPOSITORIES FOR MONGODB
builder.Services.AddScoped<IUserRepository, UserMongoRepository>();
builder.Services.AddScoped<ICargoTypeRepository, CargoTypeMongoRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyMongoRepository>();
builder.Services.AddScoped<IDelayPenaltyRepository, DelayPenaltyMongoRepository>();
builder.Services.AddScoped<IDriverRepository, DriverMongoRepository>();
builder.Services.AddScoped<IForwarderOfferRepository, ForwarderOfferMongoRepository>();
builder.Services.AddScoped<ILocationRepository, LocationMongoRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentMongoRepository>();
builder.Services.AddScoped<IPickupChangeRepository, PickupChangeMongoRepository>();
builder.Services.AddScoped<IRouteRepository, RouteMongoRepository>();
builder.Services.AddScoped<IRoutePriceRepository, RoutePriceMongoRepository>();
builder.Services.AddScoped<IShipmentRepository, ShipmentMongoRepository>();
builder.Services.AddScoped<IShipmentDocumentRepository, ShipmentDocumentMongoRepository>();
builder.Services.AddScoped<IShipmentStatusHistoryRepository, ShipmentStatusHistoryMongoRepository>();
builder.Services.AddScoped<IVehicleMaintenanceRepository, VehicleMaintenanceMongoRepository>();
builder.Services.AddScoped<IVehicleTypeRepository, VehicleTypeMongoRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleMongoRepository>();
builder.Services.AddScoped<IContractRepository, ContractMongoRepository>();
builder.Services.AddScoped<IUserLocationRepository, UserLocationMongoRepository>();
builder.Services.AddScoped(sp =>
{
	var client = sp.GetRequiredService<IMongoClient>();
	var databaseName = sp.GetRequiredService<IConfiguration>()["MongoDbSettings:DatabaseName"];
	return client.GetDatabase(databaseName);
});
// Neo4j baza
builder.Services.AddSingleton<IDriver>(sp =>
{
	var config = sp.GetRequiredService<IConfiguration>();
	var uri = config["Neo4jSettings:Uri"]!;
	var user = config["Neo4jSettings:User"]!;
	var password = config["Neo4jSettings:Password"]!;
	return GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
});
//INTERFACES + REPOSITORIES FOR NEO4J
//builder.Services.AddScoped<IUserRepository, UserNeo4jRepository > ();
//builder.Services.AddScoped<ICargoTypeRepository, CargoTypeNeo4jRepository>();
//builder.Services.AddScoped<ICompanyRepository, CompanyNeo4jRepository>();
//builder.Services.AddScoped<IDelayPenaltyRepository, DelayPenaltyNeo4jRepository>();
//builder.Services.AddScoped<IDriverRepository, DriverNeo4jRepository>();
//builder.Services.AddScoped<IForwarderOfferRepository, ForwarderOfferNeo4jRepository>();
//builder.Services.AddScoped<ILocationRepository, LocationNeo4jRepository>();
//builder.Services.AddScoped<IPaymentRepository, PaymentNeo4jRepository>();
//builder.Services.AddScoped<IPickupChangeRepository, PickupChangeNeo4jRepository>();
//builder.Services.AddScoped<IRouteRepository, RouteNeo4jRepository>();
//builder.Services.AddScoped<IRoutePriceRepository, RoutePriceNeo4jRepository>();
//builder.Services.AddScoped<IShipmentRepository, ShipmentNeo4jRepository>();
//builder.Services.AddScoped<IShipmentDocumentRepository, ShipmentDocumentNeo4jRepository>();
//builder.Services.AddScoped<IShipmentStatusHistoryRepository, ShipmentStatusHistoryNeo4jRepository>();
//builder.Services.AddScoped<IVehicleMaintenanceRepository, VehicleMaintenanceNeo4jRepository>();
//builder.Services.AddScoped<IVehicleTypeRepository, VehicleTypeNeo4jRepository>();
//builder.Services.AddScoped<IVehicleRepository, VehicleNeo4jRepository>();
//builder.Services.AddScoped<IContractRepository, ContractNeo4jRepository>();
//builder.Services.AddScoped<IUserLocationRepository, UserLocationNeo4jRepository>();

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPaymentCalculatorService, PaymentCalculatorService>();
builder.Services.AddScoped<IDelayPenaltyCalculationService, DelayPenaltyCalculationService>();
// Add services to the container.
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddValidatorsFromAssemblyContaining<LocationCreateDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddScoped<IUploadFileService, UploadFileService>();
builder.Services.AddMediatR(typeof(GetAllUserQuery).Assembly);
builder.Services.AddHttpClient<IGeoLocationService, GeoLocationService>();
builder.Services.AddHttpClient();


//builder.Services.AddCors(options =>
//{
//	options.AddPolicy("AllowAll", policy =>
//	{
//		policy.AllowAnyOrigin()
//			  .AllowAnyMethod()
//			  .AllowAnyHeader();
//	});
//});
var frontendUrl = builder.Configuration["ApiSettings:FrontendUrl"];
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
	{
		policy.WithOrigins(frontendUrl)
			  .AllowAnyHeader()
			  .AllowAnyMethod()
			  .AllowCredentials();
	});
});

var jwtSecret = builder.Configuration["JWT_SECRET"];
//var key = Encoding.ASCII.GetBytes(jwtSecret!);
var key = Encoding.UTF8.GetBytes(jwtSecret!);
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.RequireHttpsMetadata = false;
	options.SaveToken = true;
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(key),
		ValidateIssuer = false,
		ValidateAudience = false,
		ClockSkew = TimeSpan.Zero
	};
})
.AddGoogle("Google", options =>
{
	options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
	options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
	options.SaveTokens = true;

	options.Events.OnCreatingTicket = ctx =>
	{
		// ovde možeš dodatno uhvatiti info iz Google korisnika
		return Task.CompletedTask;
	};
});

builder.Services.AddAuthorization();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	// Dodaj definiciju sigurnosnog šeme
	options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
	{
		Description = "Unesite JWT token ovde sa prefixom 'Bearer '",
		Name = "Authorization",
		In = Microsoft.OpenApi.Models.ParameterLocation.Header,
		Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
		Scheme = "Bearer"
	});

	// Dodaj zahtev za koriš?enje te sigurnosne šeme globalno u Swagger UI
	options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
	{
		{
			new Microsoft.OpenApi.Models.OpenApiSecurityScheme
			{
				Reference = new Microsoft.OpenApi.Models.OpenApiReference
				{
					Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
					Id = "Bearer"
				},
				Scheme = "oauth2",
				Name = "Bearer",
				In = Microsoft.OpenApi.Models.ParameterLocation.Header,
			},
			new List<string>()
		}
	});
});

var app = builder.Build();

//Middlewares
//app.UseMiddleware<GlobalExceptionHandlerMiddleware>();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<GeoLocationValidationMiddleware>();
app.UseMiddleware<SessionLockMiddleware>();
app.MapControllers();

app.Run();
