using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Postgresql.Persistence.Configurations
{
	internal class PaymentConfiguration : IEntityTypeConfiguration<Payment>
	{
		public void Configure(EntityTypeBuilder<Payment> builder)
		{

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id);

			builder.Property(x => x.ContractId)
				.IsRequired();

			builder.Property(x => x.Amount)
				.IsRequired()
				.HasColumnType("decimal(18,2)");

			builder.Property(x => x.PaymentDate)
				.IsRequired();

			builder.Property(x => x.PaymentStatus)
				.IsRequired();

			builder.Property(x => x.ReceiptUrl)
				.HasMaxLength(500);

			builder.Property(x => x.ClientId)
				.IsRequired();

			// Relations

			builder.HasOne(x => x.Contract)
				.WithOne(c => c.Payment)
				.HasForeignKey<Payment>(x => x.ContractId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(x => x.Client)
				.WithMany()
				.HasForeignKey(x => x.ClientId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
