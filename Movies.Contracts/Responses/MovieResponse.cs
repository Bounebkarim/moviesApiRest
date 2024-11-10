﻿namespace Movies.Contracts.Responses;

public class MovieResponse
{
  public required Guid Id { get; init; }
  public required string Title { get; init; }
  public required string Slug { get; init; }
  public required int YearOfRelease { get; init; }
  public required IEnumerable<string> Genres { get; init; } = Enumerable.Empty<string>();
  public int? UserRating { get; init; }
  public float? Rating { get; init; }
}

public class MoviesResponse
{
  public IEnumerable<MovieResponse> Movies { get; init; } = Enumerable.Empty<MovieResponse>();
}
