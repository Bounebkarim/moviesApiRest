using Microsoft.Extensions.Diagnostics.HealthChecks;
using Movies.Application.Database;

namespace Movies.Api.Health;

public class DatabaseHealthCheck(IDbConnectionFactory dbConnectionFactory, ILogger<DatabaseHealthCheck> logger)
  : IHealthCheck
{
  public const string HealthCheckName = "DatabaseHealthCheck";

  public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
    CancellationToken cancellationToken = new())
  {
    try
    {
      _ = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);
      return HealthCheckResult.Healthy();
    }
    catch (Exception e)
    {
      string message = "Database not healthy !";
      logger.LogError(message, e);
      return HealthCheckResult.Unhealthy(message, e);
    }
  }
}
