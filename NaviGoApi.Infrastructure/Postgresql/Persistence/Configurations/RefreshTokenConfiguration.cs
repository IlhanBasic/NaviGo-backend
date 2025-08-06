using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Postgresql.Persistence.Configurations
{
	internal class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
	{
		public void Configure(EntityTypeBuilder<RefreshToken> builder)
		{

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id);

			builder.Property(x => x.UserId)
				.IsRequired();

			builder.Property(x => x.Token)
				.IsRequired();

			builder.Property(x => x.Expires)
				.IsRequired();

			builder.Property(x => x.CreatedAt)
				.IsRequired();

			builder.Property(x => x.Revoked);

			builder.Property(x => x.IsActive)
				.IsRequired();

			// Relation with User
			builder.HasOne(x => x.User)
				.WithMany(u => u.RefreshTokens)
				.HasForeignKey(x => x.UserId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
