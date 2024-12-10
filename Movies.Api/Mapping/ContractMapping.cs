using Movies.Application.Models;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Mapping;

public static class ContractMapping
{
  public static Movie MapToMovie(this CreateMovieRequest createMovieRequest)
  {
    return new Movie()
    {
      Id = Guid.NewGuid(),
      Title = createMovieRequest.Title,
      YearOfRelease = createMovieRequest.YearOfRelease,
      Genres = createMovieRequest.Genres.ToList()
    };
  }

  public static Movie MapToMovie(this UpdateMovieRequest updateMovieRequest, Guid movieId)
  {
    return new Movie()
    {
      Id = movieId,
      Title = updateMovieRequest.Title,
      YearOfRelease = updateMovieRequest.YearOfRelease,
      Genres = updateMovieRequest.Genres.ToList()
    };
  }

  public static MovieResponse MapToMovieResponse(this Movie movie)
  {
    return new MovieResponse()
    {
      Id = movie.Id,
      Title = movie.Title,
      Slug = movie.Slug,
      YearOfRelease = movie.YearOfRelease,
      Rating = movie.GeneralRating,
      UserRating = movie.Rating,
      Genres = movie.Genres.ToList()
    };
  }

  public static MoviesResponse MapToMoviesResponse(this IEnumerable<Movie> movies, int? requestPage,
    int? requestPageSize, int totalCount)
  {
    var listOfResponses = movies.Select(MapToMovieResponse).ToList();
    return new MoviesResponse()
    {
      Movies = listOfResponses,
      Page = requestPage,
      PageSize = requestPageSize,
      Total = totalCount
    };
  }

  public static MovieRatingResponse MapToMovieRatingResponse(this MovieRating movie)
  {
    return new MovieRatingResponse() { MovieId = movie.MovieId, MovieSlug = movie.MovieSlug, Rating = movie.Rating };
  }

  public static List<MovieRatingResponse> MapToMovieRatingResponse(this IEnumerable<MovieRating> movieRatings)
  {
    var listOfResponse = movieRatings.Select(MapToMovieRatingResponse).ToList();
    return listOfResponse;
  }

  public static GetAllMoviesOptions MapToOptions(this GetAllMoviesRequest request)
  {
    return new GetAllMoviesOptions()
    {
      Title = request.Title,
      YearOfRelease = request.YearOfRelease,
      SortField = request.SortBy?.Trim('+', '-'),
      SortOrder = request.SortBy is null ? SortOrder.Unsorted :
        request.SortBy.StartsWith('-') ? SortOrder.Descending : SortOrder.Ascending,
      Page = request.Page,
      PageSize = request.PageSize
    };
  }

  public static GetAllMoviesOptions WithUser(this GetAllMoviesOptions options, Guid? userId)
  {
    options.UserId = userId;
    return options;
  }
}
