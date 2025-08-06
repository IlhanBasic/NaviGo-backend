using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Postgresql.Persistence.Configurations
{
	internal class PickupChangeConfiguration : IEntityTypeConfiguration<PickupChange>
	{
		public void Configure(EntityTypeBuilder<PickupChange> builder)
		{

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id);

			builder.Property(x => x.ShipmentId)
				.IsRequired();

			builder.Property(x => x.ClientId)
				.IsRequired();

			builder.Property(x => x.OldTime)
				.IsRequired();

			builder.Property(x => x.NewTime)
				.IsRequired();

			builder.Property(x => x.ChangeCount)
				.IsRequired();

			builder.Property(x => x.AdditionalFee)
				.IsRequired()
				.HasColumnType("decimal(18,2)");

			builder.Property(x => x.PickupChangesStatus)
				.IsRequired();

			// Relations
			builder.HasOne(x => x.Shipment)
				.WithOne(s => s.PickupChange)
				.HasForeignKey<PickupChange>(x => x.ShipmentId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(x => x.Client)
				.WithMany()
				.HasForeignKey(x => x.ClientId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
