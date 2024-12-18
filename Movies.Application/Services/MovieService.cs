﻿using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Services;

public class MovieService(IMovieRepository movieRepository,IValidator<Movie> validator, IRatingRepository ratingRepository, IValidator<GetAllMoviesOptions> optionsValidator) :IMovieService
{
  private readonly IMovieRepository _movieRepository = movieRepository;
  private readonly IValidator<GetAllMoviesOptions> _optionsValidator = optionsValidator;
  private readonly IRatingRepository _ratingRepository = ratingRepository;
  private readonly IValidator<Movie> _validator = validator;

  public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(movie, cancellationToken: cancellationToken);
        return await _movieRepository.CreateAsync(movie, cancellationToken);
    }

  public Task<Movie?> GetByIdAsync(Guid id, Guid? userId, CancellationToken cancellationToken)
    {
        return _movieRepository.GetByIdAsync(id,userId, cancellationToken);
    }

  public Task<Movie?> GetBySlugAsync(string slug, Guid? userId, CancellationToken cancellationToken)
    {
        return _movieRepository.GetBySlugAsync(slug,userId, cancellationToken);
    }

  public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken cancellationToken)
    {
        await _optionsValidator.ValidateAndThrowAsync(options, cancellationToken);
        return await _movieRepository.GetAllAsync(options,cancellationToken);
    }

  public async Task<Movie?> UpdateAsync(Movie movie, Guid? userId, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(movie, cancellationToken: cancellationToken);
        var movieExist = await _movieRepository.ExistByIdAsync(movie.Id, cancellationToken);
        if (!movieExist)
        {
            return null;
        }

        if (!userId.HasValue)
        {
          var rating = await  _ratingRepository.GetRatingAsync(movie.Id, cancellationToken);
          movie.GeneralRating = rating;
          return movie;
        }
        
        await _movieRepository.UpdateAsync(movie, cancellationToken);
        var ratings = await  _ratingRepository.GetRatingAsync(movie.Id,userId.Value, cancellationToken);
        movie.Rating = ratings.Rating;
        movie.GeneralRating = ratings.GeneralRating;
        return movie;
    }

  public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        return _movieRepository.DeleteAsync(id, cancellationToken);
    }

  public Task<int> GetCountAsync(string? title, int? yearOfRelease, CancellationToken cancellationToken = default)
  {
    return _movieRepository.GetCountAsync(title, yearOfRelease, cancellationToken);
  }
}
