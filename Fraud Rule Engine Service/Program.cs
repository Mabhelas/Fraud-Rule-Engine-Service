using Fraud_Rule_Engine_Service.Application.Services;
using Fraud_Rule_Engine_Service.Infrastructure;
using Fraud_Rule_Engine_Service.Repositories;
using Fraud_Rule_Engine_Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddSwaggerGen(c =>
{
    // Register API document so /swagger/v1/swagger.json is produced
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Fraud Rule Engine API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter JWT Bearer token"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// MediatR (scan this assembly)
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// JWT settings - require a signing key for secure startup in production
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var signingKey = jwtSection["SigningKey"];
if (string.IsNullOrWhiteSpace(signingKey))
{
    // Fail-fast: banking deployments must provide signing credentials (use __User Secrets__ / env vars / secret store)
    throw new InvalidOperationException("JwtSettings:SigningKey is not configured. Provide via __User Secrets__ or environment variables before starting.");
}

var issuer = jwtSection["Issuer"] ?? "fraud-issuer";
var audience = jwtSection["Audience"] ?? "fraud-audience";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            RoleClaimType = "roles"
        };

        // Helpful for diagnosing 401s — logs authentication failures
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                Log.Logger.Error(ctx.Exception, "JWT authentication failed");
                return Task.CompletedTask;
            },
            OnMessageReceived = ctx =>
            {
                // Make sure token is being received as expected
                Log.Logger.Debug("JWT received from: {Source}", ctx.Request.Path);
                return Task.CompletedTask;
            },
            OnTokenValidated = ctx =>
            {
                Log.Logger.Debug("JWT validated for subject: {sub}", ctx.Principal?.Identity?.Name);
                return Task.CompletedTask;
            }
        };
    });

// Authorization policies (role or policy-based)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewResults", policy => policy.RequireRole("FraudAnalyst", "Compliance"));
    options.AddPolicy("CanSubmit", policy => policy.RequireRole("Ingestor", "FraudAnalyst"));
});

// Infrastructure configuration and repository registration
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
var mongoConn = builder.Configuration.GetValue<string>("MongoDbSettings:ConnectionString");
if (!string.IsNullOrWhiteSpace(mongoConn))
{
    builder.Services.AddSingleton<IFraudRepository, MongoFraudRepository>();
}
else
{
    // Fallback for local/testing
    builder.Services.AddSingleton<IFraudRepository, InMemoryFraudRepository>();
}

// Application orchestration
builder.Services.AddScoped<IFraudOrchestrator, FraudOrchestrator>();

var app = builder.Build();

// HTTP pipeline
// Enable Swagger if environment is Development or if config/env flag is set
var enableSwagger =
    app.Environment.IsDevelopment()
    || builder.Configuration.GetValue<bool>("EnableSwagger", false)
    || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ENABLE_SWAGGER"));

if (enableSwagger)
{
    app.UseSwagger();
    // Serve Swagger UI at the application root so http://localhost:8080/ opens the UI
    app.UseSwaggerUI(c =>
    {
        // explicitly point UI to the document produced above
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fraud Rule Engine v1");
        c.RoutePrefix = string.Empty;
    });
}

// Only use HTTPS redirection when explicitly enabled or in Development (to avoid redirect loops in container without TLS)
var enableHttpsRedirection = builder.Configuration.GetValue<bool?>("EnableHttpsRedirection") ?? app.Environment.IsDevelopment();
if (enableHttpsRedirection)
{
    app.UseHttpsRedirection();
}

// Important: authentication must run before authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
