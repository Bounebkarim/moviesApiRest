using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Movies.Api.swagger;

public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IHostEnvironment hostEnvironment)
  : IConfigureOptions<SwaggerGenOptions>
{
  public void Configure(SwaggerGenOptions options)
  {
    foreach (var description in provider.ApiVersionDescriptions)
    {
      options.SwaggerDoc(
        description.GroupName,
        new OpenApiInfo { Title = hostEnvironment.ApplicationName, Version = description.ApiVersion.ToString(), }
      );
    }

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
      {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
      }
    );
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
      {
        new OpenApiSecurityScheme
        {
          Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        },
        Array.Empty<string>()
      }
    });
  }
}
