using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class RatingRepository(IDbConnectionFactory dbConnectionFactory) : IRatingRepository
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<bool> RateMovieAsync(Guid movieId, int rating, Guid userId, CancellationToken cancellationToken)
  {
    using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

    var result = await connection.ExecuteAsync(new CommandDefinition(
      """
      INSERT INTO Ratings (userId,movieId, rating)
      VALUES (@userId, @movieId, @rating)
      ON CONFLICT (userId, movieId) DO UPDATE 
      SET rating = @rating
      """
      , new { userId, movieId, rating }, cancellationToken: cancellationToken));
    return result > 0;
  }

  public async Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken)
  {
    using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
    var result = await connection.ExecuteAsync(new CommandDefinition(
      """
      DELETE FROM Ratings WHERE userId = @userId AND movieId = @movieId
      """, new { userId, movieId }, cancellationToken: cancellationToken));
    return result > 0;
  }

  public async Task<float?> GetRatingAsync(Guid movieId, CancellationToken cancellationToken)
  {
    using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
    return await connection.QuerySingleOrDefaultAsync<float?>(new CommandDefinition(
      """
      SELECT AVG(rating) AS GeneralRating
      FROM rating
      where movieId = @movieId
      """
      , new { movieId }, cancellationToken: cancellationToken));
  }

  public async Task<(float? GeneralRating, int? Rating)> GetRatingAsync(Guid movieId, Guid userId,
    CancellationToken cancellationToken)
  {
    using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
    return await connection.QuerySingleOrDefaultAsync<(float?, int?)>(new CommandDefinition(
      """
      SELECT AVG(rating) AS GeneralRating ,(select rating from rating where movieId = @movieId and userId = @userId) as UserRating
      from rating 
      WHERE movieId = @movieId
      """
      , new { movieId, userId }, cancellationToken: cancellationToken));
  }

  public async Task<IEnumerable<MovieRating>> GetMovieRatingsAsync(Guid userId, CancellationToken cancellationToken)
  {
    using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
    return await connection.QueryAsync<MovieRating>(new CommandDefinition(
      """
      SELECT m.slug as MovieSlug ,r.movieId as MovieId, r.rating as Rating
      FROM ratings r 
      INNER JOIN movies m ON m.Id = r.MovieId
      where r.userid = @userId
      """
      ,new {userId}, cancellationToken: cancellationToken));
  }
}
