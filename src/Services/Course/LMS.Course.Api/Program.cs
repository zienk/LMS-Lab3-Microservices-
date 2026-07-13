using LMS.Course.Application.Services;
using LMS.Course.Application.Validators;
using LMS.Course.Domain.Interfaces;
using LMS.Course.Infrastructure;
using LMS.Course.Infrastructure.Repositories;
using LMS.Course.Infrastructure.Services;
using LMS.Course.GrpcClient.Services;
using LMS.Student.Grpc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using LMS.Course.Api.Middleware;
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
    // Content negotiation
    options.RespectBrowserAcceptHeader = true;
    options.ReturnHttpNotAcceptable = true;
}).AddXmlSerializerFormatters();

// DbContext
builder.Services.AddDbContext<CourseDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Repositories & Services
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<ISemesterRepository, SemesterRepository>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<ICourseService, CourseAppService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateEnrollmentRequestValidator>();

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

// gRPC Client Configuration
var studentGrpcUrl = builder.Configuration["GrpcSettings:StudentServiceUrl"] ?? "http://localhost:6001";
builder.Services.AddGrpcClient<StudentGrpcService.StudentGrpcServiceClient>(o =>
{
    o.Address = new Uri(studentGrpcUrl);
});
builder.Services.AddScoped<IStudentGrpcClient, StudentGrpcClientImpl>();

// JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? "super-secret-key-min-32-characters!!";
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
        Title = "Course Service API",
        Version = "v1",
        Description = "Quản lý khóa học, môn học, học kỳ và ghi danh. Flow ghi danh kiểm tra sinh viên qua gRPC Student Service."
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
        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"Course Service API {description.GroupName.ToUpperInvariant()}");
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
            var db = scope.ServiceProvider.GetRequiredService<CourseDbContext>();

            if (db.Database.GetMigrations().Any())
                await db.Database.MigrateAsync();
            else
                await db.Database.EnsureCreatedAsync();

            return;
        }
        catch when (attempt < maxAttempts)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}
