using FluentValidation;
using Movies.Application.Models;

namespace Movies.Application.Validators;

public class GetAllMoviesOptionsValidator : AbstractValidator<GetAllMoviesOptions>
{
  public GetAllMoviesOptionsValidator()
  {
    RuleFor(o=>o.YearOfRelease)
      .LessThanOrEqualTo(DateTime.UtcNow.Year)
      .WithMessage("Year cannot be more the present date !");
  }
}
