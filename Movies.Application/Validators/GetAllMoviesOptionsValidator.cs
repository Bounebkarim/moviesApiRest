using FluentValidation;
using Movies.Application.Models;

namespace Movies.Application.Validators;

public class GetAllMoviesOptionsValidator : AbstractValidator<GetAllMoviesOptions>
{
  private static readonly string[] _acceptableSortFields = {"title", "yearofrelease"};

  public GetAllMoviesOptionsValidator()
  {
    RuleFor(o=>o.YearOfRelease)
      .LessThanOrEqualTo(DateTime.UtcNow.Year)
      .WithMessage("Year cannot be more the present date !");
    RuleFor(x=>x.SortField)
      .Must(x=>x is null || _acceptableSortFields.Contains(x))
      .WithMessage("Must provide a valid sort field !");
    RuleFor(x => x.Page)
      .GreaterThanOrEqualTo(1).WithMessage("Page cannot be less than 1 !");
    RuleFor(x=>x.PageSize)
      .InclusiveBetween(1, 25).WithMessage("PageSize cannot be more than 25 !");
  }
}
