using GmailOrganizer.Core.Services;
using GmailOrganizer.UseCases.Contributors.Create;
using GmailOrganizer.Web.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var logger = Log.Logger = new LoggerConfiguration()
  .Enrich.FromLogContext()
  .WriteTo.Console()
  .CreateLogger();

logger.Information("Starting web host");

builder.AddLoggerConfigs();

var appLogger = new SerilogLoggerFactory(logger)
    .CreateLogger<Program>();

builder.Services.AddOptionConfigs(builder.Configuration, appLogger, builder);
builder.Services.AddServiceConfigs(appLogger, builder);
builder.Services.AddScoped<IGmailService, GmailService>();
builder.Services.AddScoped<IGmailClassificationService, GmailClassificationService>();
builder.Services.AddScoped<IUserEmailClassifier, UserEmailClassifier>();
builder.Services.AddHostedService<GmailClassificationBackgroundService>();


// ---------------------- JWT Authentication ----------------------
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "GmailOrganizer";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "GmailOrganizerUsers";

builder.Services.AddAuthentication(options =>
{
  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = jwtIssuer,
    ValidAudience = jwtAudience,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
  };
});
builder.Services.AddAuthorization();
// ---------------------------------------------------------------

builder.Services.AddFastEndpoints()
                .SwaggerDocument(o =>
                {
                  o.ShortSchemaNames = true;
                })
                .AddCommandMiddleware(c =>
                {
                  c.Register(typeof(CommandLogger<,>));
                });

// wire up commands
//builder.Services.AddTransient<ICommandHandler<CreateContributorCommand2,Result<int>>, CreateContributorCommandHandler2>();

builder.AddServiceDefaults();

builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowAngular", policy =>
  {
    policy.WithOrigins("http://localhost:4200") // 👈 tu frontend Angular
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials();
  });
});

var app = builder.Build();


app.UseCors("AllowAngular");
// ---------------------- Enable Authentication & Authorization ----------------------
app.UseAuthentication();
app.UseAuthorization();
// -------------------------------------------------------------------------------

await app.UseAppMiddlewareAndSeedDatabase();

app.Run();

// Make the implicit Program.cs class public, so integration tests can reference the correct assembly for host building
public partial class Program { }
