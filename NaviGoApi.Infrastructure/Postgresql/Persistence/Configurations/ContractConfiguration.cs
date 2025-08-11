using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Postgresql.Persistence.Configurations
{
	internal class ContractConfiguration : IEntityTypeConfiguration<Contract>
	{
		public void Configure(EntityTypeBuilder<Contract> builder)
		{

			builder.HasKey(c => c.Id);

			builder.Property(c => c.ContractNumber)
				.IsRequired()
				.HasMaxLength(50);
			builder.HasIndex(c => c.ContractNumber)
				.IsUnique();
			builder.Property(c => c.ContractDate)
				.IsRequired();

			builder.Property(c => c.Terms)
				.IsRequired()
				.HasMaxLength(1000);

			builder.Property(c => c.ContractStatus)
				.IsRequired();

			builder.Property(c => c.PenaltyRatePerHour)
				.HasPrecision(10, 2)
				.IsRequired();

			builder.Property(c => c.MaxPenaltyPercent)
				.HasPrecision(5, 2)
				.IsRequired();

			builder.Property(c => c.ValidUntil)
				.IsRequired();

			// FK: User (Client)
			builder.HasOne(c => c.Client)
				.WithMany()
				.HasForeignKey(c => c.ClientId)
				.OnDelete(DeleteBehavior.Restrict);

			// FK: Company (Forwarder)
			builder.HasOne(c => c.Forwarder)
				.WithMany()
				.HasForeignKey(c => c.ForwarderId)
				.OnDelete(DeleteBehavior.Restrict);

			// FK: Route
			builder.HasOne(c => c.Route)
				.WithMany(r => r.Contracts)
				.HasForeignKey(c => c.RouteId)
				.OnDelete(DeleteBehavior.Restrict);

			// 1:1 Payment
			builder.HasOne(c => c.Payment)
				.WithOne(p => p.Contract)
				.HasForeignKey<Payment>(p => p.ContractId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
