using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Postgresql.Persistence.Configurations
{
	internal class CargoTypeConfiguration : IEntityTypeConfiguration<CargoType>
	{
		public void Configure(EntityTypeBuilder<CargoType> builder)
		{

			builder.HasKey(ct => ct.Id);

			builder.Property(ct => ct.TypeName)
				   .IsRequired()
				   .HasMaxLength(100);

			builder.Property(ct => ct.Description)
				   .HasMaxLength(500);

			builder.Property(ct => ct.HazardLevel)
				   .IsRequired();

			builder.Property(ct => ct.RequiresSpecialEquipment)
				   .IsRequired();

			// Veza 1:N sa Shipment
			builder.HasMany(ct => ct.Shipments)
				   .WithOne(s => s.CargoType)
				   .HasForeignKey(s => s.CargoTypeId)
				   .OnDelete(DeleteBehavior.Restrict);  // ili .Cascade ako želiš kaskadno brisanje
		}
	}
}
