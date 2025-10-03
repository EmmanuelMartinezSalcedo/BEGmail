namespace GmailOrganizer.Core.UserAggregate.Specifications;
public class UserWithLabelStatsSpec : Specification<User>
{
  public UserWithLabelStatsSpec(int userId)
  {
    Query
      .Where(u => u.Id == userId)
      .Include(u => u.LabelStats); 
  }
}
