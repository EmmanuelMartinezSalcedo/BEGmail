namespace GmailOrganizer.Core.UserAggregate.Specifications;
public class UserByGoogleIdSpec : Specification<User>
{
  public UserByGoogleIdSpec(string googleUserId)
  {
    Query.Where(u => u.GoogleUserId == googleUserId);
  }
}

public class UserByEmailSpec : Specification<User>
{
  public UserByEmailSpec(string email)
  {
    Query.Where(u => u.Email == email);
  }
}

public class UserExistsByGoogleIdSpec : Specification<User, bool>
{
  public UserExistsByGoogleIdSpec(string googleUserId)
  {
    Query.Where(u => u.GoogleUserId == googleUserId)
         .Select(u => true);
  }
}

public class AllUsersSpec : Specification<User>
{
  public AllUsersSpec()
  {
    Query.OrderBy(u => u.Email);
  }
}
