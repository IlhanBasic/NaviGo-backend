using Microsoft.EntityFrameworkCore;
using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Persistence
{
	public class ApplicationDbContext:DbContext
	{
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options){}
		public DbSet<User> Users { get; set; }
		public DbSet<Company> Companies { get; set; }
		public DbSet<RefreshToken> RefreshTokens { get; set; }
		public DbSet<CargoType> CargoTypes { get; set; }
		public DbSet<Contract> Contracts { get; set; }
		public DbSet<DelayPenalty> DelayPenalties { get; set; }
		public DbSet<Driver> Drivers { get; set; }
		public DbSet<ForwarderOffer> ForwarderOffers { get; set; }
		public DbSet<Location> Locations { get; set; }
		public DbSet<Payment> Payments { get; set; }
		public DbSet<PickupChange> PickupChanges { get; set; }
		public DbSet<Route> Routes { get; set; }
		public DbSet<RoutePrice> RoutesPrices { get; set; }

		public DbSet<UserLocation> UserLocations { get; set; }
		public DbSet<Shipment> Shipments { get; set; }
		public DbSet<ShipmentDocument> ShipmentDocuments { get; set; }
		public DbSet<Vehicle> Vehicles { get; set; }
		public DbSet<VehicleType> VehicleTypes { get; set; }
		public DbSet<ShipmentStatusHistory> ShipmentStatusHistories { get; set; }
		public DbSet<VehicleMaintenance> VehicleMaintenances { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
		}

	}
}
