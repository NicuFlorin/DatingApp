using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;

using API.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;


namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {

        public static IServiceCollection AddApplicationServies(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<PresenceTracker>();
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<LogUserActivity>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);
            services.AddDbContext<DataContext>(options =>
            {
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                string connectionString;
                if (env == "Development")
                {


                    options.UseSqlite(config.GetConnectionString("DefaultConnection"));
                }
                else
                {
                    connectionString = Environment.GetEnvironmentVariable("MyDbConnection");
                    options.UseSqlite(config.GetConnectionString("DefaultConnection"));
                }
            });
            return services;
        }
    }
}