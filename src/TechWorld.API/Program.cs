using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi;
using Serilog;
using TechWorld.API.Middleware;
using TechWorld.Application;
using TechWorld.Infrastructure;
using TechWorld.Infrastructure.Identity;
using TechWorld.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TechWorld API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT obtido no login."
    });
    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", doc), new List<string>() }
    });
});

builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>()
        ?? ["http://localhost:5173", "http://localhost:3000", "http://localhost:8080", "http://localhost"];

    options.AddPolicy("FrontendPolicy", policy =>
        policy.WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim("isAdmin", "true"));
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", o =>
    {
        o.Window = TimeSpan.FromMinutes(1);
        o.PermitLimit = 10;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TechWorld API v1"));

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    await DataSeeder.SeedAsync(db, userManager, configuration);
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
