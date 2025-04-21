using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TravelMemories.Core.Interfaces;
using TravelMemories.Core.Interfaces.External;
using TravelMemories.Core.Interfaces.Repositories;
using TravelMemories.Data.Repositories;
using TravelMemories.Service.External;
using TravelMemories.Service.Services;

namespace TravelMemories
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings.GetValue<string>("SecretKey");

            services.AddAuthentication(options =>
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
                    ValidIssuer = jwtSettings.GetValue<string>("Issuer"),
                    ValidAudience = jwtSettings.GetValue<string>("Audience"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            return services;
        }

        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            // Register application services
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITripService, TripService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IAIImageService, AIImageService>();

            return services;
        }

        public static IServiceCollection RegisterRepositories(this IServiceCollection services)
        {
            // Register repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITripRepository, TripRepository>();
            services.AddScoped<IImageRepository, ImageRepository>();
            services.AddScoped<IAIImageRepository, AIImageRepository>();
            services.AddScoped<ITagRepository, TagRepository>();

            return services;
        }

        public static IServiceCollection RegisterExternalServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure AWS S3
            var awsOptions = configuration.GetAWSOptions();
            services.AddDefaultAWSOptions(awsOptions);
            services.AddAWSService<IAmazonS3>();
            services.AddScoped<IS3Service, S3Service>();

            // Configure HttpClient for Hugging Face API
            services.AddHttpClient<IHuggingFaceClient, HuggingFaceClient>(client =>
            {
                client.BaseAddress = new Uri(configuration["HuggingFace:BaseUrl"]);
                client.Timeout = TimeSpan.FromSeconds(120); // Longer timeout for image generation
            });

            return services;
        }
    }
}