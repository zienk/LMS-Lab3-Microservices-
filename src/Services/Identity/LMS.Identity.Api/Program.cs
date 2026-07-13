using LMS.Identity.Application.Services;
using LMS.Identity.Application.Validators;
using LMS.Identity.Domain.Interfaces;
using LMS.Identity.Infrastructure;
using LMS.Identity.Infrastructure.Repositories;
using LMS.Identity.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using LMS.Identity.Api.Middleware;
using LMS.Identity.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader = true;
    options.ReturnHttpNotAcceptable = true;
}).AddXmlSerializerFormatters();

// DbContext
builder.Services.AddDbContext<IdentityDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.ApiVersionReader = Microsoft.AspNetCore.Mvc.Versioning.ApiVersionReader.Combine(
        new Microsoft.AspNetCore.Mvc.Versioning.UrlSegmentApiVersionReader(),
        new Microsoft.AspNetCore.Mvc.Versioning.HeaderApiVersionReader("x-api-version"),
        new Microsoft.AspNetCore.Mvc.Versioning.MediaTypeApiVersionReader("x-api-version")
    );
});
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new Exception("JWT Key not configured.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "LMS.Identity";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "LMS.Client";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
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

// Swagger with JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Identity Service API",
        Version = "v1",
        Description = "Quản lý đăng ký, đăng nhập, JWT access token, refresh token và logout cho hệ thống LMS."
    });
    c.DocInclusionPredicate((documentName, apiDescription) =>
    {
        var relativePath = apiDescription.RelativePath ?? string.Empty;
        return relativePath.StartsWith($"api/{documentName}/", StringComparison.OrdinalIgnoreCase);
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

await InitializeDatabaseAsync(app.Services);

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging();

app.UseSwagger();
var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
app.UseSwaggerUI(options =>
{
    foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"Identity Service API {description.GroupName.ToUpperInvariant()}");
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

static async Task InitializeDatabaseAsync(IServiceProvider services)
{
    const int maxAttempts = 10;

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

            if (db.Database.GetMigrations().Any())
                await db.Database.MigrateAsync();
            else
                await db.Database.EnsureCreatedAsync();

            if (!await db.Users.AnyAsync(u => u.Username == "admin"))
            {
                db.Users.Add(new User
                {
                    Username = "admin",
                    Email = "admin@lms.edu.vn",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                });

                await db.SaveChangesAsync();
            }

            return;
        }
        catch when (attempt < maxAttempts)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}
