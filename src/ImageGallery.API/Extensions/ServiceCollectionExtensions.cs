using GreenPipes;
using IdentityServer4.AccessTokenValidation;
using ImageGallery.API.Authorization;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImageGallery.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureMassTransitRabbitMQ(this IServiceCollection services)
        {
            //DI for SendMessageConsumer
            services.AddScoped<Messages.SendMessageConsumer>();
            services.AddMassTransit(c =>
            {
                c.AddConsumer<Messages.SendMessageConsumer>();
            });

            services.AddSingleton(provider => Bus.Factory.CreateUsingRabbitMq(
                cfg =>
                {
                    var host = cfg.Host("192.168.99.100", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.ReceiveEndpoint(host, "TestQueue", e =>
                    {
                        e.PrefetchCount = 16;
                        e.UseMessageRetry(x => x.Interval(2, 100));
                        e.LoadFrom(provider);

                        EndpointConvention.Map<Messages.SendMessageConsumer>(e.InputAddress);
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

        public static void AddMustOwnImageAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(authorizationOptions =>
            {
                authorizationOptions.AddPolicy(
                    "MustOwnImage",
                    policyBuilder =>
                    {
                        policyBuilder.RequireAuthenticatedUser();
                        policyBuilder.AddRequirements(new MustOwnImageRequirement());
                    });
            });
        }

        public static void AddIS4ApiAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication(
                IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "https://localhost:44364/";
                    options.ApiName = "imagegalleryapi";
                    options.ApiSecret = "apisecret";
                });
        }
    }
}
