using AgroSolutions.Properties.Api.Configurations;
using AgroSolutions.Properties.Api.Middlewares;
using Asp.Versioning;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
builder.Host.UseSerilog();

// Configurar serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configurações customizadas
builder.Services.AddObservability(builder.Configuration, builder.Environment);
builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddAuthenticationConfiguration(builder.Configuration);
builder.Services.AddApiDocumentation();
builder.Services.AddHealthChecksConfiguration(builder.Configuration);
builder.Services.AddRateLimitingConfiguration(builder.Configuration);

// API Versioning
builder
    .Services.AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
    );
});

var app = builder.Build();

// Executar migrações
app.Services.RunMigrations();

// Middlewares
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSerilogRequestLogging();

app.UseApiDocumentation();

app.UseCors("AllowAll");

// Rate Limiter (apenas se habilitado)
var rateLimitingEnabled = builder.Configuration.GetValue("RateLimiting:EnableRateLimiting", true);
if (rateLimitingEnabled)
{
    app.UseRateLimiter();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health checks
app.MapHealthChecks("/health");
app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") }
);
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

app.Run();

// Tornar Program acessível para testes
public partial class Program { }
