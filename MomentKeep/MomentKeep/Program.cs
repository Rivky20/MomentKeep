using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MomentKeep;
using MomentKeep.Data.Context;
using MomentKeep.Middleware;

// Entry point for the application
var builder = WebApplication.CreateBuilder(args);

// Load user secrets for storing sensitive configuration (connection strings, API keys, etc.)
builder.Configuration.AddUserSecrets<Program>();

// Configure logging - clear existing providers and add console and debug providers
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add controllers to the dependency injection container
builder.Services.AddControllers();

// Configure Entity Framework with PostgreSQL database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(3),
            errorCodesToAdd: null)));

// Configure AWS S3 service for file storage
builder.Services.AddAWSService<IAmazonS3>(new AWSOptions
{
    Credentials = new BasicAWSCredentials(
        builder.Configuration["AWS:S3:AccessKey"],
        builder.Configuration["AWS:S3:SecretKey"]),
    Region = RegionEndpoint.GetBySystemName(builder.Configuration["AWS:Region"])
});

// Configure JWT authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Register application services, repositories, and external services
builder.Services.RegisterServices();
builder.Services.RegisterRepositories();
builder.Services.RegisterExternalServices(builder.Configuration);

// Configure CORS policies to allow cross-origin requests
builder.Services.AddCors(options =>
{
    // Development policy - allows all origins (not recommended for production)
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });

    // Production policy - allows only specific origins
    options.AddPolicy("AllowReactApp", builder =>
    {
        builder.WithOrigins("https://travel-memories-react.onrender.com") // React app URL
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// Configure Swagger/OpenAPI for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Basic API information
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Travel Memories API",
        Version = "v1",
        Description = "API for Travel Memories application",
        Contact = new OpenApiContact
        {
            Name = "Development Team",
            Email = "dev@travelmemories.com"
        }
    });

    // Configure Swagger to use JWT authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Show detailed error page in development
    app.UseDeveloperExceptionPage();

    // Configure Swagger UI for development environment
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "swagger/{documentName}/swagger.json";
    });

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Travel Memories API V1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
    });

    // Automatically apply database migrations in development
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var migrationLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            migrationLogger.LogInformation("Attempting to apply migrations...");
            dbContext.Database.Migrate();
            migrationLogger.LogInformation("Migrations applied successfully.");
        }
        catch (Exception ex)
        {
            var errorLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            errorLogger.LogError(ex, "An error occurred while applying migrations.");
        }
    }
}
else
{
    // Use custom exception handler middleware in production
    app.UseMiddleware<ExceptionMiddleware>();
}

// Configure middleware pipeline in the correct order
app.UseHttpsRedirection();  // Redirect HTTP requests to HTTPS
app.UseStaticFiles();       // Serve static files (e.g., images, CSS, JavaScript)
app.UseRouting();           // Enable endpoint routing

// Apply CORS policy
app.UseCors("AllowAllOrigins");

// IMPORTANT: Authentication middleware must come before Authorization
app.UseAuthentication();    // Handle authentication
app.UseMiddleware<JwtMiddleware>();  // Custom JWT middleware for additional processing
app.UseAuthorization();     // Handle authorization

// Map controller endpoints
app.MapControllers();

// Log application started and provide Swagger URL
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application started successfully. Swagger should be available at /swagger");

// Start the application
app.Run();