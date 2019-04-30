using ImageGallery.IdentityServer.DbContexts;
using ImageGallery.IdentityServer.Services;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImageGallery.IdentityServer.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureAspNetIdentity(this IServiceCollection services, string connectionString, string migrationsAssembly)
        {
            services.AddDbContext<ApplicationDbContext>(builder =>
                builder.UseSqlServer(connectionString,
                    sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)));

            services.AddIdentity<IdentityUser, IdentityRole>(options => {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddTransient<ILoginService<IdentityUser>, EFLoginService>();
        }

        public static void ConfigureIdentityServer(this IServiceCollection services, string connectionString, string migrationsAssembly)
        {
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                //.AddTestUsers(Config.GetUsers())
                .AddAspNetIdentity<IdentityUser>()
                // this adds the config data from DB (clients, resources)
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                    options.TokenCleanupInterval = 30;
                });
            //.AddInMemoryIdentityResources(Config.GetIdentityResources())
            //.AddInMemoryApiResources(Config.GetApiResources())
            //.AddInMemoryClients(Config.GetClients());
        }

        public static void ConfigureMassTransitRabbitMQ(this IServiceCollection services)
        {
            services.AddSingleton(provider => Bus.Factory.CreateUsingRabbitMq(
                cfg =>
                {
                    var host = cfg.Host("192.168.99.100", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });
                }));

            //Register Publish Endpoint of RabbitMQ bus service
            services.AddSingleton<IPublishEndpoint>(provider => provider.GetRequiredService<IBusControl>());
            //Register Send Endpoint of RabbitMQ bus service
            services.AddSingleton<ISendEndpointProvider>(provider => provider.GetRequiredService<IBusControl>());
            //Register Bus control for RabbitMQ
            services.AddSingleton<IBus>(provider => provider.GetRequiredService<IBusControl>());

            //Register Bus Service hosting
            services.AddSingleton<IHostedService, ServiceBus.ServiceBus>();
        }
    }
}
