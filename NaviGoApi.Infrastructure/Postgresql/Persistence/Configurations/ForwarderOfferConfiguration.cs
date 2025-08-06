using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Postgresql.Persistence.Configurations
{
	internal class ForwarderOfferConfiguration : IEntityTypeConfiguration<ForwarderOffer>
	{
		public void Configure(EntityTypeBuilder<ForwarderOffer> builder)
		{

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id);

			builder.Property(x => x.RouteId);

			builder.Property(x => x.ForwarderId);

			builder.Property(x => x.CommissionRate)
				.HasPrecision(5, 2);

			builder.Property(x => x.ForwarderOfferStatus);

			builder.Property(x => x.RejectionReason)
				.HasMaxLength(500);

			builder.Property(x => x.CreatedAt);

			builder.Property(x => x.ExpiresAt);

			builder.Property(x => x.DiscountRate)
				.HasPrecision(5, 2);

			builder.HasOne(x => x.Route)
				.WithMany(r => r.ForwarderOffers)
				.HasForeignKey(x => x.RouteId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(x => x.Forwarder)
				.WithMany(c => c.ForwarderOffers)
				.HasForeignKey(x => x.ForwarderId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
