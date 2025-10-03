using GmailOrganizer.Core.UserAggregate;
using GmailOrganizer.Core.UserAggregate.Entities;

namespace GmailOrganizer.Infrastructure.Data.Config;

public class LabelStatConfiguration : IEntityTypeConfiguration<LabelStat>
{
  public void Configure(EntityTypeBuilder<LabelStat> builder)
  {
    builder.Property(ls => ls.LabelName)
      .IsRequired()
      .HasMaxLength(200);

    builder.Property(ls => ls.EmailCount)
      .HasDefaultValue(0);

    builder.HasOne<User>()
      .WithMany(u => u.LabelStats)
      .HasForeignKey(ls => ls.UserId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
