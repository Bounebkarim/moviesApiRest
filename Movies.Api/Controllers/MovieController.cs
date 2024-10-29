using Microsoft.AspNetCore.Mvc;
using Movies.Api.Mapping;
using Movies.Application.Repositories;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers;

[ApiController]
public class MovieController(IMovieService movieService) : ControllerBase
{
    [HttpPost(ApiEndpoints.Movies.Create)]
    public async Task<IActionResult> Create(CreateMovieRequest request,CancellationToken cancellationToken)
    {
        var movie = request.MapToMovie();
        var result = await movieService.CreateAsync(movie,cancellationToken);
        if (!result)
        {
            return BadRequest();
        }
        var response = movie.MapToMovieResponse();
        
        return CreatedAtAction("Get", new { idOrSlug = movie.Id }, response);
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    public async Task<IActionResult> Get([FromRoute] string idOrSlug,CancellationToken cancellationToken)
    {
        var movie = Guid.TryParse(idOrSlug, out var id) ? 
                                  await movieService.GetByIdAsync(id,cancellationToken) : 
                                  await movieService.GetBySlugAsync(idOrSlug,cancellationToken);
        if (movie == null)
        {
            return NotFound();
        }
        var response = movie.MapToMovieResponse();
        return Ok(response);
    }

    [HttpPut(ApiEndpoints.Movies.Update)]
    public async Task<IActionResult> Update([FromRoute] Guid id,UpdateMovieRequest request,CancellationToken cancellationToken)
    {
        var movie = request.MapToMovie(id);
        var result = await movieService.UpdateAsync(movie,cancellationToken);
        if (result==null)
        {
            return BadRequest();
        }
        var response = movie.MapToMovieResponse();
        return Ok(response);
    }

    [HttpGet(ApiEndpoints.Movies.GetAll)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var movies = await movieService.GetAllAsync(cancellationToken);
        var response = movies.MapToMovieResponse();
        return Ok(response);
    }

    [HttpDelete(ApiEndpoints.Movies.Delete)]
    public async Task<IActionResult> Delete(Guid id,CancellationToken cancellationToken)
    {
        var result = await movieService.DeleteAsync(id,cancellationToken);
        if (!result)
        {
            return NotFound();
        }

        return Ok();
    }
}