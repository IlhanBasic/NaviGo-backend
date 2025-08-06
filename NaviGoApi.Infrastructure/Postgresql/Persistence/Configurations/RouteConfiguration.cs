using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Postgresql.Persistence.Configurations
{
	internal class RouteConfiguration : IEntityTypeConfiguration<Route>
	{
		public void Configure(EntityTypeBuilder<Route> builder)
		{

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id);

			builder.Property(x => x.CompanyId)
				.IsRequired();

			builder.Property(x => x.StartLocationId)
				.IsRequired();

			builder.Property(x => x.EndLocationId)
				.IsRequired();

			builder.Property(x => x.DistanceKm)
				.IsRequired();

			builder.Property(x => x.EstimatedDurationHours)
				.IsRequired();

			builder.Property(x => x.BasePrice)
				.IsRequired()
				.HasColumnType("decimal(18,2)");

			builder.Property(x => x.IsActive)
				.IsRequired();

			builder.Property(x => x.CreatedAt)
				.IsRequired();

			builder.Property(x => x.AvailableFrom)
				.IsRequired();

			builder.Property(x => x.AvailableTo)
				.IsRequired();

			// Veza Route - Company (M:1)
			builder.HasOne(x => x.Company)
				.WithMany(c => c.Routes)
				.HasForeignKey(x => x.CompanyId)
				.OnDelete(DeleteBehavior.Cascade);

			// Veza Route - StartLocation (M:1)
			builder.HasOne(x => x.StartLocation)
				.WithMany(l => l.RoutesStart)
				.HasForeignKey(x => x.StartLocationId)
				.OnDelete(DeleteBehavior.Restrict);

			// Veza Route - EndLocation (M:1)
			builder.HasOne(x => x.EndLocation)
				.WithMany(l => l.RoutesEnd)
				.HasForeignKey(x => x.EndLocationId)
				.OnDelete(DeleteBehavior.Restrict);

			// Veze sa kolekcijama - RoutePrices, ForwarderOffers, Contracts
			builder.HasMany(x => x.RoutePrices)
				.WithOne(rp => rp.Route)
				.HasForeignKey(rp => rp.RouteId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(x => x.ForwarderOffers)
				.WithOne(fo => fo.Route)
				.HasForeignKey(fo => fo.RouteId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(x => x.Contracts)
				.WithOne(c => c.Route)
				.HasForeignKey(c => c.RouteId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
