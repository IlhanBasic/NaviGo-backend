using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Postgresql.Persistence.Configurations
{
	internal class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
	{
		public void Configure(EntityTypeBuilder<Shipment> builder)
		{

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id);

			builder.Property(x => x.ContractId)
				.IsRequired();

			builder.Property(x => x.VehicleId)
				.IsRequired();

			builder.Property(x => x.DriverId)
				.IsRequired();

			builder.Property(x => x.CargoTypeId)
				.IsRequired();

			builder.Property(x => x.WeightKg)
				.IsRequired();

			builder.Property(x => x.Priority)
				.IsRequired();

			builder.Property(x => x.Description)
				.HasMaxLength(1000)
				.IsUnicode()
				.IsRequired(false);

			builder.Property(x => x.Status)
				.IsRequired();

			builder.Property(x => x.ScheduledDeparture)
				.IsRequired();

			builder.Property(x => x.ScheduledArrival)
				.IsRequired();

			builder.Property(x => x.ActualDeparture)
				.IsRequired(false);

			builder.Property(x => x.ActualArrival)
				.IsRequired(false);

			builder.Property(x => x.CreatedAt)
				.IsRequired();

			builder.Property(x => x.DelayHours)
				.IsRequired(false);

			builder.Property(x => x.DelayPenaltyAmount)
				.HasColumnType("decimal(18,2)")
				.IsRequired(false);

			builder.Property(x => x.PenaltyCalculatedAt)
				.IsRequired(false);

			// Relacije

			// Shipment (M) : Contract (1)
			builder.HasOne(x => x.Contract)
				.WithMany(c => c.Shipments)
				.HasForeignKey(x => x.ContractId)
				.OnDelete(DeleteBehavior.Cascade);

			// Shipment (M) : Vehicle (1)
			builder.HasOne(x => x.Vehicle)
				.WithMany(v => v.Shipments)
				.HasForeignKey(x => x.VehicleId)
				.OnDelete(DeleteBehavior.Restrict);

			// Shipment (M) : Driver (1)
			builder.HasOne(x => x.Driver)
				.WithMany(d => d.Shipments)
				.HasForeignKey(x => x.DriverId)
				.OnDelete(DeleteBehavior.Restrict);

			// Shipment (M) : CargoType (1)
			builder.HasOne(x => x.CargoType)
				.WithMany(ct => ct.Shipments)
				.HasForeignKey(x => x.CargoTypeId)
				.OnDelete(DeleteBehavior.Restrict);

			// Shipment (1) : DelayPenalty (0..1)
			builder.HasOne(x => x.DelayPenalty)
				.WithOne(dp => dp.Shipment)
				.HasForeignKey<DelayPenalty>(dp => dp.ShipmentId)
				.OnDelete(DeleteBehavior.Cascade);

			// Shipment (1) : PickupChange (0..1)
			builder.HasOne(x => x.PickupChange)
				.WithOne(pc => pc.Shipment)
				.HasForeignKey<PickupChange>(pc => pc.ShipmentId)
				.OnDelete(DeleteBehavior.Cascade);

			// Kolekcije
			builder.HasMany(x => x.ShipmentDocuments)
				.WithOne(sd => sd.Shipment)
				.HasForeignKey(sd => sd.ShipmentId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(x => x.ShipmentStatusHistories)
				.WithOne(sh => sh.Shipment)
				.HasForeignKey(sh => sh.ShipmentId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
