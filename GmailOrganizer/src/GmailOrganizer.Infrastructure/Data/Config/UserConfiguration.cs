using GmailOrganizer.Core.UserAggregate;

namespace GmailOrganizer.Infrastructure.Data.Config;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder.Property(u => u.GoogleUserId)
        .HasMaxLength(100)
        .IsRequired();

    builder.Property(u => u.Email)
        .HasMaxLength(255)
        .IsRequired();

    builder.Property(u => u.TokenExpiry)
        .IsRequired();

    // Mapear AccessToken como Owned Entity
    builder.OwnsOne(u => u.AccessToken, at =>
    {
      at.Property(p => p.Value)
        .HasColumnName("AccessToken")
        .HasMaxLength(512)
        .IsRequired();
    });

    // Mapear RefreshToken como Owned Entity
    builder.OwnsOne(u => u.RefreshToken, rt =>
    {
      rt.Property(p => p.Value)
        .HasColumnName("RefreshToken")
        .HasMaxLength(512)
        .IsRequired();
    });
  }
}
