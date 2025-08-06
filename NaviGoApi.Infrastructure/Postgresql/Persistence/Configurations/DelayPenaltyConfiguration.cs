using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Postgresql.Persistence.Configurations
{
	internal class DelayPenaltyConfiguration : IEntityTypeConfiguration<DelayPenalty>
	{
		public void Configure(EntityTypeBuilder<DelayPenalty> builder)
		{

			builder.HasKey(dp => dp.Id);

			builder.Property(dp => dp.Id)
				.IsRequired();

			builder.Property(dp => dp.ShipmentId)
				.IsRequired();

			builder.Property(dp => dp.CalculatedAt)
				.IsRequired();

			builder.Property(dp => dp.DelayHours)
				.IsRequired();

			builder.Property(dp => dp.PenaltyAmount)
				.HasColumnType("decimal(10,2)")
				.IsRequired();

			builder.Property(dp => dp.DelayPenaltiesStatus)
				.HasConversion<int>()
				.IsRequired();

			// 1:1 veza sa Shipment
			builder.HasOne(dp => dp.Shipment)
				   .WithOne(s => s.DelayPenalty)
				   .HasForeignKey<DelayPenalty>(dp => dp.ShipmentId)
				   .OnDelete(DeleteBehavior.Cascade);
		}
	}
}
