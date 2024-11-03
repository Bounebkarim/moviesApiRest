using System.Text.RegularExpressions;

namespace Movies.Application.Models;

public partial class Movie
{
  public required Guid Id { get; init; }
  public required string Title { get; init; }
  public required int  YearOfRelease { get; init; }
  public string Slug => GenerateSlug();
  public required List<string> Genres { get; init; } = new();

  private string GenerateSlug()
    {
        var slugTitle = MyRegex().Replace(Title, string.Empty)
                                  .ToLower()
                                  .Replace(" ","-")
                                  .TrimEnd('-');
        return $"{slugTitle}-{YearOfRelease}";
    }

  [GeneratedRegex(@"[^A-Za-z0-9\s-]",RegexOptions.NonBacktracking,5)]
    private static partial Regex MyRegex();
}
