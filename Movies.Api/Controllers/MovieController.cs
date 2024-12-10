using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
public class MovieController(IMovieService movieService) : ControllerBase
{
  [Authorize(AuthConstance.TrustedUserPolicyName)]
  [HttpPost(ApiEndpoints.Movies.Create)]
  [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ValidationFailureResponse),StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> CreateAsync(CreateMovieRequest request, CancellationToken cancellationToken)
  {
    var movie = request.MapToMovie();
    var result = await movieService.CreateAsync(movie, cancellationToken);
    if (!result)
    {
      return BadRequest();
    }

    var response = movie.MapToMovieResponse();

    return CreatedAtAction("Get", new { idOrSlug = movie.Id }, response);
  }

  [HttpGet(ApiEndpoints.Movies.Get)]
  // [ResponseCache(Duration = 60,VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Any)]
  [OutputCache]
  [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> GetAsync([FromRoute] string idOrSlug, CancellationToken cancellationToken)
  {
    var userId = HttpContext.GetUserId();
    var movie = Guid.TryParse(idOrSlug, out var id)
      ? await movieService.GetByIdAsync(id, userId, cancellationToken)
      : await movieService.GetBySlugAsync(idOrSlug, userId, cancellationToken);
    if (movie == null)
    {
      return NotFound();
    }

    var response = movie.MapToMovieResponse();
    return Ok(response);
  }

  [Authorize(AuthConstance.TrustedUserPolicyName)]
  [HttpPut(ApiEndpoints.Movies.Update)]
  [ProducesResponseType(typeof(MovieResponse),StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ValidationFailureResponse),StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, UpdateMovieRequest request,
    CancellationToken cancellationToken)
  {
    var userId = HttpContext.GetUserId();
    var movie = request.MapToMovie(id);
    var result = await movieService.UpdateAsync(movie, userId, cancellationToken);
    if (result == null)
    {
      return BadRequest();
    }

    var response = movie.MapToMovieResponse();
    return Ok(response);
  }

  [HttpGet(ApiEndpoints.Movies.GetAll)]
  // [ResponseCache(Duration = 60, VaryByQueryKeys = ["title","yearofrelease","sortby","page","pagesize"],Location = ResponseCacheLocation.Any)]
  [OutputCache]
  [ProducesResponseType(typeof(MoviesResponse),StatusCodes.Status200OK)]
  public async Task<IActionResult> GetAllAsync([FromQuery] GetAllMoviesRequest request,
    CancellationToken cancellationToken)
  {
    var userId = HttpContext.GetUserId();
    var options = request.MapToOptions()
      .WithUser(userId);
    var movies = await movieService.GetAllAsync(options, cancellationToken);
    var movieCount = await movieService.GetCountAsync(options.Title, options.YearOfRelease, cancellationToken);
    var response = movies.MapToMoviesResponse(request.Page, request.PageSize, movieCount);
    return Ok(response);
  }

  [Authorize(AuthConstance.AdminUserPolicyName)]
  [HttpDelete(ApiEndpoints.Movies.Delete)]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
  {
    var result = await movieService.DeleteAsync(id, cancellationToken);
    if (!result)
    {
      return NotFound();
    }

    return Ok();
  }
}
