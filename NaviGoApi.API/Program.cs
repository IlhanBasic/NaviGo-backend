using KnjizaraApi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Infrastructure.Postgresql.Persistence;
using NaviGoApi.Infrastructure.Postgresql.Repositories;
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
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
