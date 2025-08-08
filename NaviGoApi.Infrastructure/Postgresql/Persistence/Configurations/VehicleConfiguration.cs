using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Postgresql.Persistence.Configurations
{
	internal class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
	{
		public void Configure(EntityTypeBuilder<Vehicle> builder)
		{

			builder.HasKey(v => v.Id);

			builder.Property(v => v.RegistrationNumber)
				.IsRequired()
				.HasMaxLength(50);

			builder.Property(v => v.CapacityKg)
				.IsRequired();

			builder.Property(v => v.ManufactureYear)
				.IsRequired();

			builder.Property(v => v.VehicleStatus)
				.IsRequired();

			builder.Property(v => v.LastInspectionDate)
				.IsRequired(false);

			builder.Property(v => v.InsuranceExpiry)
				.IsRequired(false);

			builder.Property(v => v.CurrentLocationId)
				.IsRequired(false);
			builder.Property(v => v.Brand)
				.IsRequired()
				.HasMaxLength(100);
			builder.Property(v => v.Model)
				.IsRequired()
				.HasMaxLength(100);
			builder.Property(v => v.EngineCapacityCc)
				.IsRequired();

			builder.Property(v => v.CreatedAt)
				.IsRequired();

			builder.Property(v => v.Categories)
				.HasMaxLength(100)
				.IsRequired(false);

			// Relations

			// Vehicle (M) : Company (1)
			builder.HasOne(v => v.Company)
				.WithMany(c => c.Vehicles)
				.HasForeignKey(v => v.CompanyId)
				.OnDelete(DeleteBehavior.Cascade);

			// Vehicle (M) : VehicleType (1)
			builder.HasOne(v => v.VehicleType)
				.WithMany(vt => vt.Vehicles)
				.HasForeignKey(v => v.VehicleTypeId)
				.OnDelete(DeleteBehavior.Restrict);

			// Vehicle (M) : Location (1) optional
			builder.HasOne(v => v.CurrentLocation)
				.WithMany(l => l.VehiclesCurrentLocation)
				.HasForeignKey(v => v.CurrentLocationId)
				.OnDelete(DeleteBehavior.SetNull);

			// Vehicle (1) : VehicleMaintenance (1:N)
			builder.HasMany(v => v.VehicleMaintenances)
				.WithOne(vm => vm.Vehicle)
				.HasForeignKey(vm => vm.VehicleId)
				.OnDelete(DeleteBehavior.Cascade);

			// Vehicle (1) : Shipment (1:N)
			builder.HasMany(v => v.Shipments)
				.WithOne(s => s.Vehicle)
				.HasForeignKey(s => s.VehicleId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
