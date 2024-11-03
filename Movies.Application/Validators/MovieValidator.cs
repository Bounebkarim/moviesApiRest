using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Validators;

public class MovieValidator :AbstractValidator<Movie>
{
  private readonly IMovieRepository _movieRepository;

  public MovieValidator(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
        RuleFor(m => m.Title).NotEmpty().WithMessage("Title is required");
        RuleFor(m=>m.Genres).NotEmpty().WithMessage("Genre is required");
        RuleFor(m=>m.YearOfRelease).LessThanOrEqualTo(DateTime.UtcNow.Year).WithMessage("Year must be equal or less than current date");
        RuleFor(m=>m.Slug).MustAsync(ValidateSlugAsync).WithMessage("Slug is already taken");
    }

  private async Task<bool> ValidateSlugAsync(Movie movie,string slug,CancellationToken cancellationToken)
    {
        var existingMovie = await _movieRepository.GetBySlugAsync(slug, cancellationToken);
        if (existingMovie is not null)
        {
            return existingMovie.Id == movie.Id;
        }
        return existingMovie is null;
    }
}
