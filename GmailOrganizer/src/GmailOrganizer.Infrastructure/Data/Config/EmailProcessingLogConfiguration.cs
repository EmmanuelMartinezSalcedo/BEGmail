using GmailOrganizer.Core.UserAggregate;
using GmailOrganizer.Core.UserAggregate.Entities;

namespace GmailOrganizer.Infrastructure.Data.Config;

public class EmailProcessingLogConfiguration : IEntityTypeConfiguration<EmailProcessingLog>
{
  public void Configure(EntityTypeBuilder<EmailProcessingLog> builder)
  {
    builder.HasOne<User>()
      .WithMany(u => u.EmailProcessingLogs)
      .HasForeignKey(e => e.UserId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Property(e => e.UserId)
      .IsRequired();

    builder.Property(e => e.ProcessedAt)
      .IsRequired();

    builder.Property(e => e.LabelAssigned)
      .IsRequired()
      .HasMaxLength(200);
  }
}
