using LMS.ApiGateway.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// JWT Authentication
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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Authenticated", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});

// Swagger at the gateway documents proxied routes. YARP still handles the real requests.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LMS API Gateway",
        Version = "v1",
        Description = """
        Tài liệu API tiếng Việt cho cổng vào duy nhất của hệ thống LMS Microservices.

        Gateway dùng YARP để forward request đến Identity, Student và Course Service.
        Các route nghiệp vụ cần JWT sẽ được Gateway kiểm tra trước khi chuyển tiếp.

        Cách test nhanh:
        1. Gọi POST /api/auth/login với tài khoản admin / 123456.
        2. Copy accessToken trong response.
        3. Bấm nút Authorize và dán accessToken.
        4. Gọi các API protected như /api/v1/students hoặc /api/courses/1/enroll.
        """
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Dán accessToken nhận được từ API login. Swagger sẽ tự gửi header Authorization: Bearer {token}."
    });

    options.DocumentFilter<GatewaySwaggerDocumentFilter>();
});

// YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.DocumentTitle = "LMS API Gateway - Swagger";
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "LMS API Gateway v1");
    options.DisplayRequestDuration();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new
{
    success = true,
    message = "API Gateway đang hoạt động.",
    service = "LMS.ApiGateway"
})).AllowAnonymous();

app.MapReverseProxy();

app.Run();
