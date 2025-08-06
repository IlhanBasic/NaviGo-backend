using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Postgresql.Persistence.Configurations
{
	internal class ShipmentDocumentConfiguration : IEntityTypeConfiguration<ShipmentDocument>
	{
		public void Configure(EntityTypeBuilder<ShipmentDocument> builder)
		{

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id);

			builder.Property(x => x.ShipmentId)
				.IsRequired();

			builder.Property(x => x.DocumentType)
				.IsRequired();

			builder.Property(x => x.FileUrl)
				.IsRequired()
				.HasMaxLength(2048);

			builder.Property(x => x.UploadDate)
				.IsRequired();

			builder.Property(x => x.Verified)
				.IsRequired();

			builder.Property(x => x.VerifiedByUserId)
				.IsRequired(false);

			builder.Property(x => x.ExpiryDate)
				.IsRequired(false);

			// Relacije

			// ShipmentDocument (M) : Shipment (1)
			builder.HasOne(x => x.Shipment)
				.WithMany(s => s.ShipmentDocuments)
				.HasForeignKey(x => x.ShipmentId)
				.OnDelete(DeleteBehavior.Cascade);

			// ShipmentDocument (M) : User (1) (VerifiedByUser)
			builder.HasOne(x => x.VerifiedByUser)
				.WithMany()
				.HasForeignKey(x => x.VerifiedByUserId)
				.OnDelete(DeleteBehavior.SetNull);
		}
	}
}
