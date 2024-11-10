namespace Movies.Application.Models;

public class MovieRating
{
  public required Guid MovieId { get; init; }
  public required string MovieSlug { get; init; }
  public required int Rating { get; init; }
}
