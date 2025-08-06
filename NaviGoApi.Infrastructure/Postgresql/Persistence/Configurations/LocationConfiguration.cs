using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Postgresql.Persistence.Configurations
{
	internal class LocationConfiguration : IEntityTypeConfiguration<Location>
	{
		public void Configure(EntityTypeBuilder<Location> builder)
		{

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id);

			builder.Property(x => x.City)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(x => x.Country)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(x => x.ZIP)
				.IsRequired()
				.HasMaxLength(20);

			builder.Property(x => x.Latitude)
				.IsRequired();

			builder.Property(x => x.Longitude)
				.IsRequired();

			builder.Property(x => x.FullAddress)
				.IsRequired()
				.HasMaxLength(250);

			// Veze - navigaciona svojstva

			// RoutesStart (1 lokacija može biti početna za više ruta)
			builder.HasMany(x => x.RoutesStart)
				.WithOne(r => r.StartLocation)
				.HasForeignKey(r => r.StartLocationId)
				.OnDelete(DeleteBehavior.Restrict);

			// RoutesEnd (1 lokacija može biti krajnja za više ruta)
			builder.HasMany(x => x.RoutesEnd)
				.WithOne(r => r.EndLocation)
				.HasForeignKey(r => r.EndLocationId)
				.OnDelete(DeleteBehavior.Restrict);

			// VehiclesCurrentLocation (1 lokacija može biti trenutna lokacija za više vozila)
			builder.HasMany(x => x.VehiclesCurrentLocation)
				.WithOne(v => v.CurrentLocation)
				.HasForeignKey(v => v.CurrentLocationId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
