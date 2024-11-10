using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers;

[ApiController]
public class MovieController(IMovieService movieService) : ControllerBase
{
  [Authorize(AuthConstance.TrustedUserPolicyName)]
  [HttpPost(ApiEndpoints.Movies.Create)]
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
  public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
  {
    var userId = HttpContext.GetUserId();
    var movies = await movieService.GetAllAsync(userId, cancellationToken);
    var response = movies.MapToMovieResponse();
    return Ok(response);
  }

  [Authorize(AuthConstance.AdminUserPolicyName)]
  [HttpDelete(ApiEndpoints.Movies.Delete)]
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
