using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Postgresql.Persistence.Configurations
{
	internal class RoutePriceConfiguration : IEntityTypeConfiguration<RoutePrice>
	{
		public void Configure(EntityTypeBuilder<RoutePrice> builder)
		{

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id);

			builder.Property(x => x.RouteId)
				.IsRequired();

			builder.Property(x => x.VehicleTypeId)
				.IsRequired();

			builder.Property(x => x.PricePerKm)
				.IsRequired()
				.HasColumnType("decimal(18,2)");

			builder.Property(x => x.MinimumPrice)
				.IsRequired()
				.HasColumnType("decimal(18,2)");

			// Veza RoutePrice - Route (M:1)
			builder.HasOne(x => x.Route)
				.WithMany(r => r.RoutePrices)
				.HasForeignKey(x => x.RouteId)
				.OnDelete(DeleteBehavior.Cascade);

			// Veza RoutePrice - VehicleType (M:1)
			builder.HasOne(x => x.VehicleType)
				.WithMany(vt => vt.RoutePrices)
				.HasForeignKey(x => x.VehicleTypeId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
