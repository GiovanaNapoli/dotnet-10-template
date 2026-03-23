using System;
using Domain.Interfaces;
using Domain.Interfaces.Common;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Infrastructure.Connector;
using Infrastructure.Context;
using Infrastructure.Events;
using Infrastructure.Identity;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                options.UseSqlServer(connectionString);
            });

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.Password.RequireDigit = true;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = false;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            System.Text.Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!)),
                        ValidateIssuer = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = configuration["Jwt:Audience"],
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            // Repositories
            services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
            services.AddRepositories();

            // UnitOfWork e outros
            services.AddScoped<DbConnector>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            services.AddScoped<IEmailService, EmailService>();

            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Aponta direto para o assembly da Infrastructure — determinístico
            var assembly = typeof(DependencyInjection).Assembly;

            var repositoryTypes = assembly.GetTypes()
                .Where(t => t.Name.EndsWith("Repository") && t.IsClass && !t.IsAbstract)
                .ToList();

            foreach (var implementationType in repositoryTypes)
            {
                var interfaceType = implementationType.GetInterface($"I{implementationType.Name}");

                if (interfaceType != null)
                    services.AddScoped(interfaceType, implementationType);
            }

            return services;
        }
    }
}
