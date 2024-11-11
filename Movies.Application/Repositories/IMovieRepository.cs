using Movies.Application.Models;

namespace Movies.Application.Repositories;

public interface IMovieRepository
{
  Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default);
  Task<Movie?> GetByIdAsync(Guid id, Guid? userId, CancellationToken cancellationToken = default);
  Task<Movie?> GetBySlugAsync(string slug, Guid? userId, CancellationToken cancellationToken = default);
  Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken cancellationToken = default);
  Task<bool> UpdateAsync(Movie movie, CancellationToken cancellationToken = default);
  Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
  Task<bool> ExistByIdAsync(Guid id , CancellationToken cancellationToken = default);
}
