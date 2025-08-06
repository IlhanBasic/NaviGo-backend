using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Postgresql.Persistence.Configurations
{
	public class DriverConfiguration : IEntityTypeConfiguration<Driver>
	{
		public void Configure(EntityTypeBuilder<Driver> builder)
		{
			builder.HasKey(d => d.Id);

			builder.Property(d => d.FirstName)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(d => d.LastName)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(d => d.PhoneNumber)
				.IsRequired()
				.HasMaxLength(20);

			builder.Property(d => d.LicenseNumber)
				.IsRequired()
				.HasMaxLength(50);

			builder.Property(d => d.LicenseExpiry)
				.HasColumnType("date");

			builder.Property(d => d.LicenseCategories)
				.HasMaxLength(100); // CSV string, možeš povećati ako treba

			builder.Property(d => d.HireDate)
				.IsRequired()
				.HasColumnType("date");

			builder.Property(d => d.DriverStatus)
				.HasConversion<int>()
				.IsRequired();

			builder.HasOne(d => d.Company)
				.WithMany(c => c.Drivers)
				.HasForeignKey(d => d.CompanyId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(d => d.Shipments)
				.WithOne(s => s.Driver)
				.HasForeignKey(s => s.DriverId)
				.OnDelete(DeleteBehavior.SetNull);
		}
	}
}
