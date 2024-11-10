using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers;
[ApiController]
public class RatingsController(IRatingService ratingService) :ControllerBase
{
  private readonly IRatingService _ratingService = ratingService;

  [Authorize]
  [HttpPut(ApiEndpoints.Movies.Rate)]
  public async Task<IActionResult> RateMovieAsync([FromRoute] Guid id,[FromBody] RateMovieRequest request,CancellationToken cancellationToken)
  {
    var userId = HttpContext.GetUserId();
    if (!userId.HasValue)
    {
      return Unauthorized();
    }
    var result = await _ratingService.RateMovieAsync(id,request.Rating,userId.Value , cancellationToken);
    return result ? Ok() : BadRequest();
  }

  [Authorize]
  [HttpDelete(ApiEndpoints.Movies.DeleteRating)]
  public async Task<IActionResult> DeleteRatingAsync([FromRoute] Guid id, CancellationToken cancellationToken)
  {
    var userId = HttpContext.GetUserId();
    if (!userId.HasValue)
    {
      return Unauthorized();
    }
    var result = await _ratingService.DeleteRatingAsync(id,userId.Value , cancellationToken);
    return result ? Ok() : BadRequest();
  }

  [Authorize]
  [HttpGet(ApiEndpoints.Ratings.GetUserRatings)]
  public async Task<IActionResult> GetUserRatingsAsync(CancellationToken cancellationToken)
  {
    var userId = HttpContext.GetUserId();
    if (!userId.HasValue)
    {
      return Unauthorized();
    }
    var result = await _ratingService.GetAllRatingsAsync(userId.Value,cancellationToken);
    var mappedResult = result.MapToMovieRatingResponse();
    return Ok(mappedResult);
  }
}
