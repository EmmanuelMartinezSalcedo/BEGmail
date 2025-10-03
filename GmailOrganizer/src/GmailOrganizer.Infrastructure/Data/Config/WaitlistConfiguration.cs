using GmailOrganizer.Core.WaitlistAggregate;

namespace GmailOrganizer.Infrastructure.Data.Config;

public class WaitlistConfiguration : IEntityTypeConfiguration<Waitlist>
{
  public void Configure(EntityTypeBuilder<Waitlist> builder)
  {
    builder.Property(w => w.Email)
        .HasMaxLength(255)
        .IsRequired();

    builder.Property(w => w.CreatedAt)
        .IsRequired()
        .HasDefaultValueSql("NOW()");

    builder.HasKey(w => w.Id);
  }
}
