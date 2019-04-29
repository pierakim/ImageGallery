using GreenPipes;
using IdentityServer4.AccessTokenValidation;
using ImageGallery.API.Authorization;
using ImageGallery.API.Entities;
using ImageGallery.API.Messages;
using ImageGallery.API.Services;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ImageGallery.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            this.ConfigureMassTransitRabbitMQ(services);

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

            services.AddScoped<IAuthorizationHandler, MustOwnImageHandler>();

            services.AddAuthentication(
                IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "https://localhost:44364/";
                    options.ApiName = "imagegalleryapi";
                    options.ApiSecret = "apisecret";
                });

            // register the DbContext on the container, getting the connection string from
            // appSettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            var connectionString = Configuration["ConnectionStrings:imageGalleryDBConnectionString"];
            services.AddDbContext<GalleryContext>(o => o.UseSqlServer(connectionString));

            // register the repository
            services.AddScoped<IGalleryRepository, GalleryRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env,
            ILoggerFactory loggerFactory, GalleryContext galleryContext)
        {

            //To initialise and seed database with the config from Config.cs
            //Check if botth configuration and application context exist
            //Is it better to do this in Main?
            var GalleryContextExist = (galleryContext.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator).Exists();

            if (!GalleryContextExist)
            {
                InitializeGalleryDatabase(app);
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        // ensure generic 500 status code on fault.
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happened. Try again later.");
                    });
                });
            }

            app.UseAuthentication();

            app.UseStaticFiles();

            AutoMapper.Mapper.Initialize(cfg =>
            {
                // Map from Image (entity) to Image, and back
                cfg.CreateMap<Image, Model.Image>().ReverseMap();

                // Map from ImageForCreation to Image
                // Ignore properties that shouldn't be mapped
                cfg.CreateMap<Model.ImageForCreation, Image>()
                    .ForMember(m => m.FileName, options => options.Ignore())
                    .ForMember(m => m.Id, options => options.Ignore())
                    .ForMember(m => m.OwnerId, options => options.Ignore());

                // Map from ImageForUpdate to Image
                // ignore properties that shouldn't be mapped
                cfg.CreateMap<Model.ImageForUpdate, Image>()
                    .ForMember(m => m.FileName, options => options.Ignore())
                    .ForMember(m => m.Id, options => options.Ignore())
                    .ForMember(m => m.OwnerId, options => options.Ignore());
            });

            AutoMapper.Mapper.AssertConfigurationIsValid();

            app.UseMvc();
        }

        private void InitializeGalleryDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var contextApp = serviceScope.ServiceProvider.GetRequiredService<GalleryContext>();
                contextApp.Database.Migrate();
                contextApp.EnsureSeedDataForContext();
            }
        }

        private void ConfigureMassTransitRabbitMQ(IServiceCollection services)
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
    }
}
