namespace Movies.Api;

public static class ApiEndpoints
{
  private const string BaseApi = "api";

  public static class Movies
    {
      private const string Base = $"{BaseApi}/movies";
      public const string Create = $"{Base}";
      public const string Get = $"{Base}/{{idOrSlug}}";
      public const string GetAll = $"{Base}";
      public const string Update = $"{Base}/{{id:guid}}";
      public const string Delete = $"{Base}/{{id:guid}}";

      public const string Rate = $"{Base}/{{id:guid}}/ratings";
      public const string DeleteRating = $"{Base}/{{id:guid}}/ratings";
    }

  public static class Ratings
    {
      private const string Base = $"{BaseApi}/ratings";
      public const string GetUserRatings = $"{Base}/me";
    }
}
