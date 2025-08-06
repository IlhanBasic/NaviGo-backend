using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Postgresql.Persistence.Configurations
{
	internal class VehicleTypeConfiguration : IEntityTypeConfiguration<VehicleType>
	{
		public void Configure(EntityTypeBuilder<VehicleType> builder)
		{

			builder.HasKey(vt => vt.Id);

			builder.Property(vt => vt.TypeName)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(vt => vt.Description)
				.HasMaxLength(500)
				.IsRequired(false);

			builder.Property(vt => vt.RequiresSpecialLicense)
				.IsRequired();

			// Veze

			builder.HasMany(vt => vt.Vehicles)
				.WithOne(v => v.VehicleType)
				.HasForeignKey(v => v.VehicleTypeId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasMany(vt => vt.RoutePrices)
				.WithOne(rp => rp.VehicleType)
				.HasForeignKey(rp => rp.VehicleTypeId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
