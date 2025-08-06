using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Postgresql.Persistence.Configurations
{
	internal class ShipmentStatusHistoryConfiguration : IEntityTypeConfiguration<ShipmentStatusHistory>
	{
		public void Configure(EntityTypeBuilder<ShipmentStatusHistory> builder)
		{

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id);

			builder.Property(x => x.ShipmentId)
				.IsRequired();

			builder.Property(x => x.ShipmentStatus)
				.IsRequired();

			builder.Property(x => x.ChangedAt)
				.IsRequired();

			builder.Property(x => x.ChangedByUserId)
				.IsRequired();

			builder.Property(x => x.Notes)
				.HasMaxLength(1000)
				.IsRequired(false);

			// Relacije

			// ShipmentStatusHistory (M) : Shipment (1)
			builder.HasOne(x => x.Shipment)
				.WithMany(s => s.ShipmentStatusHistories)
				.HasForeignKey(x => x.ShipmentId)
				.OnDelete(DeleteBehavior.Cascade);

			// ShipmentStatusHistory (M) : User (1) (ChangedByUser)
			builder.HasOne(x => x.ChangedByUser)
				.WithMany()
				.HasForeignKey(x => x.ChangedByUserId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
