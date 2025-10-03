using GmailOrganizer.Core.UserAggregate;
using GmailOrganizer.Core.UserAggregate.Entities;

namespace GmailOrganizer.Infrastructure.Data.Config;
public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
  public void Configure(EntityTypeBuilder<Subscription> builder)
  {
    builder.Property(t => t.Id)
      .ValueGeneratedNever();

    builder.HasOne<User>()
      .WithOne(u => u.Subscription!)
      .HasForeignKey<Subscription>(s => s.UserId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Property(s => s.Tier)
      .HasConversion(
        tier => tier.Value,
        value => SubscriptionTier.FromValue(value)
      )
      .IsRequired();

    builder.Property(s => s.Status)
      .HasConversion(
        status => status.Value,
        value => SubscriptionStatus.FromValue(value)
      )
      .IsRequired();

    builder.Property(s => s.StartDate)
      .IsRequired();

    builder.Property(s => s.EndDate)
      .IsRequired();

    builder.Property(s => s.EmailLimit)
      .HasDefaultValue(100);

    builder.Property(s => s.EmailsProcessed)
      .HasDefaultValue(0);
  }
}
