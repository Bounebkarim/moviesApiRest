using Movies.Application.Models;

namespace Movies.Application.Repositories;

public interface IRatingRepository
{
  Task<bool> RateMovieAsync(Guid movieId, int rating, Guid userId, CancellationToken cancellationToken);
  Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken);
  Task<float?> GetRatingAsync(Guid movieId , CancellationToken cancellationToken);
  Task<(float? GeneralRating, int? Rating)> GetRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken);
  Task<IEnumerable<MovieRating>> GetMovieRatingsAsync(Guid userId, CancellationToken cancellationToken);
}
