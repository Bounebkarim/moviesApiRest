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

    public static Movie MapToMovie(this UpdateMovieRequest updateMovieRequest,Guid movieId)
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
            Genres = movie.Genres.ToList()

        };
    }

    public static List<MovieResponse> MapToMovieResponse(this IEnumerable<Movie> movies)
    {
        var listOfResponses = movies.Select(MapToMovieResponse).ToList();
        return listOfResponses;
    }
}