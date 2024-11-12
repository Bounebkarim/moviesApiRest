using System.Runtime.Intrinsics.X86;
using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class MovieRepository(IDbConnectionFactory dbConnectionFactory) : IMovieRepository
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken)
  {
    if (movie == null || movie.Id == Guid.Empty || string.IsNullOrWhiteSpace(movie.Title))
    {
      throw new ArgumentException("Movie cannot be null, and it must have a valid Id and Title.");
    }

    using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
    using var transaction = connection.BeginTransaction();

    try
    {
      var result = await connection.ExecuteAsync(
        new CommandDefinition($"""
                               insert into movies(id, slug, title, yearofrelease)
                               values(@Id, @Slug, @Title, @YearOfRelease)
                               """, movie, transaction));

      if (result <= 0)
      {
        throw new Exception("Failed to insert movie.");
      }

      if (movie.Genres.Any())
      {
        foreach (var genre in movie.Genres)
        {
          if (string.IsNullOrWhiteSpace(genre))
          {
            throw new ArgumentException("Genre name cannot be null or empty.");
          }

          await connection.ExecuteAsync(
            new CommandDefinition($"""
                                   insert into genres(movieId, name)
                                   values(@MovieId, @Name)
                                   """, new { MovieId = movie.Id, Name = genre }, transaction,
              cancellationToken: cancellationToken));
        }
      }

      transaction.Commit();
      return true;
    }
    catch (Exception)
    {
      transaction.Rollback();
      throw;
    }
    finally
    {
      if (connection.State != System.Data.ConnectionState.Closed)
      {
        connection.Close();
      }
    }
  }


  public async Task<Movie?> GetByIdAsync(Guid id, Guid? userId, CancellationToken cancellationToken)
  {
    using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

    var sql = @"
    SELECT m.*, ur.rating AS Rating, gr.GeneralRating, g.name AS GenreName
    FROM movies m
    LEFT JOIN genres g ON m.id = g.movieId
    LEFT JOIN ratings ur ON m.id = ur.movieId AND ur.UserId = @UserId
    LEFT JOIN (
        SELECT movieId, AVG(rating) AS GeneralRating
        FROM ratings
        GROUP BY movieId
    ) gr ON m.id = gr.movieId
    WHERE m.id = @Id;";

    var movieDictionary = new Dictionary<Guid, Movie>();

    var movies = await connection.QueryAsync<Movie, string, Movie>(
      sql,
      (movie, genreName) =>
      {
        if (!movieDictionary.TryGetValue(movie.Id, out var currentMovie))
        {
          currentMovie = movie;
          // currentMovie.UserRating = userRating;
          // currentMovie.Rating = generalRating;
          movieDictionary.Add(currentMovie.Id, currentMovie);
        }

        if (!string.IsNullOrEmpty(genreName))
        {
          currentMovie.Genres.Add(genreName);
        }

        return currentMovie;
      },
      new { Id = id, UserId = userId },
      splitOn: "GenreName");

    return movieDictionary.Values.FirstOrDefault();
  }


  public async Task<Movie?> GetBySlugAsync(string slug, Guid? userId, CancellationToken cancellationToken)
  {
    using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

    var sql = """
                      SELECT m.* ,ur.rating AS Rating, gr.GeneralRating,  g.name AS GenreName
              FROM movies m
              LEFT JOIN genres g ON m.id = g.movieId
              LEFT JOIN ratings ur ON m.id = ur.movieId AND ur.UserId = @UserId
              LEFT JOIN (
                  SELECT movieId, AVG(rating) AS GeneralRating
                  FROM ratings
                  GROUP BY movieId
              ) gr ON m.id = gr.movieId
                      WHERE m.slug = @Slug 
              """;

    var movieDictionary = new Dictionary<Guid, Movie>();

    var movies = await connection.QueryAsync<Movie, string,Movie>(
      sql,
      (movie, genreName) =>
      {
        if (!movieDictionary.TryGetValue(movie.Id, out var currentMovie))
        {
          currentMovie = movie;
          // currentMovie.Rating = generalRating;
          // currentMovie.UserRating = userRating;
          movieDictionary.Add(currentMovie.Id, currentMovie);
        }

        if (!string.IsNullOrEmpty(genreName))
        {
          currentMovie.Genres.Add(genreName);
        }

        return currentMovie;
      },
      new { Slug = slug , UserId = userId },
      splitOn: "GenreName");
    return movieDictionary.Values.FirstOrDefault();
  }

  public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken cancellationToken)
  {
    using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

    // Define the order clause if a sort field is provided
    var orderClause = string.Empty;
    if (options.SortField is not null)
    {
      orderClause = $"""
                         ORDER BY m.{options.SortField} {(options.SortOrder == SortOrder.Ascending ? "ASC" : "DESC")}
                     """;
    }
    var result = await connection.QueryAsync(new CommandDefinition(
    $"""
                   SELECT m.*, gr.GeneralRating, ur.rating AS Rating, STRING_AGG(g.name, ',') AS Genres
                   FROM movies m
                   LEFT JOIN genres g ON m.id = g.movieId 
                   LEFT JOIN ratings ur ON m.id = ur.movieId AND ur.UserId = @UserId
                   LEFT JOIN (
                       SELECT movieId, AVG(rating) AS GeneralRating
                       FROM ratings
                       GROUP BY movieId
                   ) gr ON m.id = gr.movieId
                   WHERE (@Title IS NULL OR m.title LIKE ('%' || @Title || '%'))
                   AND (@YearOfRelease IS NULL OR m.yearofrelease = @YearOfRelease)
                   GROUP BY m.id, gr.GeneralRating, ur.rating
                   {orderClause}
                   LIMIT @PageSize OFFSET @Pageoffset;
               """,
      new
      {
        options.UserId,
        options.Title,
        options.YearOfRelease,
        options.PageSize,
        Pageoffset = (options.Page - 1) * options.PageSize
      },cancellationToken:cancellationToken));
    
    return result.Select(x=>new Movie()
    {
      Id = x.id,
      Title = x.title,
      YearOfRelease = x.yearofrelease,
      Rating = x.rating,
      GeneralRating = x.generalRating,
      Genres = Enumerable.ToList(x.genres.Split(','))
    });
  }

  public async Task<bool> UpdateAsync(Movie movie, CancellationToken cancellationToken)
  {
    using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
    using var transaction = connection.BeginTransaction();

    try
    {
      var result = await connection.ExecuteAsync(new CommandDefinition(
        $"""
         UPDATE movies
         SET slug = @Slug, title = @Title, yearofrelease = @YearOfRelease
         WHERE id = @Id
         """, movie, transaction, cancellationToken: cancellationToken));

      if (result <= 0)
      {
        throw new Exception("Failed to update the movie.");
      }

      var existingGenres = await connection.QueryAsync<string>(new CommandDefinition(
        $"""
         SELECT name FROM genres WHERE movieId = @MovieId
         """, new { MovieId = movie.Id }, transaction, cancellationToken: cancellationToken));

      var enumerable = existingGenres as string[] ?? existingGenres.ToArray();
      var genresToRemove = enumerable.Except(movie.Genres).ToList();
      if (genresToRemove.Count != 0)
      {
        foreach (var genre in genresToRemove)
        {
          await connection.ExecuteAsync(new CommandDefinition(
            """
            DELETE FROM genres 
            WHERE movieId = @MovieId AND name = @Name
            """,
            new { MovieId = movie.Id, Name = genre },
            transaction, cancellationToken: cancellationToken));
        }
      }

      var genresToAdd = movie.Genres.Except(enumerable).ToList();
      if (genresToAdd.Any())
      {
        foreach (var genre in genresToAdd)
        {
          await connection.ExecuteAsync(new CommandDefinition(
            $"""
             INSERT INTO genres(movieId, name)
             VALUES (@MovieId, @Name)
             """, new { MovieId = movie.Id, Name = genre },
            transaction, cancellationToken: cancellationToken));
        }
      }

      transaction.Commit();
      return true;
    }
    catch (Exception)
    {
      transaction.Rollback();
      throw;
    }
    finally
    {
      if (connection.State != System.Data.ConnectionState.Closed)
      {
        connection.Close();
      }
    }
  }


  public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
  {
    using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
    using var transaction = connection.BeginTransaction();

    try
    {
      await connection.ExecuteAsync(new CommandDefinition(
        $"""
         DELETE FROM genres WHERE movieId = @MovieId
         """, new { MovieId = id }, transaction, cancellationToken: cancellationToken));
      
      await connection.ExecuteAsync(new CommandDefinition(
        """
                    DELETE FROM ratings WHERE movieId = @MovieId
                   """,id, cancellationToken: cancellationToken));
      
      var result = await connection.ExecuteAsync(new CommandDefinition(
        $"""
         DELETE FROM movies WHERE id = @Id
         """, new { Id = id }, transaction, cancellationToken: cancellationToken));


      if (result <= 0)
      {
        throw new Exception("Failed to delete the movie.");
      }

      transaction.Commit();
      return true;
    }
    catch (Exception)
    {
      transaction.Rollback();
      throw;
    }
    finally
    {
      if (connection.State != System.Data.ConnectionState.Closed)
      {
        connection.Close();
      }
    }
  }


  public async Task<bool> ExistByIdAsync(Guid id, CancellationToken token)
  {
    using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
    var exists = await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
      $"""
       SELECT COUNT(1) FROM movies WHERE id = @Id
       """, new { Id = id }, cancellationToken: token));
    return exists;
  }

  public async Task<int> GetCountAsync(string? title, int? yearOfRelease, CancellationToken cancellationToken = default)
  {
    using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
    return await connection.QuerySingleAsync<int>(new CommandDefinition("""
                                                                  SELECT COUNT(*) FROM movies 
                                                                  WHERE 
                                                                    @title is null OR title LIKE ('%' || @Title ||'%')
                                                                    AND
                                                                    @yearOfRelease is null OR yearofrelease = @YearOfRelease
                                                                  """,new { title, yearOfRelease }, cancellationToken: cancellationToken));
  }
}
