using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Postgresql.Persistence.Configurations
{
	internal class UserConfiguration : IEntityTypeConfiguration<User>
	{
		public void Configure(EntityTypeBuilder<User> builder)
		{

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id);

			builder.Property(x => x.Email)
				.IsRequired()
				.HasMaxLength(255);

			builder.Property(x => x.PasswordHash)
				.IsRequired()
				.HasMaxLength(512);

			builder.Property(x => x.FirstName)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(x => x.LastName)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(x => x.PhoneNumber)
				.IsRequired()
				.HasMaxLength(20);

			builder.Property(x => x.UserRole)
				.IsRequired();

			builder.Property(x => x.CreatedAt)
				.IsRequired();

			builder.Property(x => x.LastLogin)
				.IsRequired(false);

			builder.Property(x => x.EmailVerified)
				.IsRequired();

			builder.Property(x => x.UserStatus)
				.IsRequired();

			builder.Property(x => x.CompanyId)
				.IsRequired(false);
			builder.HasCheckConstraint("CK_User_CompanyId_Role", @"
					(""UserRole"" = 1 AND ""CompanyId"" IS NULL) OR
					(""UserRole"" IN (2, 3) AND ""CompanyId"" IS NOT NULL) OR
					(""UserRole"" = 4 AND ""CompanyId"" IS NULL)
				");


			// Relacije

			// User (M) : Company (1) optional
			builder.HasOne(x => x.Company)
				.WithMany(c => c.Users)
				.HasForeignKey(x => x.CompanyId)
				.OnDelete(DeleteBehavior.SetNull);

			// RefreshTokens (1:N)
			builder.HasMany(x => x.RefreshTokens)
				.WithOne(rt => rt.User)
				.HasForeignKey(rt => rt.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			// VehicleMaintenancesReported (1:N)
			builder.HasMany(x => x.VehicleMaintenancesReported)
				.WithOne(vm => vm.ReportedByUser)
				.HasForeignKey(vm => vm.ReportedByUserId)
				.OnDelete(DeleteBehavior.Restrict);

			// ShipmentDocumentsVerified (1:N)
			builder.HasMany(x => x.ShipmentDocumentsVerified)
				.WithOne(sd => sd.VerifiedByUser)
				.HasForeignKey(sd => sd.VerifiedByUserId)
				.OnDelete(DeleteBehavior.Restrict);

			// ShipmentStatusChanges (1:N)
			builder.HasMany(x => x.ShipmentStatusChanges)
				.WithOne(sh => sh.ChangedByUser)
				.HasForeignKey(sh => sh.ChangedByUserId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
