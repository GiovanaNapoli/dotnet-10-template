using System.Text.Json.Serialization;
using Application;
using Infrastructure;
using Infrastructure.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebApi
{
    public static class DependencyInjection
    {
        private const string DefaultCorsPolicy = "AllowSpecificOrigins";

        public static IServiceCollection AddWebApi(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

            services.AddCors(options =>
            {
                options.AddPolicy(DefaultCorsPolicy, policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                });
            });

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddApplication();           // ← MediatR, FluentValidation, Behaviors
            services.AddInfrastructure(configuration); // ← DbContext, Repositories, JWT

            return services;
        }

        public static WebApplication UseApiSettings(this WebApplication app)
        {
            app.UseCors(DefaultCorsPolicy);

            var swaggerEnabled = app.Configuration
                .GetValue<bool?>("Swagger:Enabled") ?? app.Environment.IsDevelopment();

            if (swaggerEnabled)
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication(); // ← JWT lê o token
            app.UseAuthorization();  // ← valida permissões

            app.MapControllers();

            return app;
        }
    }
}