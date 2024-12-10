using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Movies.Api;
using Movies.Api.Health;
using Movies.Api.Mapping;
using Movies.Api.swagger;
using Movies.Application;
using Movies.Application.Database;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddHealthChecks()
  .AddCheck<DatabaseHealthCheck>(DatabaseHealthCheck.HealthCheckName);
builder.Services.AddAuthentication(x =>
  {
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
  }
).AddJwtBearer(x =>
{
  x.TokenValidationParameters = new TokenValidationParameters
  {
    IssuerSigningKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException())),
    ValidateIssuerSigningKey = true,
    ValidateLifetime = true,
    ValidIssuer = builder.Configuration["Jwt:Issuer"],
    ValidAudience = builder.Configuration["Jwt:Audience"],
    ValidateIssuer = true,
    ValidateAudience = true,
  };
});
builder.Services.AddAuthorization(x =>
{
  x.AddPolicy(AuthConstance.AdminUserPolicyName, policy => policy.RequireClaim(AuthConstance.AdminUserClaimName, "true"));
  x.AddPolicy(AuthConstance.TrustedUserPolicyName, policy => policy.RequireAssertion(c =>
    c.User.HasClaim(m=> m is {Type : AuthConstance.AdminUserClaimName,Value: "true"}) ||
    c.User.HasClaim(m=> m is {Type : AuthConstance.TrustedUserClaimName,Value: "true"})));
});
builder.Services.AddApiVersioning(options =>
{
  options.AssumeDefaultVersionWhenUnspecified = true;
  options.DefaultApiVersion = ApiVersion.Default;
  options.ReportApiVersions = true;
  options.ApiVersionReader = new MediaTypeApiVersionReader("api-version");
}).AddMvc().AddApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>,ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(x=>x.OperationFilter<SwaggerDefaultValues>());
builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddDatabase(builder.Configuration);
// builder.Services.AddResponseCaching();
builder.Services.AddOutputCache(x =>
{
  x.AddBasePolicy(c => c.Cache());
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(x =>
    {
      foreach (var description in app.DescribeApiVersions())
      {
        x.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName);
      }
    });
}

app.MapHealthChecks("/health");

app.UseMiddleware<ValidationMappingMiddleware>();
app.MapControllers();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
// must add Cors before adding the response caching
// app.UseCors();
// app.UseResponseCaching();
app.UseOutputCache();
var dbInitializer = app.Services.GetRequiredService<DbInitializer>();
await dbInitializer.InitializeAsync();

app.Run();

public partial class Program
{}
