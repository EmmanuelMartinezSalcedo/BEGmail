using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmailOrganizer.Core.UserAggregate.Specifications;
public class UserWithEmailProcessingLogsSpec : Specification<User>
{
  public UserWithEmailProcessingLogsSpec(int userId)
  {
    Query
      .Where(u => u.Id == userId)
      .Include(u => u.EmailProcessingLogs);
  }
}
