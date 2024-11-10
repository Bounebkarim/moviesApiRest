using FluentValidation;
using FluentValidation.Results;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Services;

public class RatingService(IRatingRepository ratingRepository, IMovieRepository movieRepository) : IRatingService
{
  private readonly IMovieRepository _movieRepository = movieRepository;
  private readonly IRatingRepository _ratingRepository = ratingRepository;

  public async Task<bool> RateMovieAsync(Guid movieId, int rating, Guid userId, CancellationToken cancellationToken)
  {
    if (rating is <= 0 or > 5)
    {
      throw new ValidationException(new[]
      {
        new ValidationFailure() { PropertyName = nameof(rating), ErrorMessage = "Rating must be between 1 and 5" }
      });
    }

    var movieExist = await _movieRepository.ExistByIdAsync(movieId, cancellationToken);
    if (!movieExist)
    {
      throw new ValidationException(new[]
      {
        new ValidationFailure() { PropertyName = nameof(movieId), ErrorMessage = "Movie does not exist" }
      });
    }

    return await _ratingRepository.RateMovieAsync(movieId, rating, userId, cancellationToken);
  }

  public async Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default)
  {
     return await _ratingRepository.DeleteRatingAsync(movieId, userId, cancellationToken);
  }

  public async Task<IEnumerable<MovieRating>> GetAllRatingsAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    return await _ratingRepository.GetMovieRatingsAsync(userId, cancellationToken);
  }
}
