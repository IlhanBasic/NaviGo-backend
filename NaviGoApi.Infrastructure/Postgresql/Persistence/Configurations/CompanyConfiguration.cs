using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Postgresql.Persistence.Configurations
{
	internal class CompanyConfiguration : IEntityTypeConfiguration<Company>
	{
		public void Configure(EntityTypeBuilder<Company> builder)
		{
			builder.HasKey(c => c.Id);

			builder.Property(c => c.CompanyName)
				   .IsRequired()
				   .HasMaxLength(150);

			builder.Property(c => c.PIB)
				   .IsRequired()
				   .HasMaxLength(20);

			// Dodajemo unique constraint na PIB
			builder.HasIndex(c => c.PIB).IsUnique();

			builder.Property(c => c.Address)
				   .IsRequired()
				   .HasMaxLength(250);

			builder.Property(c => c.LogoUrl)
				   .HasMaxLength(300);

			builder.Property(c => c.ContactEmail)
				   .IsRequired()
				   .HasMaxLength(100);

			builder.Property(c => c.Website)
				   .HasMaxLength(200);

			builder.Property(c => c.Description)
				   .HasMaxLength(1000);

			builder.Property(c => c.CompanyType)
				   .IsRequired();

			builder.Property(c => c.CompanyStatus)
				   .IsRequired()
				   .HasDefaultValue(CompanyStatus.Pending);

			builder.Property(c => c.MaxCommissionRate)
				   .HasPrecision(5, 2); 

			builder.Property(c => c.SaldoAmount)
				   .HasPrecision(18, 2);

			builder.Property(c => c.CreatedAt)
				   .IsRequired();

			builder.Property(c => c.ProofFileUrl)
				   .HasMaxLength(1000);

			// Veze

			builder.HasMany(c => c.Users)
				   .WithOne(u => u.Company)
				   .HasForeignKey(u => u.CompanyId)
				   .OnDelete(DeleteBehavior.Restrict);

			builder.HasMany(c => c.Vehicles)
				   .WithOne(v => v.Company)
				   .HasForeignKey(v => v.CompanyId)
				   .OnDelete(DeleteBehavior.Restrict);

			builder.HasMany(c => c.Drivers)
				   .WithOne(d => d.Company)
				   .HasForeignKey(d => d.CompanyId)
				   .OnDelete(DeleteBehavior.Restrict);

			builder.HasMany(c => c.Routes)
				   .WithOne(r => r.Company)
				   .HasForeignKey(r => r.CompanyId)
				   .OnDelete(DeleteBehavior.Restrict);

			builder.HasMany(c => c.ForwarderOffers)
				   .WithOne(fo => fo.Forwarder)
				   .HasForeignKey(fo => fo.ForwarderId)
				   .OnDelete(DeleteBehavior.Restrict);
		}
	}
}
