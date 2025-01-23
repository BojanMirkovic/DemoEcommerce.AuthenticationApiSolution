using AuthenticationApi.Application.Interfaces;
using AuthenticationApi.Infrastructure.Database;
using AuthenticationApi.Infrastructure.Repositories;
using eCommerce.SharedLibrary.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthenticationApi.Infrastructure.DependenyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection AddInfrastructureService(this IServiceCollection services, IConfiguration config)
        {
            /*Here we have to add Database connectivity and authentication scheme, becouse we have done this in 
             SharedLibrary.DependencyInjection.SharedServiceContainer 
             we dont have to do it again, insted we can just register it in here*/

            // Register shared infrastructure services (database, logging, etc.)
            SharedServiceContainer.AddSharedServices<AuthenticationDbContext>(services, config, config["MySerilog:FileName"]!);

            // Register local infrastructure services like repositories
            services.AddScoped<IUser, UserRepository>();

            return services;
        }
        public static IApplicationBuilder UseInfrastructurePolicy(this IApplicationBuilder app)
        {
            //Register middleware such as GlobalException for handling external errors and Listen to only API Gateway/block all outside calls
            SharedServiceContainer.UseSharedPolicies(app);

            return app;
        }
    }
}
