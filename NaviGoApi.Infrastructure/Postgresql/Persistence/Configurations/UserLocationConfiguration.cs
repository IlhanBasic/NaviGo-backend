using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Postgresql.Persistence.Configurations
{
	public class UserLocationConfiguration : IEntityTypeConfiguration<UserLocation>
	{
		public void Configure(EntityTypeBuilder<UserLocation> builder)
		{
			builder.ToTable("UserLocations");

			builder.HasKey(ul => ul.Id);

			builder.Property(ul => ul.IpAddress)
				.IsRequired()
				.HasMaxLength(45);

			builder.Property(ul => ul.Region)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(ul => ul.AccessTime)
				.IsRequired();

			builder.HasOne(ul => ul.User)
				.WithMany(u => u.UserLocations) 
				.HasForeignKey(ul => ul.UserId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
