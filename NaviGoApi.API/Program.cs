using FluentValidation;
using FluentValidation.AspNetCore;
using KnjizaraApi.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NaviGoApi.Application.CQRS.Handlers.User;
using NaviGoApi.Application.CQRS.Queries.User;
using NaviGoApi.Application.MappingProfiles;
using NaviGoApi.Application.Services;
using NaviGoApi.Application.Settings;
using NaviGoApi.Application.Validators.Location;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Infrastructure.Postgresql.Persistence;
using NaviGoApi.Infrastructure.Postgresql.Repositories;
using System.Text;
//POSTGRESQL
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
//INTERFACES + REPOSITORIES FOR POSTGRESQL
builder.Services.AddScoped<IUserRepository,UserRepository>();
builder.Services.AddScoped<ICargoTypeRepository,CargoTypeRepository>();
builder.Services.AddScoped<ICompanyRepository,CompanyRepository>();
builder.Services.AddScoped<IDelayPenaltyRepository,DelayPenalityRepository>();
builder.Services.AddScoped<IDriverRepository,DriverRepository>();
builder.Services.AddScoped<IForwarderOfferRepository,ForwarderOfferRepository>();
builder.Services.AddScoped<ILocationRepository,LocationRepository>();
builder.Services.AddScoped<IPaymentRepository,PaymentRepository>();
builder.Services.AddScoped<IPickupChangeRepository,PickupChangeRepository>();
builder.Services.AddScoped<IRouteRepository,RouteRepository>();
builder.Services.AddScoped<IRoutePriceRepository,RoutePriceRepository>();
builder.Services.AddScoped<IShipmentRepository,ShipmentRepository>();
builder.Services.AddScoped<IShipmentDocumentRepository,ShipmentDocumentRepository>();
builder.Services.AddScoped<IShipmentStatusHistoryRepository,ShipmentStatusHistoryRepository>();
builder.Services.AddScoped<IVehicleMaintenanceRepository,VehicleMaintenanceRepository>();
builder.Services.AddScoped<IVehicleTypeRepository, VehicleTypeRepository>();
builder.Services.AddScoped<IVehicleRepository,VehicleRepository>();
builder.Services.AddScoped<IContractRepository,ContractRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
// Add services to the container.
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddControllersWithViews();

builder.Services.AddValidatorsFromAssemblyContaining<LocationCreateDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddScoped<IUploadFileService, UploadFileService>();
builder.Services.AddMediatR(typeof(GetAllUserQuery).Assembly);
var jwtSecret = builder.Configuration["JWT_SECRET"];
var key = Encoding.ASCII.GetBytes(jwtSecret!);

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
		// ovde mo�e� dodatno uhvatiti info iz Google korisnika
		return Task.CompletedTask;
	};
});
builder.Services.AddAuthorization();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	// Dodaj definiciju sigurnosnog �eme
	options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
	{
		Description = "Unesite JWT token ovde sa prefixom 'Bearer '",
		Name = "Authorization",
		In = Microsoft.OpenApi.Models.ParameterLocation.Header,
		Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
		Scheme = "Bearer"
	});

	// Dodaj zahtev za kori�?enje te sigurnosne �eme globalno u Swagger UI
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
