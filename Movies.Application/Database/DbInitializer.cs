using Dapper;

namespace Movies.Application.Database;

public class DbInitializer(IDbConnectionFactory connection)
{
  private readonly IDbConnectionFactory _connection = connection;

  public async Task InitializeAsync()
    {
        using var cnx = await _connection.CreateConnectionAsync();
        await cnx.ExecuteAsync($"""
                                create table if not exists movies(
                                id UUID primary key,
                                slug Text not null,
                                title Text not null,
                                yearofrelease integer not null);
                                """);
        await cnx.ExecuteAsync("""
                                create unique index concurrently if not exists movies_slug_idx
                                on movies
                                using btree(slug)
                               """);
        await cnx.ExecuteAsync($"""
                                create table if not exists genres (
                                movieId UUID references movies (id),
                                name Text not null 
                                );
                                """);
await cnx.ExecuteAsync($"""
                                create table if not exists ratings (
                                userid uuid,
                                movieId uuid references movies (id),
                                rating Int not null,
                                primary key (userid, movieId)
                                );
                                """);

    }
}
