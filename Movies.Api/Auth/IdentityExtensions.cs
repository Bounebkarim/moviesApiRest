
namespace Movies.Api.Auth;

public static class IdentityExtensions
{
  public static Guid? GetUserId(this HttpContext context)
  {
    var userId = context.User.Claims.FirstOrDefault(c => c.Type == "userid")?.Value;
    if (Guid.TryParse(userId, out Guid parsedUserId))
    {
      return parsedUserId;
    }
    return null;
  }
}
