using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Movies.Api;
using Movies.Api.Mapping;
using Movies.Application;
using Movies.Application.Database;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

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
  x.AddPolicy(AuthConstance.TrustedUserClaimName, policy => policy.RequireAssertion(c =>
    c.User.HasClaim(m=> m is {Type : AuthConstance.AdminUserClaimName,Value: "true"}) ||
    c.User.HasClaim(m=> m is {Type : AuthConstance.TrustedUserClaimName,Value: "true"})));
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
builder.Services.AddApplication();
builder.Services.AddDatabase(builder.Configuration);
var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

app.UseMiddleware<ValidationMappingMiddleware>();
app.MapControllers();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
var dbInitializer = app.Services.GetRequiredService<DbInitializer>();
await dbInitializer.InitializeAsync();

app.Run();

public partial class Program
{}
